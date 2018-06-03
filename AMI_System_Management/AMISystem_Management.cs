using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using AMY_System_Management;

namespace AMI_System_Management
{
    public class AMISystem_Management : IAMI_System_Management
    {
		public static SystemDB SDB = new SystemDB();

		public AMISystem_Management()
        {

        }

		public bool SendDataToSystemDataBase(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			return SDB.SendDataToSystemDataBase(agregator_code, dateTimeList, buffer);
		}
	}
}
