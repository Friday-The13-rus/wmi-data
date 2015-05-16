using System;
using System.Management;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Drive : IComparable<Drive>, IUpdatable<Drive>, IUpdatable<ManagementBaseObject>
	{
		public string Name { get; private set; }
		public string VolumeName { get; set; }
		public ulong FreeSpace { get; set; }
		public ulong Space { get; set; }

		public byte UsePercent
		{
			get { return Convert.ToByte(100 * (1 - (double) FreeSpace / Space)); }
		}

		public byte ActivePercent { get; set; }

		public Drive(ManagementBaseObject managementBaseObject)
		{
			Name = managementBaseObject.GetName();
			if (managementBaseObject.GetTargetClass() == "Win32_LogicalDisk")
				Space = (ulong) managementBaseObject["Size"];
			Update(managementBaseObject);
		}

		public Drive(string name, string volumeName, ulong freeSpace, ulong space, byte activePercent)
		{
			Name = name;
			VolumeName = volumeName;
			FreeSpace = freeSpace;
			Space = space;
			ActivePercent = activePercent;
		}

		public int CompareTo(Drive other)
		{
			return string.CompareOrdinal(Name, other.Name);
		}

		public void Update(Drive other)
		{
			VolumeName = other.VolumeName;
			Space = other.Space;
			FreeSpace = other.FreeSpace;
			ActivePercent = other.ActivePercent;
		}

		public void Update(ManagementBaseObject other)
		{
			switch (other.GetTargetClass())
			{
				case "Win32_LogicalDisk":
					FreeSpace = (ulong) other["FreeSpace"];
					VolumeName = (string) other["VolumeName"];
					break;
				case "Win32_PerfFormattedData_PerfDisk_LogicalDisk":
					ActivePercent = Convert.ToByte(other["PercentDiskTime"]);
					break;
			}
		}
	}
}