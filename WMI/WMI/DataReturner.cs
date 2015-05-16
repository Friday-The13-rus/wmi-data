﻿using System;
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
		IDataManager _data;

		public void Start()
		{
			_data = new DataManager(1000);
			//_data = new EventsDataManager();
		}

		public void Stop()
		{
			_data.Dispose();
		}

		public Drive GetDriveData(int i)
		{
			return _data.GetDriveData(i);
		}

		public int GetDrivesCount()
		{
			return _data.GetDrivesCount();
		}

		public Core GetProcessorData(int i)
		{
			return _data.GetProcessorData(i);
		}

		public int GetCoresCount()
		{
			return _data.GetCoresCount();
		}

		public NetworkInterface GetNetworkData()
		{
			return _data.GetNetworkData();
		}
	}
}