using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataManager : IDisposable
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
				}
				disposed = true;
			}
		}

		public Drive GetDriveData(int drive)
		{
			return driveLock.ReadLock(() =>
			{
				return drives.Values[drive];
			});
		}

		public int GetDrivesCount()
		{
			return driveLock.ReadLock(() =>
			{
				return drives.Count;
			});
		}

		public Core GetProcessorData(int core)
		{
			return cpuLock.ReadLock(() =>
			{
				return cores.Values[core];
			});
		}

		public int GetCoresCount()
		{
			return cpuLock.ReadLock(() =>
			{
				return cores.Count;
			});
		}

		public NetworkInterface GetNetworkData()
		{
			return network;
		}

		private void UpdateNetworkInfo()
		{
			foreach (ManagementObject obj in networkSearcher.Get())
			{
				network.Received = Convert.ToUInt32(obj["BytesReceivedPerSec"]);
				network.Sent = Convert.ToUInt32(obj["BytesSentPerSec"]);
				break;
			}
		}

		private void UpdateProcessorInfo()
		{
			foreach (ManagementObject obj in processorSearcher.Get())
			{
				string name = obj["Name"].ToString();
				byte percent = Convert.ToByte(obj["PercentProcessorTime"]);

				Core currentCore;
				if (!cores.TryGetValue(name, out currentCore))
				{
					cpuLock.WriteLock(() =>
					{
						var core = new Core(name, percent);
						cores.Add(name, core);
					});
				}
				else
				{
					cpuLock.WriteLock(() =>
					{
						currentCore.UsePercent = percent;
					});
				}
			}
		}

		public void UpdateDrivesInfo()
		{
			ManagementObjectCollection searcherGet = drivesSearcher.Get();

			if (drives.Count > searcherGet.Count)
			{
				driveLock.WriteLock(() =>
				{
					drives.Clear();
				});
			}

			foreach (ManagementObject obj in searcherGet)
			{
				string name = obj["Name"].ToString();
				ulong freeSpace = Convert.ToUInt64(obj["FreeSpace"]);
				ulong space = Convert.ToUInt64(obj["Size"]);
				string volumeName = obj["VolumeName"].ToString();
				byte usePercent = Convert.ToByte(100 * (1 - (double)freeSpace / space));

				Drive currentDrive;
				if (!drives.TryGetValue(name, out currentDrive))
				{
					driveLock.WriteLock(() =>
					{
						var drive = new Drive(name, volumeName, freeSpace, space, usePercent, 0);
						drives.Add(name, drive);
					});
				}
				else
				{
					driveLock.WriteLock(() =>
					{
						currentDrive.FreeSpace = freeSpace;
						currentDrive.VolumeName = volumeName;
						currentDrive.UsePercent = usePercent;
					});
				}
			}
		}

		void UpdateDrivesPersentDiskTimeInfo()
		{
			foreach (ManagementObject obj in drivesPerfSearcher.Get())
			{
				Drive drive;
				string driveName = obj["Name"].ToString();
				if (drives.TryGetValue(driveName, out drive))
				{
					driveLock.WriteLock(() =>
					{
						drive.ActivePercent = Convert.ToByte(obj["PercentDiskTime"]);
					});
				}
			}
		}
	}
}