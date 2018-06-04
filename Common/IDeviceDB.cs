using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public interface IDeviceDB
	{
		void LoadNotAddedDevices();

		void SaveDeviceToDataBase(string agregator_code, string device_code);

		void RemoveDeviceFromDataBase(string device_code);
	}
}
