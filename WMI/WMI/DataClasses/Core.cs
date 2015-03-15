using System;
using System.Management;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Core
	{
		public string Name { get; private set; }
		public byte UsePercent { get; set; }

		public Core(string name, byte percent)
		{
			Name = name;
			UsePercent = percent;
		}
	}
}