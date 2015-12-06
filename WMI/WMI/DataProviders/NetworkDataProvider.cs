using WMI.DataClasses;

namespace WMI.DataProviders
{
	class NetworkDataProvider : DataProvider<NetworkInterface>
	{
		public NetworkDataProvider(int updateInterval)
			: base(updateInterval)
		{
			AddSearcher("Win32_PerfFormattedData_Tcpip_NetworkInterface", new PropertySettersDictionary<NetworkInterface>()
			{
				{"BytesReceivedPerSec", (@interface, o) => @interface.Received = (ulong) o},
				{"BytesSentPerSec", (@interface, o) => @interface.Sent = (ulong) o}
			});
		}
	}
}
