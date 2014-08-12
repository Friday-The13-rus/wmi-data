 using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataManager : IDisposable
	{
		private readonly SortedList<string, Core> cores = new SortedList<string, Core>(6);
		private readonly SortedList<string, Drive> drives = new SortedList<string, Drive>(3);
		private readonly NetworkInterface network = new NetworkInterface();

		private readonly ManagementObjectSearcher drivesPerfSearcher;
		private readonly ManagementObjectSearcher drivesSearcher;
		private readonly ManagementObjectSearcher networkSearcher;
		private readonly ManagementObjectSearcher processorSearcher;

		private readonly Timer timer = new Timer();
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
			timer.Elapsed += (source, e) => UpdateDrivesInfo();
			timer.Elapsed += (source, e) => UpdateProcessorInfo();
			timer.Elapsed += (source, e) => UpdateNetworkInfo();
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
				}
				disposed = true;
			}
		}

		public Drive GetDriveData(int drive)
		{
			try
			{
				return drives.Values[drive];
			}
			catch (IndexOutOfRangeException)
			{
				return new Drive(string.Empty, string.Empty, 0, 0, 0, 0);
			}
		}

		public int GetDrivesCount()
		{
			return drives.Count;
		}

		public Core GetProcessorData(int core)
		{
			return cores.Values[core];
		}

		public int GetCoresCount()
		{
			return cores.Count;
		}

		public NetworkInterface GetNetworkData()
		{
			return network;
		}

		private void UpdateNetworkInfo()
		{
			foreach (ManagementObject obj in networkSearcher.Get())
			{
				network.received = Convert.ToUInt32(obj["BytesReceivedPerSec"]);
				network.sent = Convert.ToUInt32(obj["BytesSentPerSec"]);
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
					var core = new Core(name, percent);
					cores.Add(name, core);
				}
				else
				{
					currentCore.usePercent = percent;
				}
			}
		}

		public void UpdateDrivesInfo()
		{
			ManagementObjectCollection searcherGet = drivesSearcher.Get();

			if (drives.Count > searcherGet.Count)
			{
				drives.Clear();
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
					var drive = new Drive(name, volumeName, freeSpace, space, usePercent, 0);
					drives.Add(name, drive);
				}
				else
				{
					currentDrive.freeSpace = freeSpace;
					currentDrive.volumeName = volumeName;
					currentDrive.usePercent = usePercent;

					foreach (ManagementObject objPerf in drivesPerfSearcher.Get())
					{
						if (currentDrive.name == objPerf["Name"].ToString())
						{
							currentDrive.activePercent = Convert.ToByte(objPerf["PercentDiskTime"]);
							break;
						}
					}
				}
			}
		}

		/*void GetDataFromEvent()
		{
			EventQuery query = new EventQuery();
			query.QueryString = "SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfOS_Processor' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfDisk_LogicalDisk' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_Tcpip_NetworkInterface'";

			bool stop = false;

			using (ManagementEventWatcher watcher = new ManagementEventWatcher(query))
			{
				Console.WriteLine("Begin");

				do
				{
					using (ManagementBaseObject e = watcher.WaitForNextEvent())
					using (ManagementBaseObject obj = (ManagementBaseObject)e["TargetInstance"])
					{
						string outLine;

						switch ((string)obj["__Class"])
						{
							case "Win32_PerfFormattedData_PerfOS_Processor":
								{
									outLine = String.Format("Core: {0} Percent: {1}", obj["Name"], obj["PercentProcessorTime"]);
									break;
								}
							case "Win32_PerfFormattedData_PerfDisk_LogicalDisk":
								{
									outLine = String.Format("Disk: {0} Percent: {1}", obj["Name"], obj["PercentDiskTime"]);
									break;
								}
							case "Win32_PerfFormattedData_Tcpip_NetworkInterface":
								{
									outLine = String.Format("Received: {0} Sent: {1}", obj["BytesReceivedPerSec"], obj["BytesSentPerSec"]);
									break;
								}
							default:
								{
									outLine = String.Empty;
									break;
								}
						}
					}
				} while (!stop);

				watcher.Stop();
			}
		}*/
	}
}