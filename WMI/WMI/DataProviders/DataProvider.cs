using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Timers;

namespace WMI.DataProviders
{
	abstract class DataProvider<T> : IDisposable
		where T : IUpdatable<ManagementBaseObject>, new()
	{
		private static ManagementScope Scope = new ManagementScope("root\\CIMV2");
		private static EnumerationOptions Options = new EnumerationOptions
		{
			DirectRead = true,
			EnsureLocatable = false,
			EnumerateDeep = false,
			ReturnImmediately = true,
			Rewindable = false
		};

		protected readonly SortedList<string, T> _data = new SortedList<string, T>();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		private readonly IDictionary<ManagementObjectSearcher, Action<ManagementObjectSearcher>> _searchers =
			new Dictionary<ManagementObjectSearcher, Action<ManagementObjectSearcher>>();
 
		private bool _disposed;

		static DataProvider()
		{
			Scope.Connect();
		}

		public void AddSearcher(SelectQuery query, Action<ManagementObjectSearcher> updateAction)
		{
			_searchers.Add(new ManagementObjectSearcher(Scope, query, Options), updateAction);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
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
			return _lock.ReadLock(() => _data.ContainsKey(name) ? _data[name] : new T());
		}

		public int ElementsCount
		{
			get { return _lock.ReadLock(() => _data.Count); }
		}

		public IEnumerable<ElapsedEventHandler> UpdateActions
		{
			get
			{
				foreach (var searcher in _searchers)
				{
					yield return (sender, args) => searcher.Value(searcher.Key);
				}
			}
		}

		protected void AddElement(string name, T item)
		{
			_lock.WriteLock(() => _data.Add(name, item));
		}

		protected void UpdateElement(T item, ManagementBaseObject newValue)
		{
			_lock.WriteLock(() => item.Update(newValue));
		}
	}
}
