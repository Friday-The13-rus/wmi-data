using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Drive
	{
		public string name;
		public string volumeName;
		public ulong freeSpace;
		public ulong space;
		public byte usePercent;
		public byte activePercent;

		public Drive(string name, string volumeName, ulong freeSpace, ulong space, byte usePercent, byte activePercent)
		{
			this.name = name;
			this.volumeName = volumeName;
			this.freeSpace = freeSpace;
			this.space = space;
			this.usePercent = usePercent;
			this.activePercent = activePercent;
		}
	}
}