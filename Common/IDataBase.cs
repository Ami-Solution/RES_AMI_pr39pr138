using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IDataBase
	{
		void LoadNotAddeDevices();

		void SaveDeviceToDataBase(string agregator_code, string device_code);

		void RemoveDeviceFromDataBase(string device_code);

		void LoadAllDataFromLocalDataBase();

		void LoadAgregatorsFromLocalDataBase();

		void SaveAgragatorToDataBase(string agregator_code);

		void RemoveAgregatorFromDataBase(string agregator_code);

		void DeleteAgregatorFromLocalDatabase(string agregator_code);

		void DeleteDeviceFromLocalDatabase(string agregator_code, string device_code);

		void SendToLocalDatabase(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values);

		List<string> GetAllAgregatorsFromDataBase();

		List<string> GetAllDevicesFromDataBase();

		DateTime GetEarliesOrLatesttDateFromDatabase(string DeviceCodeOrEARLIESTorLATEST);

		Dictionary<DateTime, double> GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate);

		List<Tuple<DateTime, double>> GetValuesFromDatabase(string code, string typeMeasurment, DateTime selectedDate, out int devicesCount);

		Dictionary<DateTime, List<double>> GetDatesAndValuesFromDataBase(string device_code, DateTime selectedDate);
	}
}
