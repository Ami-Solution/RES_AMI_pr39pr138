using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IAgregatorDB
	{
		void DeleteAgregatorFromLocalDatabase(string agregator_code);

		void DeleteDeviceFromLocalDatabase(string agregator_code, string device_code);

		void SendToLocalDatabase(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values);

		void LoadAllDataFromLocalDataBase();

		void LoadAllAgregators();

		void SaveAgragatorToDataBase(string agregator_code);

		void RemoveAgregatorFromDataBase(string agregator_code);

	}
}
