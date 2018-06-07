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
		private static string CS_AMI_AGREGATOR = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Dalibor\Desktop\GithubRepos\RES_AMI_pr39pr138\Enums\AMI_Agregator.mdf;Integrated Security=True";

		public AgregatorDB()
		{

		}

        //brise CEO agregator i sve u njemu
		public void DeleteAgregatorFromLocalDatabase(string agregator_code) 
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM AMI_LocalData_Table WHERE Agregator_Code like '{agregator_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

        //brise odredjeni device iz odredjenog agregatora
		public void DeleteDeviceFromLocalDatabase(string agregator_code, string device_code)
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
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

			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO [AMI_LocalData_Table](Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
				$"VALUES('{agregator_code}', '{device_code}', {Voltage}, {CurrentP}, {ActivePower}, {ReactivePower}, '{dateTime}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}

		//sluzi za ucitavanje agregatora i njegovih uredjaja koji nisu poslati u globanu bazu podataka
		public void LoadAllDataFromLocalDataBase()
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				SqlCommand cmd = new SqlCommand();
				con.Open();
				cmd.Connection = con;
				cmd.CommandText = "SELECT DISTINCT(Agregator_Code) FROM AMI_LocalData_Table";

				List<string> agregator_codes = new List<string>();

				//svi agregati se izdvoje ovde
				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						MainWindow.agregatorNumber++;
						string agregator_code = rdr["Agregator_Code"].ToString();
						MainWindow.agregators.Add(agregator_code, new AMIAgregator(agregator_code));
						agregator_codes.Add(agregator_code);

					}
				}

				//treba mi Device_Code, Dictionary<TypeMeasurement, List<double>>>()
				for (int i = 0; i < agregator_codes.Count; i++)
				{
					cmd.CommandText = $"SELECT DISTINCT(Device_Code) FROM AMI_LocalData_Table where Agregator_Code like '{agregator_codes[i]}'";

					using (SqlDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							string device_code = rdr["Device_Code"].ToString();
							MainWindow.agregators[agregator_codes[i]].Buffer.Add(device_code, new Dictionary<TypeMeasurement, List<double>>());
							MainWindow.agregators[agregator_codes[i]].ListOfDevices.Add(device_code);
						}
					}
				}

				// prolazimo kroz recnik: agregator_code, agregator(objekat)
				foreach (var keyValue in MainWindow.agregators)
				{
					//prolazimo kroz sve agregate(objekte) iz gore navedenog recnika
					foreach (var keyValue2 in MainWindow.agregators.Values)
					{
						List<DateTime> dateTimes = new List<DateTime>();
						//prolazimo kroz svaki baffer svakog agregata
						foreach (var keyValue3 in keyValue2.Buffer)
						{
							List<double> Voltage = new List<double>();
							List<double> CurrentP = new List<double>();
							List<double> ActivePower = new List<double>();
							List<double> ReactivePower = new List<double>();

							cmd.CommandText = $"SELECT * FROM AMI_LocalData_Table where Agregator_Code like '{keyValue2.Agregator_code}' and Device_Code like '{keyValue3.Key}'";

							using (SqlDataReader rdr = cmd.ExecuteReader())
							{
								while (rdr.Read())
								{
									Voltage.Add(Convert.ToDouble(rdr["Voltage"]));
									CurrentP.Add(Convert.ToDouble(rdr["CurrentP"]));
									ActivePower.Add(Convert.ToDouble(rdr["ActivePower"]));
									ReactivePower.Add(Convert.ToDouble(rdr["ReactivePower"]));
									dateTimes.Add(Convert.ToDateTime(rdr["DateAndTime"]));
								}
							}

							keyValue2.Buffer[keyValue3.Key][TypeMeasurement.ActivePower] = ActivePower;
							keyValue2.Buffer[keyValue3.Key][TypeMeasurement.ReactivePower] = ReactivePower;
							keyValue2.Buffer[keyValue3.Key][TypeMeasurement.Voltage] = Voltage;
							keyValue2.Buffer[keyValue3.Key][TypeMeasurement.CurrentP] = CurrentP;
							keyValue2.Dates[keyValue3.Key] = dateTimes;
						}


					}
				}

			}
		}

		//sluzi za ucitavanje agregatora i njegovih uredjaja(ako ih ima) koji nemaju podatke u lokalnoj bazi podataka
		public void LoadAllAgregators()
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				SqlCommand cmd = new SqlCommand();
				con.Open();
				cmd.Connection = con;
				cmd.CommandText = "SELECT DISTINCT(Agregator_Code) FROM Agregators_Table";

				//izaberemo sve agregate koji postoje, i proveravamo da li smo ih vec dodali u funkciji koja ucitava agregate
				//iz lokalne baze podataka gde se nalaze neposlati podaci
				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						string agregator_code = rdr["Agregator_Code"].ToString();

						if (!MainWindow.agregators.ContainsKey(agregator_code))
						{
							MainWindow.agregatorNumber++;
							MainWindow.agregators.Add(agregator_code, new AMIAgregator(agregator_code));
						}
					}
				}

			}
		}

		//prilikom dodavanja novo agregatora, doda se i u bazu koja sadrzi sve agregatore
		public void RemoveAgregatorFromDataBase(string agregator_code)
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM Agregators_Table WHERE Agregator_Code like '{agregator_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		public void SaveAgragatorToDataBase(string agregator_code)
		{
			using (SqlConnection con = new SqlConnection(CS_AMI_AGREGATOR))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO [Agregators_Table](Agregator_Code) VALUES('{agregator_code}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}
	}
}
