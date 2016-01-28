using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Timers;
using WMI.DataClasses;
using Timer = System.Timers.Timer;

namespace WMI.DataProviders
{
	internal abstract class DataProvider : IDisposable
	{
		protected bool _disposed;

		protected static readonly ManagementScope Scope = new ManagementScope("root\\CIMV2");
		protected static readonly EnumerationOptions Options = new EnumerationOptions
		{
			DirectRead = true,
			EnsureLocatable = false,
			EnumerateDeep = false,
			ReturnImmediately = true,
			Rewindable = false
		};

		protected readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		protected readonly Timer _timer = new Timer();

		static DataProvider()
		{
			Scope.Connect();
		}

		protected DataProvider(int updateInterval)
		{
			InitializeTimer(updateInterval);
		}

		private void InitializeTimer(int updateInterval)
		{
			_timer.Interval = updateInterval;
			_timer.AutoReset = false;
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
		}

		protected abstract void UpdateEntities();

		private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			try
			{
				UpdateEntities();
			}
			catch (Exception exception)
			{
				EventLog.WriteEntry("Application", exception.ToString(), EventLogEntryType.Error);
			}
			finally
			{
				_timer.Start();
			}
		}

		public virtual void Dispose()
		{
			if (!_disposed)
			{
				_timer.Stop();
				_timer.Dispose();

				_lock.Dispose();
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}

	internal abstract class DataProvider<T> : DataProvider, IDataProvider<T> where T : NamedObject, new()
	{
		private readonly SortedList<string, T> _data = new SortedList<string, T>();
		private readonly List<SearcherEntity<T>> _searcherEntities = new List<SearcherEntity<T>>();

		protected DataProvider(int updateInterval) 
			: base(updateInterval)
		{
		}

		protected void AddSearcher(string table, PropertySettersDictionary<T> propertiesSetters, string condition = null, bool canAddElements = true, bool canRemoveElements = false)
		{
			var selectQuery = new SelectQuery(table, condition ?? string.Empty, propertiesSetters.Keys.ToArray());
			selectQuery.SelectedProperties.Add("Name");
			var searcherEntity = new SearcherEntity<T>(new ManagementObjectSearcher(Scope, selectQuery, Options), canAddElements,
				canRemoveElements, propertiesSetters);
			_searcherEntities.Add(searcherEntity);
		}

		protected override void UpdateEntities()
		{
			foreach (var searcherEntity in _searcherEntities)
				Update(searcherEntity);
		}

		private void Update(SearcherEntity<T> searcherEntity)
		{
			ISet<string> removedEntities = null;
			if (searcherEntity.CanRemoveElements)
				removedEntities = new HashSet<string>(_data.Keys);

			foreach (ManagementObject obj in searcherEntity.Searcher.Get())
			{
				string name = obj.GetName();
				T current;
				if (!_data.TryGetValue(name, out current))
				{
					if (!searcherEntity.CanAddElements)
						continue;
					current = new T {Name = name};
					AddElement(name, current);
				}

				_lock.WriteLock(() =>
				{
					foreach (var propertiesSetter in searcherEntity.PropertySetters)
					{
						propertiesSetter.Value(current, obj[propertiesSetter.Key]);
					}
				});

				if (!searcherEntity.CanRemoveElements)
					continue;
				removedEntities.Remove(name);
			}

			if (!searcherEntity.CanRemoveElements)
				return;
			foreach (var entity in removedEntities)
			{
				_data.Remove(entity);
			}
		}

		public override void Dispose()
		{
			if (!_disposed)
			{
				_timer.Stop();
				_timer.Dispose();

				_lock.Dispose();
				_searcherEntities.ForEach(entity => entity.Dispose());
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public T this[int index]
		{
			get { return _lock.ReadLock(() => _data.Values[index]); }
		}

		public T GetByName(string name)
		{
			T value;
			return _lock.ReadLock(() => _data.TryGetValue(name, out value) ? value : new T());
		}

		public int Count
		{
			get { return _lock.ReadLock(() => _data.Count); }
		}

		private void AddElement(string name, T item)
		{
			_lock.WriteLock(() => _data.Add(name, item));
		}
	}
}
