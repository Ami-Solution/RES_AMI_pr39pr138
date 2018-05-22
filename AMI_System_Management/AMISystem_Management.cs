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

namespace AMI_System_Management
{
	public class AMISystem_Management : IAMI_System_Management
	{
		public AMISystem_Management()
		{

		}

		public bool SendDataToSystemDataBase(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			bool retVal = false;
			
			Trace.WriteLine($"Agregat: {agregator_code}, broj redova: {dateTimeList.Count()}, pocetak: {DateTime.Now} !");
			if (buffer.Count != 0)
			{
				string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
				using (SqlConnection con = new SqlConnection(CS))
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
						string query = $"INSERT INTO AMI_Tables(Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
						$"VALUES";

						Device_Code = keyValue.Key;

						foreach (var pair in keyValue.Value)
						{

							if(pair.Key == TypeMeasurement.CurrentP)
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

					//brisanje vremena
					
				}
			}

			Trace.WriteLine($"Agregat: {agregator_code}, ispraznjen buffer, kraj: {DateTime.Now} !");
			return retVal;
		}

		public static List<string> GetAllAgregatorsFromDataBase()
		{
			List<string> retVal = new List<string>();

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				string query = "SELECT DISTINCT(Agregator_Code) FROM AMI_Tables";

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

		public static List<string> GetAllDevicesFromDataBase()
		{
			List<string> retVal = new List<string>();

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				string query = "SELECT DISTINCT(Device_Code) FROM AMI_Tables";

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

		//zbog biranja datuma, ova metoda vraca najraniji datum moguci
		public static DateTime GetEarliestDateFromDatabase(string name)
		{
			DateTime retVal;

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				string query = $"SELECT MIN(DateAndTime) FROM AMI_Tables WHERE Device_Code like '{name}' or Agregator_Code like '{name}'";

				SqlCommand cmd = new SqlCommand(query, con);

				retVal = Convert.ToDateTime(cmd.ExecuteScalar());

			}

			return retVal;
		}

		public static Dictionary<DateTime, double> GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate)
		{
			Dictionary<DateTime, double> retVal = new Dictionary<DateTime, double>();

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				string query = $"SELECT {typeMeasurment}, DateAndTime FROM AMI_Tables WHERE Device_Code like '{device_code}' AND DateAndTime >= '{selectedDate}' AND DateAndTime < '{selectedDate.Date.AddDays(1)}'";

				SqlCommand cmd = new SqlCommand(query, con);

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						retVal.Add(Convert.ToDateTime(rdr["DateAndTime"]), Convert.ToDouble(rdr[typeMeasurment]));
					}
				}
			}


			return retVal;
		}

		public static Dictionary<DateTime, List<double>> GetDatesAndValuesFromDataBase(string device_code,  DateTime selectedDate)
		{
			Dictionary<DateTime, List<double>> retVal = new Dictionary<DateTime, List<double>>();

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_System"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				string query = $"SELECT CurrentP, Voltage, ActivePower, ReactivePower, DateAndTime FROM AMI_Tables WHERE Device_Code like '{device_code}' AND DateAndTime >= '{selectedDate}' AND DateAndTime < '{selectedDate.Date.AddDays(1)}'";
				SqlCommand cmd = new SqlCommand(query, con);
				con.Open();

				//ovde ce biti vrednosti,napone,struje,snage,reaktivne snage, bas tim redom
				List<double> vrednostiMerenja = new List<double>();

				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						vrednostiMerenja.Add(Convert.ToDouble(rdr["Voltage"]));
						vrednostiMerenja.Add(Convert.ToDouble(rdr["CurrentP"]));
						vrednostiMerenja.Add(Convert.ToDouble(rdr["ActivePower"]));
						vrednostiMerenja.Add(Convert.ToDouble(rdr["ReactivePower"]));
						retVal.Add(Convert.ToDateTime(rdr["DateAndTime"]), vrednostiMerenja); // u recnik dodajemo: Datum - napon,struja,snaga,reaktivna snaga
					}
				}

			}

			return retVal;
		}

		

	}
}
