using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	class NetworkDataProvider : DataProvider<NetworkInterface>
	{
		public NetworkDataProvider()
		{
			AddSearcher(new SelectQuery("SELECT Name, BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface"), Update);
		}

		private void Update(ManagementObjectSearcher searcher)
		{
			foreach (ManagementObject obj in searcher.Get())
			{
				var name = obj.GetName();
				NetworkInterface networkInterface;
				if (!_data.TryGetValue(name, out networkInterface))
				{
					AddElement(name, new NetworkInterface(obj));
				}
				else
				{
					UpdateElement(networkInterface, obj);
				}
			}
		}
	}
}
