using System;
using System.Management;

namespace WMI.ManagementObjects
{
	abstract class ManagementEventListenerBase : IDisposable
	{
		private readonly ManagementScope _scope = new ManagementScope("root\\CIMV2");
		
		private bool _disposed;
		private readonly ManagementEventWatcher _watcher;

		public event EventHandler<EventArrivedEventArgs> DataUpdated; 

		protected ManagementEventListenerBase(params string[] tableNames)
		{
			foreach (string tableName in tableNames)
			{
				string condition = string.Format("TargetInstance isa '{0}'", tableName);
				var wqlEventQuery = new WqlEventQuery("__InstanceOperationEvent", new TimeSpan(0, 0, 1), condition);
				_watcher = new ManagementEventWatcher(_scope, wqlEventQuery);
				_watcher.EventArrived += WatcherOnEventArrived;
				_watcher.Start();
			}
		}

		private void WatcherOnEventArrived(object sender, EventArrivedEventArgs eventArrivedEventArgs)
		{
			if (DataUpdated == null)
				return;

			DataUpdated(sender, eventArrivedEventArgs);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_watcher.Stop();
				_watcher.Dispose();
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}
}
