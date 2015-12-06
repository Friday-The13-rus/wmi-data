using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;
using WMI.DataClasses;
using WMI.DataProviders;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataManager : IDataManager
	{
		private readonly CpuDataProvider _cpuDataProvider;
		private readonly DrivesDataProvider _drivesDataProvider;
		private readonly NetworkDataProvider _networkDataProvider;

		private bool _disposed;

		public DataManager(int updateInterval)
		{
			_cpuDataProvider = new CpuDataProvider(updateInterval);
			_drivesDataProvider = new DrivesDataProvider(updateInterval);
			_networkDataProvider = new NetworkDataProvider(updateInterval);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_cpuDataProvider.Dispose();
				_drivesDataProvider.Dispose();
				_networkDataProvider.Dispose();

				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public Drive GetDriveData(int drive)
		{
			return _drivesDataProvider.GetByIndex(drive);
		}

		public int GetDrivesCount()
		{
			return _drivesDataProvider.ElementsCount;
		}

		public Core GetProcessorData(int core)
		{
			return _cpuDataProvider.GetByIndex(core);
		}

		public int GetCoresCount()
		{
			return _cpuDataProvider.ElementsCount;
		}

		public NetworkInterface GetNetworkData(string name)
		{
			return _networkDataProvider.GetByName(name);
		}
	}
}