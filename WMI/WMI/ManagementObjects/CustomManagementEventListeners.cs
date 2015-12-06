using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using WMI.DataClasses;

namespace WMI.ManagementObjects
{
	class CpuManagementEventListener : ManagementEventListenerBase
	{
		public CpuManagementEventListener() : base("Win32_PerfFormattedData_PerfOS_Processor") {}
	}

	class DriveManagementEventListener : ManagementEventListenerBase
	{
		public DriveManagementEventListener() : base("Win32_LogicalDisk", "Win32_PerfFormattedData_PerfDisk_LogicalDisk") { }
	}

	class NetworkManagementEventListener : ManagementEventListenerBase
	{
		public NetworkManagementEventListener() : base("Win32_PerfFormattedData_Tcpip_NetworkInterface") { }
	}
}
