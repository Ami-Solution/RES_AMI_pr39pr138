using Common;
using Storage;
using System;
using System.Collections.Generic;
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
			throw new NotImplementedException();
		}
	}
}
