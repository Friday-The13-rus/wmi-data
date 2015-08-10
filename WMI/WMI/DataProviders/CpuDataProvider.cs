using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	class CpuDataProvider : DataProvider<Core>
	{
		public CpuDataProvider(int updateInterval)
			: base(updateInterval)
		{
			AddSearcher(new SelectQuery("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor"), Update);
		}

		private void Update(ManagementObjectSearcher searcher)
		{
			foreach (ManagementObject obj in searcher.Get())
			{
				string name = obj.GetName();

				Core currentCore;
				if (!_data.TryGetValue(name, out currentCore))
				{
					AddElement(name, new Core(obj));
				}
				else
				{
					UpdateElement(currentCore, obj);
				}
			}
		}
	}
}
