using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal class RamDataProvider : DataProvider<Ram>
	{
		public RamDataProvider(int updateInterval) : base(updateInterval)
		{
			AddSearcher("Win32_OperatingSystem",
				new PropertySettersDictionary<Ram>() 
				{
					{"TotalVisibleMemorySize", (ram, o) => ram.Total = (ulong) o },
					{"FreePhysicalMemory", (ram, o) => ram.Free = (ulong) o }
				}
			);
		}
	}
}
