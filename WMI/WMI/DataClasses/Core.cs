using System;
using System.Management;
using System.Runtime.InteropServices;

namespace WMI.DataClasses
{
	[ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]
	public class Core : IComparable<Core>, IUpdatable<Core>, IUpdatable<ManagementBaseObject>
	{
		public string Name { get; private set; }
		public byte UsePercent { get; set; }

		public Core(ManagementBaseObject managementBaseObject)
		{
			Name = managementBaseObject.GetName();
			Update(managementBaseObject);
		}

		public Core(string name, byte percent)
		{
			Name = name;
			UsePercent = percent;
		}

		public int CompareTo(Core other)
		{
			return string.CompareOrdinal(Name, other.Name);
		}

		public void Update(Core other)
		{
			UsePercent = other.UsePercent;
		}

		public void Update(ManagementBaseObject other)
		{
			UsePercent = Convert.ToByte(other["PercentProcessorTime"]);
		}
	}
}