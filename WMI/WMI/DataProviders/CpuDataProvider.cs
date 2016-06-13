using System;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal class CpuDataProvider : DataProvider<Core>
	{
		public CpuDataProvider(int updateInterval)
			: base(updateInterval)
		{
			AddSearcher("Win32_PerfFormattedData_PerfOS_Processor",
				new PropertySettersDictionary<Core>()
				{
					{"PercentProcessorTime", (core, o) => core.UsePercent = Convert.ToByte(o)}
				});
		}
	}
}
