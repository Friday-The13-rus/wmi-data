using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WMI.DataClasses;

namespace WMI
{
	[ComVisible(true),
	 Guid("A79AC85C-547C-3ED3-AD94-530DC4BBB672"),
	 ProgId("WMI.GetData"),
	 ClassInterface(ClassInterfaceType.None)]
	public class DataReturner : IWMI
	{
		private DataManager data;

		public void Start()
		{
			CreateTraceListener();
			Trace.WriteLine("dll started", DateTime.Now.ToString() + " INFO ");

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			data = new DataManager(1000);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			if (ex != null)
			{
				Trace.WriteLine(ex.ToString(), DateTime.Now.ToString() + " ERROR");
			}
		}

		private static void CreateTraceListener()
		{
			TraceListener listener = new TextWriterTraceListener(@"C:\SystemInfoGadgetDLL.log", "fileListener");
			Trace.Listeners.Add(listener);
			Trace.AutoFlush = true;
		}

		public void Stop()
		{
			data.Dispose();
			Trace.WriteLine("dll stopped", DateTime.Now.ToString() + " INFO ");
		}

		public Drive GetDriveData(int i)
		{
			return data.GetDriveData(i);
		}

		public int GetDrivesCount()
		{
			return data.GetDrivesCount();
		}

		public Core GetProcessorData(int i)
		{
			return data.GetProcessorData(i);
		}

		public int GetCoresCount()
		{
			return data.GetCoresCount();
		}

		public NetworkInterface GetNetworkData()
		{
			return data.GetNetworkData();
		}
	}
}