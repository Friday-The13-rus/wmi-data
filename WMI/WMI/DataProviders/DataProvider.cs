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
	abstract class DataProvider<T> : IDisposable
		where T : NamedObject, new()
	{
		private static readonly ManagementScope Scope = new ManagementScope("root\\CIMV2");
		private static readonly EnumerationOptions Options = new EnumerationOptions
		{
			DirectRead = true,
			EnsureLocatable = false,
			EnumerateDeep = false,
			ReturnImmediately = true,
			Rewindable = false
		};

		protected readonly SortedList<string, T> _data = new SortedList<string, T>();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly Timer _timer = new Timer();

		private readonly IDictionary<ManagementObjectSearcher, Action<ManagementObjectSearcher>> _searchers =
			new Dictionary<ManagementObjectSearcher, Action<ManagementObjectSearcher>>();

		private readonly List<SearcherEntity<T>> _searcherEntities = new List<SearcherEntity<T>>();
 
		private bool _disposed;

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

		private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			try
			{
				foreach (var searcherEntity in _searcherEntities)
					Update(searcherEntity);
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

		public void AddSearcher(SelectQuery query, Action<ManagementObjectSearcher> updateAction)
		{
			_searchers.Add(new ManagementObjectSearcher(Scope, query, Options), updateAction);
		}

		public void AddSearcher(string table, PropertySettersDictionary<T> propertiesSetters, string condition = null, bool canAddElements = true, bool canRemoveElements = false)
		{
			var selectQuery = new SelectQuery(table, condition ?? string.Empty, propertiesSetters.Keys.ToArray());
			selectQuery.SelectedProperties.Add("Name");
			var searcherEntity = new SearcherEntity<T>(new ManagementObjectSearcher(Scope, selectQuery, Options), canAddElements,
				canRemoveElements, propertiesSetters);
			_searcherEntities.Add(searcherEntity);
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

		public void Dispose()
		{
			if (!_disposed)
			{
				_timer.Stop();
				_timer.Dispose();

				_lock.Dispose();
				foreach (var searcher in _searchers.Keys)
				{
					searcher.Dispose();
				}
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public T GetByIndex(int index)
		{
			return _lock.ReadLock(() => _data.Values[index]);
		}

		public T GetByName(string name)
		{
			T value;
			return _lock.ReadLock(() => _data.TryGetValue(name, out value) ? value : new T());
		}

		public int ElementsCount
		{
			get { return _lock.ReadLock(() => _data.Count); }
		}

		private void AddElement(string name, T item)
		{
			_lock.WriteLock(() => _data.Add(name, item));
		}
	}
}
