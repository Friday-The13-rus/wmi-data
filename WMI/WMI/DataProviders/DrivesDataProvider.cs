using System;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal class DrivesDataProvider : DataProvider<Drive>
	{
		public DrivesDataProvider(int updateInterval)
			: base(updateInterval, new TrimEndSlashesComparer())
		{
			AddSearcher("Win32_Volume",
				new PropertySettersDictionary<Drive>()
				{
					{"FreeSpace", (drive, o) => drive.FreeSpace = (ulong) o},
					{"Label", (drive, o) => drive.VolumeName = (string) o},
					{"Capacity", (drive, o) => drive.Space = (ulong) o},
					{"DriveLetter", (drive, o) => drive.DriveLetter = (string) o}
				}, "SystemVolume = false", canRemoveElements: true);
			AddSearcher("Win32_PerfFormattedData_PerfDisk_LogicalDisk",
				new PropertySettersDictionary<Drive>()
				{
					{"PercentDiskTime", (drive, o) => drive.ActivePercent = Convert.ToByte(o)}
				}, canAddElements: false);
		}
	}
}
