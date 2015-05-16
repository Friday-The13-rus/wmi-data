using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMI.DataClasses;

namespace WMI
{
	interface IDataManager : IDisposable
	{
		Drive GetDriveData(int i);
		int GetDrivesCount();
		Core GetProcessorData(int i);
		int GetCoresCount();
		NetworkInterface GetNetworkData();
	}
}
