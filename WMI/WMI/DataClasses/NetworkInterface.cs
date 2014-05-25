using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class NetworkInterface
	{
		public uint received;
		public uint sent;

		public NetworkInterface(uint received, uint sent)
		{
			this.received = received;
			this.sent = sent;
		}

		public NetworkInterface()
		{
			received = 0;
			sent = 0;
		}
	}
}