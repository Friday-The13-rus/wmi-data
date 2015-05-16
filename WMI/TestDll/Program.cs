using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMI;

namespace TestDll
{
	class Program
	{
		static void Main(string[] args)
		{
			DataReturner dr = new DataReturner();
			dr.Start();

			Console.ReadLine();
			dr.Stop();
		}
	}
}
