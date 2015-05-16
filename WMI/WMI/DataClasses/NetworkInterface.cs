using System;
using System.Management;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class NetworkInterface : IUpdatable<NetworkInterface>, IUpdatable<ManagementBaseObject>
	{
		public ulong Received { get; set; }
		public ulong Sent { get; set; }

		public NetworkInterface(ManagementObject managementObject)
		{
			Update(managementObject);
		}

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

		public void Update(NetworkInterface other)
		{
			Received = other.Received;
			Sent = other.Sent;
		}

		public void Update(ManagementBaseObject other)
		{
			Received = (ulong)other["BytesReceivedPerSec"];
			Sent = (ulong)other["BytesSentPerSec"];
		}
	}
}