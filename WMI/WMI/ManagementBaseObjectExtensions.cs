using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Timers;

namespace WMI
{
	public static class ManagementBaseObjectExtensions
	{
		public static bool ContainsProperty(this ManagementBaseObject managementObject, string propertyName)
		{
			return managementObject.Properties.OfType<PropertyData>().FirstOrDefault(el => el.Name == propertyName) != null;
		}

		public static string GetTargetClass(this ManagementBaseObject managementBaseObject)
		{
			return (string) managementBaseObject["__Class"];
		}

		public static ManagementBaseObject GetTargetInstance(this ManagementBaseObject managementBaseObject)
		{
			return (ManagementBaseObject) managementBaseObject["TargetInstance"];
		}

		public static string GetName(this ManagementBaseObject managementBaseObject)
		{
			return (string) managementBaseObject["Name"];
		}

		public static bool IsInstanceDeletionEvent(this ManagementBaseObject managementBaseObject)
		{
			return managementBaseObject.Properties["PreviousInstance"].Origin == "__InstanceDeletionEvent";
		}

		public static bool IsInstanceCreationEvent(this ManagementBaseObject managementBaseObject)
		{
			return (string) managementBaseObject["PreviousInstance"] == "__InstanceCreationEvent";
		}
	}
}
