using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
	public interface IAMI_Agregator
	{
        [OperationContract]
		string AddDevice(string agregator_code, string device_code);
        [OperationContract]
        void ReceiveDataFromDevice(string agregator_code, DateTime datetime, string device_code, Dictionary<TypeMeasurement, double> values);
        [OperationContract]
        Dictionary<string, List<string>> AgregatorsAndTheirDevices();
	}
}
