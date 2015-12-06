using System;
using System.Collections.Generic;
using System.Management;

namespace WMI.ManagementObjects
{
	abstract class ManagementEventListenerBase : IDisposable
	{
		private readonly ManagementScope _scope = new ManagementScope("root\\CIMV2");
		
		private bool _disposed;
		private readonly List<ManagementEventWatcher> _watchers = new List<ManagementEventWatcher>();

		public event EventHandler<EventArrivedEventArgs> DataUpdated; 

		protected ManagementEventListenerBase(params string[] tableNames)
		{
			foreach (string tableName in tableNames)
			{
				string condition = string.Format("TargetInstance isa '{0}'", tableName);
				var wqlEventQuery = new WqlEventQuery("__InstanceOperationEvent", new TimeSpan(0, 0, 1), condition);
				var watcher = new ManagementEventWatcher(_scope, wqlEventQuery);
				watcher.EventArrived += WatcherOnEventArrived;
				watcher.Start();
				_watchers.Add(watcher);
			}
		}

		private void WatcherOnEventArrived(object sender, EventArrivedEventArgs eventArrivedEventArgs)
		{
			if (DataUpdated == null)
				return;

			try
			{
				DataUpdated(sender, eventArrivedEventArgs);
			}
			finally
			{
				eventArrivedEventArgs.NewEvent.Dispose();
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				foreach (var watcher in _watchers)
				{
					watcher.Stop();
					watcher.Dispose();
				}
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}
}
