using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;

namespace Test
{
	class Program
	{
		static Dictionary<string, Func<EventArrivedEventArgs, string>> tables = new Dictionary<string, Func<EventArrivedEventArgs, string>>()
			{
				{"Win32_PerfFormattedData_PerfOS_Processor", GetProcessorString},
				{"Win32_PerfFormattedData_PerfDisk_LogicalDisk", GetDiskString},
				{"Win32_PerfFormattedData_Tcpip_NetworkInterface", GetNetworkString}
			};

		static void Main(string[] args)
		{
			string condition = GetConditionString(tables.Keys.ToArray());

			var wqlEventQuery = new WqlEventQuery("__InstanceOperationEvent", new TimeSpan(0, 0, 1), condition);
			using (ManagementEventWatcher watcher = new ManagementEventWatcher(wqlEventQuery))
			{
				watcher.EventArrived += watcher_EventArrived;
				watcher.Stopped += watcher_Stopped;
				watcher.Scope = new ManagementScope("root\\CIMV2");

				Console.WriteLine("Watcher started");
				watcher.Start();

				Thread.Sleep(10000);
				watcher.Stop();
			}
		}

		private static string GetConditionString(string[] tables)
		{
			return tables.Select(el => string.Format("TargetInstance isa '{0}'", el)).Aggregate((a, b) => string.Format("{0} Or {1}", a, b));
		}

		static void watcher_Stopped(object sender, StoppedEventArgs e)
		{
			Console.WriteLine("Watcher stoped");
		}

		static void watcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			string outLine = tables[GetClass(e)](e);
			Console.WriteLine(outLine);
		}

		static string GetClass(EventArrivedEventArgs e)
		{
			return (string)((ManagementBaseObject)e.NewEvent["TargetInstance"])["__Class"];
		}

		private static string GetProcessorString(EventArrivedEventArgs e)
		{
			var obj = (ManagementBaseObject)e.NewEvent["TargetInstance"];
			return String.Format("Core: {0} Percent: {1} ({2})", obj["Name"], obj["PercentProcessorTime"], e.NewEvent["__CLASS"]);
		}

		private static string GetDiskString(EventArrivedEventArgs e)
		{
			var obj = (ManagementBaseObject)e.NewEvent["TargetInstance"];
			return String.Format("Disk: {0} Percent: {1} ({2})", obj["Name"], obj["PercentDiskTime"], e.NewEvent["__CLASS"]);
		}

		private static string GetNetworkString(EventArrivedEventArgs e)
		{
			var obj = (ManagementBaseObject)e.NewEvent["TargetInstance"];
			return String.Format("Received: {0} Sent: {1} ({2})", obj["BytesReceivedPerSec"], obj["BytesSentPerSec"], e.NewEvent["__CLASS"]);
		}
	}
}