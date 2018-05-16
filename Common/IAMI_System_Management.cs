using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IAMI_System_Management
	{
		bool SendDataToDataBase(string agregator_code, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer);
	}
}
