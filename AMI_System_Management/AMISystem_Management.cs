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

namespace AMY_System_Management
{
	public class AMISystem_Management : IAMI_System_Management
	{
		public AMISystem_Management()
		{

		}

		public bool SendDataToDataBase(string agregator_code, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				SqlCommand cmd = new SqlCommand($"INSERT INTO AMI_Tables(Agregator_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) VALUES('{agregator_code}', 1, 2, 3, 4, 1)", con);
				con.Open();
				cmd.ExecuteReader();

			}

			Trace.WriteLine($"Agregat {agregator_code} je poslao buffer!");
			return true;
		}
	}
}
