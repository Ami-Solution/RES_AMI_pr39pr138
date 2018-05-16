using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMY_System_Management
{
	public class AMISystem_Management : IAMI_System_Management
	{
		public AMISystem_Management()
		{

		}

		public bool SendDataToDataBase(string agregator_code, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			//implementirati logiku za slanje u bazu podataka
			Trace.WriteLine($"Agregat {agregator_code} je poslao buffer!");
			return true;
		}
	}
}
