using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class NetworkInterface : NamedObject
	{
		public ulong Received { get; set; }
		public ulong Sent { get; set; }
	}
}