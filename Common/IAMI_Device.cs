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
    public interface IAMI_Device
    {
        [OperationContract]
        void Connect(int id);
        [OperationContract]
        void SendDataToAgregator(string agrID, string devID, Dictionary<TypeMeasurement, double> measurement);
    }
}
