using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Core
	{
		public string name;
		public byte usePercent;

		public Core(string name, byte percent)
		{
			this.name = name;
			this.usePercent = percent;
		}
	}
}