using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Agregator
{
	public class AgregatorDB : IAgregatorDB
	{
		private string CS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Dalibor\Desktop\GithubRepos\RES_AMI_pr39pr138\Enums\AMI_Agregator.mdf;Integrated Security=True";

		public AgregatorDB()
		{

		}

		public void DeleteAgregatorFromLocalDatabase(string agregator_code)
		{
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM AMI_LocalData_Table WHERE Agregator_Code like '{agregator_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		public void DeleteDeviceFromLocalDatabase(string agregator_code, string device_code)
		{
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM AMI_LocalData_Table WHERE Agregator_Code like '{agregator_code}' and Device_Code like '{device_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		public void SendToLocalDatabase(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			double Voltage = 0;
			double CurrentP = 0;
			double ActivePower = 0;
			double ReactivePower = 0;

			//vadimo rednosti iz recninka
			foreach (var keyValue in values)
			{

				if (keyValue.Key == TypeMeasurement.CurrentP)
					CurrentP = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.ActivePower)
					ActivePower = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.Voltage)
					Voltage = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.ReactivePower)
					ReactivePower = keyValue.Value;

			}

			//string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO [AMI_LocalData_Table](Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
				$"VALUES('{agregator_code}', '{device_code}', {Voltage}, {CurrentP}, {ActivePower}, {ReactivePower}, '{dateTime}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}
	}
}
