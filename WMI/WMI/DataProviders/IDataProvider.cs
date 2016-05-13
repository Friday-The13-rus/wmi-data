using WMI.DataClasses;

namespace WMI.DataProviders
{
	internal interface IDataProvider<out T> where T : NamedObject, new()
	{
		void Dispose();
		T this[int index] { get; }
		T GetByName(string name);
		int Count { get; }
		T[] GetAll();
	}
}