using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataManager : IDataManager
	{
		readonly ReaderWriterLockSlim _driveLock = new ReaderWriterLockSlim();
		readonly ReaderWriterLockSlim _cpuLock = new ReaderWriterLockSlim();

		private readonly SortedList<string, Core> _cores = new SortedList<string, Core>(6);
		private readonly SortedList<string, Drive> _drives = new SortedList<string, Drive>(3);
		private readonly NetworkInterface _network = new NetworkInterface();

		private readonly ManagementObjectSearcher _drivesPerfSearcher;
		private readonly ManagementObjectSearcher _drivesSearcher;
		private readonly ManagementObjectSearcher _networkSearcher;
		private readonly ManagementObjectSearcher _processorSearcher;

		private readonly System.Timers.Timer _timer = new System.Timers.Timer();
		private bool _disposed;

		public DataManager(int updateInterval)
		{
			var scope = new ManagementScope("root\\CIMV2");
			scope.Connect();
			var opt = new EnumerationOptions
			{
				DirectRead = true,
				EnsureLocatable = false,
				EnumerateDeep = false,
				ReturnImmediately = true,
				Rewindable = false
			};

			_processorSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor"), opt);
			_drivesSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, FreeSpace, VolumeName, Size FROM Win32_LogicalDisk WHERE Access != null"));
			_drivesPerfSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk"), opt);
			_networkSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface"),
				opt);

			_timer.Interval = updateInterval;
			_timer.Elapsed += (source, e) => UpdateProcessorInfo();
			_timer.Elapsed += (source, e) => UpdateNetworkInfo();
			_timer.Elapsed += (source, e) => UpdateDrivesInfo();
			_timer.Elapsed += (source, e) => UpdateDrivesPersentDiskTimeInfo();
			_timer.Start();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_timer.Stop();
					_timer.Dispose();

					_processorSearcher.Dispose();
					_drivesSearcher.Dispose();
					_drivesPerfSearcher.Dispose();
					_networkSearcher.Dispose();

					_driveLock.Dispose();
					_cpuLock.Dispose();
				}
				_disposed = true;
			}
		}

		public Drive GetDriveData(int drive)
		{
			return _driveLock.ReadLock(() => _drives.Values[drive]);
		}

		public int GetDrivesCount()
		{
			return _driveLock.ReadLock(() => _drives.Count);
		}

		public Core GetProcessorData(int core)
		{
			return _cpuLock.ReadLock(() => _cores.Values[core]);
		}

		public int GetCoresCount()
		{
			return _cpuLock.ReadLock(() => _cores.Count);
		}

		public NetworkInterface GetNetworkData()
		{
			return _network;
		}

		private void UpdateNetworkInfo()
		{
			foreach (ManagementObject obj in _networkSearcher.Get())
			{
				_network.Update(obj);
				obj.Dispose();
				break;
			}
		}

		private void UpdateProcessorInfo()
		{
			foreach (ManagementObject obj in _processorSearcher.Get())
			{
				string name = obj.GetName();

				Core currentCore;
				if (!_cores.TryGetValue(name, out currentCore))
				{
					_cpuLock.WriteLock(() =>
					{
						_cores.Add(name, new Core(obj));
					});
				}
				else
				{
					_cpuLock.WriteLock(() =>
					{
						currentCore.Update(obj);
					});
				}
			}
		}

		public void UpdateDrivesInfo()
		{
			using (ManagementObjectCollection searcherGet = _drivesSearcher.Get())
			{
				if (_drives.Count > searcherGet.Count)
				{
					_driveLock.WriteLock(() => _drives.Clear());
				}

				foreach (ManagementObject obj in searcherGet)
				{
					string name = obj.GetName();

					Drive currentDrive;
					if (!_drives.TryGetValue(name, out currentDrive))
					{
						_driveLock.WriteLock(() =>
						{
							_drives.Add(name, new Drive(obj));
						});
					}
					else
					{
						_driveLock.WriteLock(() =>
						{
							currentDrive.Update(obj);
						});
					}
				}
			}
		}

		void UpdateDrivesPersentDiskTimeInfo()
		{
			foreach (ManagementObject obj in _drivesPerfSearcher.Get())
			{
				Drive drive;
				string driveName = obj.GetName();
				if (_drives.TryGetValue(driveName, out drive))
				{
					_driveLock.WriteLock(() =>
					{
						drive.Update(obj);
					});
				}
			}
		}
	}
}