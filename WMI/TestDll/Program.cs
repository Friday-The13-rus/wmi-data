using System;
using Newtonsoft.Json;
using WMI;
using Timer = System.Timers.Timer;

namespace TestDll
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var timer = new Timer(1000))
			using (var dataManager = new DataManager(1000))
			{
				timer.Elapsed += (sender, eventArgs) => { DumpData(dataManager); };
				timer.Start();
				Console.ReadLine();
			}
		}

		private static void DumpData(DataManager dr)
		{
			Console.WriteLine(JsonConvert.SerializeObject(dr.GetDriveData(), Formatting.Indented));
		}
	}
}
