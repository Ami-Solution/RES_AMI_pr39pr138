using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UserStart
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Please wait while application is beeing started");

			Process.Start(@"..\..\..\AMI_System_Management\bin\Debug\AMY_System_Management.exe");
			Thread.Sleep(1500);

			Process.Start(@"..\..\..\AMI_Agregator\bin\Debug\AMI_Agregator.exe");
			Thread.Sleep(1500);

			Process.Start(@"..\..\..\AMI_Device\bin\Debug\AMI_Device.exe");
			Thread.Sleep(1500);
		}
	}
}
