using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Ram : NamedObject
	{
		public ulong Total { get; set; }
		public ulong Free { get; set; }

		public ulong InUse
		{
			get { return Total - Free; }
		}
	}
}
