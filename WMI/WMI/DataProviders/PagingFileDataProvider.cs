using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal class PagingFileDataProvider : DataProvider<PagingFile>
	{
		public PagingFileDataProvider(int updateInterval) 
			: base(updateInterval)
		{
			AddSearcher("Win32_PerfFormattedData_PerfOS_PagingFile",
				new PropertySettersDictionary<PagingFile>() {{"PercentUsage", (file, o) => file.UsagePercent = (uint) o}},
				"Name = '_Total'");
		}
	}
}
