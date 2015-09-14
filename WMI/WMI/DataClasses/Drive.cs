using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Drive : NamedObject
	{
		public string VolumeName { get; set; }
		public ulong FreeSpace { get; set; }
		public ulong Space { get; set; }

		public byte UsePercent
		{
			get { return Convert.ToByte(100 * (1 - (double) FreeSpace / Space)); }
		}

		public byte ActivePercent { get; set; }
	}
}