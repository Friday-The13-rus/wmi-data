using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	class DrivesDataProvider : DataProvider<Drive>
	{
		public DrivesDataProvider(int updateInterval)
			: base(updateInterval)
		{
			AddSearcher(new SelectQuery("SELECT Name, FreeSpace, VolumeName, Size FROM Win32_LogicalDisk WHERE Access != null"), UpdateGeneralInfo);
			AddSearcher(new SelectQuery("SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk"), UpdatePersentDiskTimeInfo);
		}

		private void UpdateGeneralInfo(ManagementObjectSearcher searcher)
		{
			HashSet<string> existingDrives = new HashSet<string>(_data.Keys);
			foreach (ManagementObject obj in searcher.Get())
			{
				string name = obj.GetName();

				Drive currentDrive;
				if (!_data.TryGetValue(name, out currentDrive))
				{
					AddElement(name, new Drive(obj));
				}
				else
				{
					UpdateElement(currentDrive, obj);
					existingDrives.Remove(name);
				}
			}
			foreach (var existingDrive in existingDrives)
			{
				_data.Remove(existingDrive);
			}
		}

		private void UpdatePersentDiskTimeInfo(ManagementObjectSearcher searcher)
		{
			foreach (ManagementObject obj in searcher.Get())
			{
				Drive drive;
				string driveName = obj.GetName();
				if (_data.TryGetValue(driveName, out drive))
				{
					UpdateElement(drive, obj);
				}
			}
		}
	}
}
