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
		private bool _disposed;

		protected static readonly ManagementScope Scope = new ManagementScope("root\\CIMV2");
		protected static readonly EnumerationOptions Options = new EnumerationOptions
		{
			DirectRead = true,
			EnsureLocatable = false,
			EnumerateDeep = false,
			ReturnImmediately = true,
			Rewindable = false
		};

		private readonly Timer _timer = new Timer();

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
				EventLog.WriteEntry("SystemInfo Windows Sidebar Gadget", exception.ToString(), EventLogEntryType.Error);
			}
			finally
			{
				_timer.Start();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DataProvider()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_timer.Stop();
				_timer.Dispose();
			}

			_disposed = true;
		}
	}

	internal abstract class DataProvider<T> : DataProvider, IDataProvider<T> where T : NamedObject, new()
	{
		private bool _disposed;

		private readonly IDictionary<string, T> _data;
		private readonly ICollection<SearcherEntity<T>> _searcherEntities = new List<SearcherEntity<T>>();

		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		protected DataProvider(int updateInterval)
			:this(updateInterval, Comparer<string>.Default)
		{
		}

		protected DataProvider(int updateInterval, IComparer<string> dataComparer) 
			: base(updateInterval)
		{
			_data = new SortedList<string, T>(dataComparer);
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
			ICollection<string> removedEntities = null;
			if (searcherEntity.CanRemoveElements)
				removedEntities = new HashSet<string>(_data.Keys);

			foreach (ManagementObject obj in searcherEntity.Searcher.Get())
			{
				var name = obj.GetName();
				T current;
				if (!_data.TryGetValue(name, out current))
				{
					if (!searcherEntity.CanAddElements)
						continue;
					current = Add(name);
				}

				UpdateProperties(current, searcherEntity.PropertySetters, obj);

				if (searcherEntity.CanRemoveElements)
					removedEntities.Remove(name);
			}

			if (searcherEntity.CanRemoveElements)
				Remove(removedEntities);
		}

		private T Add(string key)
		{
			var value = new T() { Name = key };
			_lock.WriteLock(() => _data.Add(key, value));
			return value;
		}

		private void UpdateProperties(T item, PropertySettersDictionary<T> propertySetters, ManagementObject managementObject)
		{
			_lock.WriteLock(() =>
			{
				foreach (var propertiesSetter in propertySetters)
				{
					propertiesSetter.Value(item, managementObject[propertiesSetter.Key]);
				}
			});
		}

		private void Remove(IEnumerable<string> keys)
		{
			_lock.WriteLock(() =>
			{
				foreach (var entity in keys)
				{
					_data.Remove(entity);
				}
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_lock.Dispose();
				foreach (var searcherEntity in _searcherEntities)
					searcherEntity.Dispose();
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		~DataProvider()
		{
			Dispose(false);
		}

		public T this[int index]
		{
			get { return _lock.ReadLock(() => _data.ElementAt(index).Value ); }
		}

		public int Count
		{
			get { return _lock.ReadLock(() => _data.Count); }
		}

		public T[] GetAll()
		{
			return _lock.ReadLock(() => _data.Values.ToArray());
		}
	}
}
