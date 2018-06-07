using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMY_System_Management
{
	public class SystemDB : ISystemDB
	{
		private static string CS_AMI_SYSTEM = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Dalibor\Desktop\GithubRepos\RES_AMI_pr39pr138\Enums\AMI_System.mdf;Integrated Security=True";
		private static string CS_AMI_AGREGATOR = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Dalibor\Desktop\GithubRepos\RES_AMI_pr39pr138\Enums\AMI_Agregator.mdf;Integrated Security=True";

		//vraca agregatore kako bismo prikazali u combo boxu sve agr
		public List<string> GetAllAgregatorsFromDataBase()
		{
			List<string> retVal = new List<string>();

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = "SELECT DISTINCT(Agregator_Code) FROM [AMI_Tables]";

				SqlCommand cmd = new SqlCommand(query, con);

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						retVal.Add(rdr["Agregator_Code"].ToString());
					}
				}
			}

			return retVal;
		}

        // isto, samo device
		public List<string> GetAllDevicesFromDataBase()
		{
			List<string> retVal = new List<string>();

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = "SELECT DISTINCT(Device_Code) FROM [AMI_Tables]";

				SqlCommand cmd = new SqlCommand(query, con);

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						retVal.Add(rdr["Device_Code"].ToString());
					}
				}
			}

			return retVal;
		}

		public Dictionary<DateTime, double> GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate)
		{
			Dictionary<DateTime, double> retVal = new Dictionary<DateTime, double>();

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = $"SELECT {typeMeasurment}, DateAndTime FROM AMI_Tables WHERE Device_Code like '{device_code}' AND DateAndTime >= '{selectedDate}' AND DateAndTime < '{selectedDate.Date.AddDays(1)}'";

				SqlCommand cmd = new SqlCommand(query, con);

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						if (!retVal.ContainsKey(Convert.ToDateTime(rdr["DateAndTime"])))
							retVal.Add(Convert.ToDateTime(rdr["DateAndTime"]), Convert.ToDouble(rdr[typeMeasurment]));
					}
				}
			}

			return retVal;
		}

		public Dictionary<DateTime, List<double>> GetDatesAndValuesFromDataBase(string device_code, DateTime selectedDate)
		{
			Dictionary<DateTime, List<double>> retVal = new Dictionary<DateTime, List<double>>();

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = $"SELECT CurrentP, Voltage, ActivePower, Reactivepower, DateAndTime FROM AMI_Tables WHERE Device_Code like '{device_code}' AND DateAndTime >= '{selectedDate}' AND DateAndTime < '{selectedDate.Date.AddDays(1)}'";

				SqlCommand cmd = new SqlCommand(query, con);

				//redom ce ici vrednosti -> struja, napon, aktivna snaga, reaktivna snaga

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						List<double> Values = new List<double>();

						Values.Add(Convert.ToDouble(rdr["CurrentP"]));
						Values.Add(Convert.ToDouble(rdr["Voltage"]));
						Values.Add(Convert.ToDouble(rdr["ActivePower"]));
						Values.Add(Convert.ToDouble(rdr["ReactivePower"]));

						if(!retVal.ContainsKey(Convert.ToDateTime(rdr["DateAndTime"])))
							retVal.Add(Convert.ToDateTime(rdr["DateAndTime"]), Values);

					}
				}
			}

			return retVal;
		}

		public DateTime GetEarliesOrLatesttDateFromDatabase(string DeviceCodeOrEARLIESTorLATEST)
		{
			DateTime retVal = DateTime.Now;

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = "";

				if (DeviceCodeOrEARLIESTorLATEST == "EARLIEST")
				{
					query = $"SELECT MIN(DateAndTime) FROM AMI_Tables";
				}
				else if (DeviceCodeOrEARLIESTorLATEST == "LATEST")
				{
					query = $"SELECT MAX(DateAndTime) FROM AMI_Tables";
				}
				else
				{
					query = $"SELECT MIN(DateAndTime) FROM AMI_Tables WHERE Device_Code like '{DeviceCodeOrEARLIESTorLATEST}' or Agregator_Code like '{DeviceCodeOrEARLIESTorLATEST}'";
				}

				SqlCommand cmd = new SqlCommand(query, con);

				if (!(cmd.ExecuteScalar() is DBNull))
				{
					retVal = Convert.ToDateTime(cmd.ExecuteScalar());
				}

			}

			return retVal;
		}

		//koristi se za iscrtavanje prosecnog/sumarnog grafa agregata
		public List<Tuple<DateTime, double>> GetValuesFromDatabase(string code, string typeMeasurment, DateTime selectedDate, out int devicesCount)
		{
			List<Tuple<DateTime, double>> retVal = new List<Tuple<DateTime, double>>();

			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = $"SELECT DateAndTime, {typeMeasurment} FROM AMI_Tables WHERE Agregator_Code like '{code}' AND DateAndTime >= '{selectedDate}' AND DateAndTime < '{selectedDate.Date.AddDays(1)}'";

				SqlCommand cmd = new SqlCommand(query, con);

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						retVal.Add(new Tuple<DateTime, double>(Convert.ToDateTime(rdr["DateAndTime"]), Convert.ToDouble(rdr[typeMeasurment])));
					}
				}
			}

			
			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = $"SELECT COUNT(DISTINCT Device_Code) FROM AMI_TABLES WHERE Agregator_Code LIKE '{code}'";

				SqlCommand cmd = new SqlCommand(query, con);
				object count = cmd.ExecuteScalar();
				devicesCount = Convert.ToInt32(count);

			}

			return retVal;
		}

		public bool SendDataToSystemDataBase(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			bool retVal = false;

			Trace.WriteLine($"Agregat: {agregator_code}, broj uredjaja u agregatu: {dateTimeList.Count()}!");
			if (buffer.Count != 0)
			{

				using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
				{
					con.Open();
					SqlCommand cmd;

					string Device_Code = "";
					List<double> Voltage = new List<double>().ToList(); //tolist zbog nekog buga
					List<double> CurrentP = new List<double>().ToList();
					List<double> ActivePower = new List<double>().ToList();
					List<double> ReactivePower = new List<double>().ToList();

					foreach (var keyValue in buffer)
					{
						Device_Code = keyValue.Key;

						Trace.WriteLine($"Agregat: {agregator_code}, broj merenja uredjaja {Device_Code}: {dateTimeList[keyValue.Key].Count()}, pocetak: {DateTime.Now} !");

						string query = $"INSERT INTO [AMI_Tables](Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
						$"VALUES";

						foreach (var pair in keyValue.Value)
						{

							if (pair.Key == TypeMeasurement.CurrentP)
								CurrentP = pair.Value;

							if (pair.Key == TypeMeasurement.ActivePower)
								ActivePower = pair.Value;

							if (pair.Key == TypeMeasurement.Voltage)
								Voltage = pair.Value;

							if (pair.Key == TypeMeasurement.ReactivePower)
								ReactivePower = pair.Value;

						}

						if (CurrentP.Count() > 0) // ako se neki uredjaj iskljuci, on ce i dalje biti u baferu, ali nece imati vrednosti
												  // za slanje u bazu podataka
						{

							for (int i = 0; i < CurrentP.Count(); i++)
							{
								if (i == CurrentP.Count() - 1)
								{
									query += $" ('{agregator_code}', '{Device_Code}', {Voltage[i]}, {CurrentP[i]}, {ActivePower[i]}, {ReactivePower[i]}, '{dateTimeList[Device_Code][i]}')";
									break;
								}

								query += $" ('{agregator_code}', '{Device_Code}', {Voltage[i]}, {CurrentP[i]}, {ActivePower[i]}, {ReactivePower[i]}, '{dateTimeList[Device_Code][i]}'), ";

							}

							cmd = new SqlCommand(query, con);
							cmd.ExecuteNonQuery();

							retVal = true;
						}

						dateTimeList[keyValue.Key] = new List<DateTime>();
					}

				}
			}

			Trace.WriteLine($"Agregat: {agregator_code}, ispraznjen buffer, kraj: {DateTime.Now} !");
			return retVal;
		}
	}
}
