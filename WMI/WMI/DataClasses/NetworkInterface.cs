using System;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class NetworkInterface
	{
		public uint Received { get; set; }
		public uint Sent { get; set; }

		public NetworkInterface(uint received, uint sent)
		{
			Received = received;
			Sent = sent;
		}

		public NetworkInterface()
		{
			Received = 0;
			Sent = 0;
		}
	}
}