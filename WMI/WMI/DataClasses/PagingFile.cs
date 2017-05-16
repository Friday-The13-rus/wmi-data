using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class PagingFile : NamedObject
	{
		public uint UsagePercent { get; set; }
	}
}
