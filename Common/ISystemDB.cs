using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface ISystemDB
	{
		bool SendDataToSystemDataBase(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer);

		List<string> GetAllAgregatorsFromDataBase();

		List<string> GetAllDevicesFromDataBase();

		DateTime GetEarliesOrLatesttDateFromDatabase(string DeviceCodeOrEARLIESTorLATEST);

		Dictionary<DateTime, double> GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate);

		List<Tuple<DateTime, double>> GetValuesFromDatabase(string code, string typeMeasurment, DateTime selectedDate, out int devicesCount);

		Dictionary<DateTime, List<double>> GetDatesAndValuesFromDataBase(string device_code, DateTime selectedDate);
	}
}
