using System;
using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal interface IDataProvider<out T> : IDisposable
		where T : NamedObject, new()
	{
		T this[int index] { get; }
		T GetByName(string name);
		int Count { get; }
		T[] GetAll();
	}
}