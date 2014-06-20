using System;
using System.Runtime.InteropServices;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true),
	 Guid("A79AC85C-547C-3ED3-AD94-530DC4BBB672"),
	 ProgId("WMI.GetData"),
	 ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataReturner
	{
		DataManager data;

		public void Start()
		{
			data = new DataManager(1000);
		}

		public void Stop()
		{
			data.Dispose();
		}

		public Drive GetDriveData(int i)
		{
			return data.GetDriveData(i);
		}

		public int GetDrivesCount()
		{
			return data.GetDrivesCount();
		}

		public Core GetProcessorData(int i)
		{
			return data.GetProcessorData(i);
		}

		public int GetCoresCount()
		{
			return data.GetCoresCount();
		}

		public NetworkInterface GetNetworkData()
		{
			return data.GetNetworkData();
		}
	}
}