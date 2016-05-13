using System;
using WMI.DataClasses;

namespace WMI
{
	interface IDataManager : IDisposable
	{
		Drive[] GetDriveData();
		Core[] GetProcessorData();
		NetworkInterface[] GetNetworkData();
		Ram GetRamData();
		bool HasRamData();
	}
}
