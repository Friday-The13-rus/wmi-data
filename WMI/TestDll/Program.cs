using System;
using WMI;
using Timer = System.Timers.Timer;

namespace TestDll
{
	class Program
	{
		readonly static Timer Timer = new Timer(1000);

		static void Main(string[] args)
		{
			DataReturner dr = new DataReturner();
			dr.Start();

			WriteData(dr);

			Console.ReadLine();

			Timer.Stop();

			dr.Stop();
		}

		private static void WriteData(DataReturner dr)
		{
			Timer.Elapsed += (sender, args) =>
			{
				for (int i = 0; i < dr.GetCoresCount(); i++)
				{
					var core = dr.GetProcessorData(i);
					Console.WriteLine("C {0} - {1}", core.Name, core.UsePercent);
				}
				for (int i = 0; i < dr.GetDrivesCount(); i++)
				{
					var drive = dr.GetDriveData(i);
					Console.WriteLine("D {0} - {1}", drive.Name, drive.ActivePercent);
				}
			};
			Timer.Start();
		}
	}
}
