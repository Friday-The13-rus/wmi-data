using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WMI
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
    public class Container
    {
        public List<Drive> drivers;
		public List<Core> processor;
		public NetworkInterface network;

		public Container()
		{
			drivers = new List<Drive>();
			processor = new List<Core>();
			network = new NetworkInterface();
		}
    }

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
    public class Drive
    {
        public string name;
		public string volumeName;
        public UInt64 freeSpace;
		public UInt64 usePercent;
		public UInt64 activePercent;
    }

	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
    public class Core
    {
		public string name;
		public UInt64 usePercent;
    }

	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class NetworkInterface
	{
		public UInt32 received;
		public UInt32 sent;
	}
}
