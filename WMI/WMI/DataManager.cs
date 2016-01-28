using System;
using System.Runtime.InteropServices;
using WMI.DataClasses;
using WMI.DataProviders;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataManager : IDataManager
	{
		private readonly IDataProvider<Core> _cpuDataProvider;
		private readonly IDataProvider<Drive> _drivesDataProvider;
		private readonly IDataProvider<NetworkInterface> _networkDataProvider;
		private readonly IDataProvider<Ram> _ramDataProvider;

		private bool _disposed;

		public DataManager(int updateInterval)
		{
			_cpuDataProvider = new CpuDataProvider(updateInterval);
			_drivesDataProvider = new DrivesDataProvider(updateInterval);
			_networkDataProvider = new NetworkDataProvider(updateInterval);
			_ramDataProvider = new RamDataProvider(updateInterval);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_cpuDataProvider.Dispose();
				_drivesDataProvider.Dispose();
				_networkDataProvider.Dispose();
				_ramDataProvider.Dispose();

				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public Drive GetDriveData(int i)
		{
			return _drivesDataProvider[i];
		}

		public int GetDrivesCount()
		{
			return _drivesDataProvider.Count;
		}

		public Core GetProcessorData(int i)
		{
			return _cpuDataProvider[i];
		}

		public int GetCoresCount()
		{
			return _cpuDataProvider.Count;
		}

		public NetworkInterface GetNetworkData(string name)
		{
			return _networkDataProvider.GetByName(name);
		}

		public string GetNetworkInterfaceName(int i)
		{
			return _networkDataProvider[i].Name;
		}

		public int GetNetworkInterfacesCount()
		{
			return _networkDataProvider.Count;
		}

		public Ram GetRamData()
		{
			return _ramDataProvider[0];
		}

		public bool HasRamData()
		{
			return _ramDataProvider.Count != 0;
		}
	}
}