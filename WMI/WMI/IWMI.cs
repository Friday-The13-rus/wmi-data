using System.Runtime.InteropServices;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true),
	InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IWMI
	{
		void Start();

		void Stop();

		Drive GetDriveData(int i);

		int GetDrivesCount();

		Core GetProcessorData(int i);

		int GetCoresCount();

		NetworkInterface GetNetworkData();
	}
}