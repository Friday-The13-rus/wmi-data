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
		readonly ReaderWriterLockSlim driveLock = new ReaderWriterLockSlim();
		readonly ReaderWriterLockSlim cpuLock = new ReaderWriterLockSlim();

		private readonly SortedList<string, Core> cores = new SortedList<string, Core>(6);
		private readonly SortedList<string, Drive> drives = new SortedList<string, Drive>(3);
		private readonly NetworkInterface network = new NetworkInterface();

		private readonly ManagementObjectSearcher drivesPerfSearcher;
		private readonly ManagementObjectSearcher drivesSearcher;
		private readonly ManagementObjectSearcher networkSearcher;
		private readonly ManagementObjectSearcher processorSearcher;

		private readonly System.Timers.Timer timer = new System.Timers.Timer();
		private bool disposed;

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

			processorSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor"), opt);
			drivesSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, FreeSpace, VolumeName, Size FROM Win32_LogicalDisk WHERE Access != null"));
			drivesPerfSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk"), opt);
			networkSearcher = new ManagementObjectSearcher(scope,
				new SelectQuery("SELECT BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface"),
				opt);

			timer.Interval = updateInterval;
			timer.Elapsed += (source, e) => UpdateProcessorInfo();
			timer.Elapsed += (source, e) => UpdateNetworkInfo();
			timer.Elapsed += (source, e) => UpdateDrivesInfo();
			timer.Elapsed += (source, e) => UpdateDrivesPersentDiskTimeInfo();
			timer.Start();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					timer.Stop();
					timer.Dispose();

					processorSearcher.Dispose();
					drivesSearcher.Dispose();
					drivesPerfSearcher.Dispose();
					networkSearcher.Dispose();

					driveLock.Dispose();
					cpuLock.Dispose();
				}
				disposed = true;
			}
		}

		public Drive GetDriveData(int drive)
		{
			return driveLock.ReadLock(() => drives.Values[drive]);
		}

		public int GetDrivesCount()
		{
			return driveLock.ReadLock(() => drives.Count);
		}

		public Core GetProcessorData(int core)
		{
			return cpuLock.ReadLock(() => cores.Values[core]);
		}

		public int GetCoresCount()
		{
			return cpuLock.ReadLock(() => cores.Count);
		}

		public NetworkInterface GetNetworkData()
		{
			return network;
		}

		private void UpdateNetworkInfo()
		{
			foreach (ManagementObject obj in networkSearcher.Get())
			{
				network.Update(obj);
				obj.Dispose();
				break;
			}
		}

		private void UpdateProcessorInfo()
		{
			foreach (ManagementObject obj in processorSearcher.Get())
			{
				string name = obj.GetName();

				Core currentCore;
				if (!cores.TryGetValue(name, out currentCore))
				{
					cpuLock.WriteLock(() =>
					{
						cores.Add(name, new Core(obj));
					});
				}
				else
				{
					cpuLock.WriteLock(() =>
					{
						currentCore.Update(obj);
					});
				}
				obj.Dispose();
			}
		}

		public void UpdateDrivesInfo()
		{
			using (ManagementObjectCollection searcherGet = drivesSearcher.Get())
			{
				if (drives.Count > searcherGet.Count)
				{
					driveLock.WriteLock(() => drives.Clear());
				}

				foreach (ManagementObject obj in searcherGet)
				{
					string name = obj.GetName();

					Drive currentDrive;
					if (!drives.TryGetValue(name, out currentDrive))
					{
						driveLock.WriteLock(() =>
						{
							drives.Add(name, new Drive(obj));
						});
					}
					else
					{
						driveLock.WriteLock(() =>
						{
							currentDrive.Update(obj);
						});
					}
					obj.Dispose();
				}
			}
		}

		void UpdateDrivesPersentDiskTimeInfo()
		{
			foreach (ManagementObject obj in drivesPerfSearcher.Get())
			{
				Drive drive;
				string driveName = obj.GetName();
				if (drives.TryGetValue(driveName, out drive))
				{
					driveLock.WriteLock(() =>
					{
						drive.Update(obj);
					});
				}
				obj.Dispose();
			}
		}
	}
}