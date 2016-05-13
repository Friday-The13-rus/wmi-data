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
		IDataManager _data;

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

		public ArrayObject GetCpuData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetProcessorData());
		}

		public ArrayObject GetDriveData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetDriveData());
		}

		public ArrayObject GetNetworkData()
		{
			return GlobalObject.Array.ConstructArray(_data.GetNetworkData());
		}
	}
}