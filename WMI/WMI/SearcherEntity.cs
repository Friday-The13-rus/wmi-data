using System;
using System.Management;

namespace WMI
{
	class SearcherEntity<T> : IDisposable
	{
		private bool _disposed;

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

		public void Dispose()
		{
			if (!_disposed)
			{
				Searcher.Dispose();
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}
