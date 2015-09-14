using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Core : NamedObject
	{
		public byte UsePercent { get; set; }
	}
}