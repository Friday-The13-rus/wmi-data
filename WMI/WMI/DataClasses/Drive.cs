using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Drive
	{
		public string Name { get; private set; }
		public string VolumeName { get; set; }
		public ulong FreeSpace { get; set; }
		public ulong Space { get; set; }
		public byte UsePercent { get; set; }
		public byte ActivePercent { get; set; }

		public Drive(string name, string volumeName, ulong freeSpace, ulong space, byte usePercent, byte activePercent)
		{
			Name = name;
			VolumeName = volumeName;
			FreeSpace = freeSpace;
			Space = space;
			UsePercent = usePercent;
			ActivePercent = activePercent;
		}
	}
}