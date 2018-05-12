using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IAMI_Agregator
	{
		bool AddDevice(string agregator_code, string device_code);

		bool ReceiveDataFromDevice(string agregator_code, string device_code, Dictionary<TypeMeasurement, double> values);

		List<string> agregator_ids { get; set; }
	}
}
