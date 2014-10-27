using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			string QueryString = "SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfOS_Processor' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_PerfDisk_LogicalDisk' Or " +
				"TargetInstance isa 'Win32_PerfFormattedData_Tcpip_NetworkInterface'";
			using (ManagementEventWatcher watcher = new ManagementEventWatcher(QueryString))
			{
				watcher.EventArrived += watcher_EventArrived;
				watcher.Stopped += watcher_Stopped;

				Console.WriteLine("Watcher started");
				watcher.Start();

				Thread.Sleep(10000);
				watcher.Stop();
			}
		}

		static void watcher_Stopped(object sender, StoppedEventArgs e)
		{
			Console.WriteLine("Watcher stoped");
		}

		static void watcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			var obj = (ManagementBaseObject)e.NewEvent["TargetInstance"];

			string outLine;

			switch ((string)obj["__Class"])
			{
				case "Win32_PerfFormattedData_PerfOS_Processor":
					{
						outLine = String.Format("Core: {0} Percent: {1} ({2})", obj["Name"], obj["PercentProcessorTime"], e.NewEvent["__CLASS"]);
						break;
					}
				case "Win32_PerfFormattedData_PerfDisk_LogicalDisk":
					{
						outLine = String.Format("Disk: {0} Percent: {1} ({2})", obj["Name"], obj["PercentDiskTime"], e.NewEvent["__CLASS"]);
						break;
					}
				case "Win32_PerfFormattedData_Tcpip_NetworkInterface":
					{
						outLine = String.Format("Received: {0} Sent: {1} ({2})", obj["BytesReceivedPerSec"], obj["BytesSentPerSec"], e.NewEvent["__CLASS"]);
						break;
					}
				default:
					{
						outLine = String.Empty;
						break;
					}
			}

			Console.WriteLine(outLine);

		}
	}
}
