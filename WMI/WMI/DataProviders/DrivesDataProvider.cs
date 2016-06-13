using System;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal class DrivesDataProvider : DataProvider<Drive>
	{
		public DrivesDataProvider(int updateInterval)
			: base(updateInterval)
		{
			AddSearcher("Win32_LogicalDisk",
				new PropertySettersDictionary<Drive>()
				{
					{"FreeSpace", (drive, o) => drive.FreeSpace = (ulong) o},
					{"VolumeName", (drive, o) => drive.VolumeName = (string) o},
					{"Size", (drive, o) => drive.Space = (ulong) o}
				}, "Access != null", canRemoveElements: true);
			AddSearcher("Win32_PerfFormattedData_PerfDisk_LogicalDisk",
				new PropertySettersDictionary<Drive>()
				{
					{"PercentDiskTime", (drive, o) => drive.ActivePercent = Convert.ToByte(o)}
				}, canAddElements: false);
		}
	}
}
