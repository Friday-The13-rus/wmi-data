using System;
using System.Management;

namespace WMI
{
	internal class SearcherEntity<T> : IDisposable
	{
		private bool _disposed;

		public ManagementObjectSearcher Searcher { get; }
		public bool CanRemoveElements { get; }
		public bool CanAddElements { get; }
		public PropertySettersDictionary<T> PropertySetters { get; }

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
