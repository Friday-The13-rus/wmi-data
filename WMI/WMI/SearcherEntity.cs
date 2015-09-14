using System.Management;

namespace WMI
{
	class SearcherEntity<T>
	{
		public ManagementObjectSearcher Searcher { get; private set; }
		public bool CanRemoveElements { get; private set; }
		public bool CanAddElements { get; private set; }
		public PropertySettersDictionary<T> PropertySetters { get; private set; }

		public SearcherEntity(ManagementObjectSearcher searcher, bool canAddElements, bool canRemoveElements, PropertySettersDictionary<T> propertySetters)
		{
			Searcher = searcher;
			CanAddElements = canAddElements;
			CanRemoveElements = canRemoveElements;
			PropertySetters = propertySetters;
		}
	}
}
