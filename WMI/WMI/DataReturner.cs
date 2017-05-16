using System.Runtime.InteropServices;
using Microsoft.JScript;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true),
	 Guid("A79AC85C-547C-3ED3-AD94-530DC4BBB672"),
	 ProgId("WMI.DataReturner"),
	 ClassInterface(ClassInterfaceType.AutoDual)]
	public class DataReturner
	{
		private IDataManager _data;

		public void Start()
		{
			_data = new DataManager(1000);
		}

		public void Stop()
		{
			_data.Dispose();
		}

		public Ram GetRamData()
		{
			return _data.GetRamData();
		}

		public bool HasRamData()
		{
			return _data.HasRamData();
		}

		public object GetCpuData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetProcessorData());
		}

		public object GetDriveData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetDriveData());
		}

		public object GetNetworkData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetNetworkData());
		}

		public PagingFile GetPagingFileData()
		{
			return _data.GetPagingFileData();
		}

		public bool HasPagingFileData()
		{
			return _data.HasPagingFileData();
		}
	}
}