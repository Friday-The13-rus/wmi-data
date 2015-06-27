using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using WMI.DataClasses;
using WMI.ManagementObjects;

namespace WMI
{
	[Obsolete("Biggest memory leak, don't used", true)]
	class EventsDataManager : IDataManager
	{
		private bool _disposed;

		private readonly CpuManagementEventListener _cpuManagementEventListener;
		private readonly DriveManagementEventListener _driveManagementEventListener;
		private readonly NetworkManagementEventListener _networkManagementEventListener;

		private readonly List<Core> _cores = new List<Core>(6);
		private readonly List<Drive> _drives = new List<Drive>(3);
		private readonly NetworkInterface _network = new NetworkInterface();

		public EventsDataManager()
		{
			Initialize();
			_cpuManagementEventListener = new CpuManagementEventListener();
			_driveManagementEventListener = new DriveManagementEventListener();
			_networkManagementEventListener = new NetworkManagementEventListener();

			_cpuManagementEventListener.DataUpdated += CpuManagementEventListenerOnDataUpdated;
			_driveManagementEventListener.DataUpdated += DriveManagementEventListenerOnDataUpdated;
			_networkManagementEventListener.DataUpdated += NetworkManagementEventListenerOnDataUpdated;
		}

		private void Initialize()
		{
			using (var drivesSearcher = new ManagementObjectSearcher("root\\CIMV2",
				"SELECT Name, FreeSpace, VolumeName, Size FROM Win32_LogicalDisk WHERE Access != null"))
			using (ManagementObjectCollection searcherGet = drivesSearcher.Get())
			{
				foreach (ManagementBaseObject obj in searcherGet)
				{
					_drives.Add(new Drive(obj));
					obj.Dispose();
				}
			}
		}

		private void NetworkManagementEventListenerOnDataUpdated(object sender, EventArrivedEventArgs dataUpdatedEventArgs)
		{
			var newObject = dataUpdatedEventArgs.NewEvent.GetTargetInstance();
			try
			{
				_network.Update(newObject);
			}
			finally
			{
				newObject.Dispose();
			}
		}

		private void DriveManagementEventListenerOnDataUpdated(object sender, EventArrivedEventArgs dataUpdatedEventArgs)
		{
			var newObject = dataUpdatedEventArgs.NewEvent.GetTargetInstance();
			try
			{
				var objectName = newObject.GetName();
				if (objectName == "_Total")
					return;

				var drive = _drives.Find(el => string.Equals(el.Name, objectName));
				if (drive == null)
				{
					_drives.Add(new Drive(newObject));
					_drives.Sort();
				}
				else
				{
					if (dataUpdatedEventArgs.NewEvent.IsInstanceDeletionEvent())
						_drives.Remove(drive);
					else
						drive.Update(newObject);
				}
			}
			finally
			{
				newObject.Dispose();
			}
		}

		private void CpuManagementEventListenerOnDataUpdated(object sender, EventArrivedEventArgs dataUpdatedEventArgs)
		{
			var newObject = dataUpdatedEventArgs.NewEvent.GetTargetInstance();
			try
			{
				var objectName = newObject.GetName();
				var core = _cores.Find(el => string.Equals(el.Name, objectName));
				if (core == null)
				{
					_cores.Add(new Core(newObject));
					_cores.Sort();
				}
				else
				{
					core.Update(newObject);
				}
			}
			finally
			{
				newObject.Dispose();
			}
		}

		public Drive GetDriveData(int i)
		{
			return _drives[i];
		}

		public int GetDrivesCount()
		{
			return _drives.Count;
		}

		public Core GetProcessorData(int i)
		{
			return _cores[i];
		}

		public int GetCoresCount()
		{
			return _cores.Count;
		}

		public NetworkInterface GetNetworkData(string name)
		{
			throw new NotImplementedException();
		}

		public NetworkInterface GetNetworkData()
		{
			return _network;
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_cpuManagementEventListener.Dispose();
				_driveManagementEventListener.Dispose();
				_networkManagementEventListener.Dispose();
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}
}
