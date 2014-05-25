using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;

namespace WMI
{
	[ComVisible(true),
	InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface WMI
	{
		void Start();

		void Stop();

		void GetDriversInfo(object source, ElapsedEventArgs e);

		void GetProcessorInfo(object source, ElapsedEventArgs e);

		Drive GetDriverData(int i);

		Core GetProcessorData(int i);

		int GetDriversCount();

		int GetCoresCount();

		void GetNetworkInfo(object source, ElapsedEventArgs e);

		NetworkInterface GetNetworkData();

		void GetDataFromEvent();
	}

	[ComVisible(true), 
	GuidAttribute("A79AC85C-547C-3ED3-AD94-530DC4BBB672"), 
	ProgId("WMI.GetData"), 
	ClassInterface(ClassInterfaceType.None)]
	public class GetData : WMI
	{
		private Container data;

		private bool stop = false;
		
		public void Start()
		{
			data = new Container();

			//Timer DriversTimer = new Timer(GetDriversInfo, null, 1000, 1000);
			//Timer ProcessorTimer = new Timer(GetProcessorInfo, null, 1000, 1000);
			Timer timer = new Timer(1000);
			timer.Elapsed += new ElapsedEventHandler(GetDriversInfo);
			timer.Elapsed += new ElapsedEventHandler(GetProcessorInfo);
			timer.Elapsed += new ElapsedEventHandler(GetNetworkInfo);
			timer.AutoReset = true;
			timer.Start();
		}

		public void Stop()
		{
			//data.Dispose();
		}

		public Drive GetDriverData(int i)
		{
			return data.drivers[i];
		}

		public void GetDriversInfo(object source, ElapsedEventArgs e)
		{
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, FreeSpace, VolumeName, Size FROM Win32_LogicalDisk WHERE Access != null"))
			using (ManagementObjectSearcher searcherPerf = new ManagementObjectSearcher("SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk"))
			{
				ManagementObjectCollection searcherGet = searcher.Get();

				if (data.drivers.Count > searcherGet.Count)
				{
					data.drivers.Clear();
				}

				foreach (ManagementObject obj in searcherGet)
				{
					if (!data.drivers.Exists(dr => dr.name == obj["Name"].ToString()))
					{
						Drive drive = new Drive
						{
							name = obj["Name"].ToString(),
							freeSpace = Convert.ToUInt64(obj["FreeSpace"]),
							volumeName = obj["VolumeName"].ToString(),
							usePercent = Convert.ToUInt64(100 * (1 - Convert.ToDouble(obj["FreeSpace"]) / Convert.ToDouble(obj["Size"]))),
							activePercent = 0
						};

						data.drivers.Add(drive);
					}
					else
					{
						Drive currentDrive = data.drivers.Find(dr => dr.name == obj["Name"].ToString());
						currentDrive.freeSpace = Convert.ToUInt64(obj["FreeSpace"]);
						currentDrive.volumeName = obj["VolumeName"].ToString();
						currentDrive.usePercent = Convert.ToUInt64(100 * (1 - Convert.ToDouble(obj["FreeSpace"]) / Convert.ToDouble(obj["Size"])));
						
						foreach (ManagementObject objPerf in searcherPerf.Get())
						{
							if (currentDrive.name == objPerf["Name"].ToString())
							{
								currentDrive.activePercent = Convert.ToUInt64(objPerf["PercentDiskTime"]);
								break;
							}
						}
					}
				}
			}
		}

		public int GetDriversCount()
		{
			return data.drivers.Count;
		}

		public Core GetProcessorData(int i)
		{
			return data.processor[i];
		}

		public void GetProcessorInfo(object source, ElapsedEventArgs e)
		{
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor"))
			{
				foreach (ManagementObject obj in searcher.Get())
				{
					if (!data.processor.Exists(core => core.name == obj["Name"].ToString()))
					{
						Core core = new Core
						{
							name = obj["Name"].ToString(),
							usePercent = Convert.ToUInt64(obj["PercentProcessorTime"])
						};

						data.processor.Add(core);
					}
					else
					{
						Core currentCore = data.processor.Find(core => core.name == obj["Name"].ToString());
						currentCore.usePercent = Convert.ToUInt64(obj["PercentProcessorTime"]);
					}
				}
			}
		}

		public int GetCoresCount()
		{
			return data.processor.Count;
		}

		public void GetNetworkInfo(object source, ElapsedEventArgs e)
		{
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface"))
			{
				foreach (ManagementObject obj in searcher.Get())
				{
					NetworkInterface network = new NetworkInterface
					{
						received = Convert.ToUInt32(obj["BytesReceivedPerSec"]),
						sent = Convert.ToUInt32(obj["BytesSentPerSec"])
					};

					data.network = network;
					break;
				}
			}
		}

		public NetworkInterface GetNetworkData()
		{
			return data.network;
		}

		public void GetDataFromEvent()
		{
			EventQuery query = new EventQuery();
			query.QueryString = "SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfOS_Processor' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfDisk_LogicalDisk' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_Tcpip_NetworkInterface'";

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
		}
	}
}