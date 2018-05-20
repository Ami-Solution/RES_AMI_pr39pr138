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

		public bool SendDataToDataBase(string agregator_code, List<DateTime> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			Trace.WriteLine($"Agregat: {agregator_code}, broj merenja: {dateTimeList.Count()}, pocetak: {DateTime.Now} !");
			if (buffer.Count != 0)
			{
				string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
				using (SqlConnection con = new SqlConnection(CS))
				{
					con.Open();
					SqlCommand cmd;

					string Device_Code = "";
					List<double> Voltage = new List<double>().ToList(); //to list zbog nekog buga
					List<double> CurrentP = new List<double>().ToList();
					List<double> ActivePower = new List<double>().ToList();
					List<double> ReactivePower = new List<double>().ToList();

					//dateTime = dateTimeList[0];
					//dateTimeList.RemoveAt(0);

					//ne salje dobro u bazu podataka, vreme je lose, i raspored vrednosti
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

						for (int i = 0; i < CurrentP.Count(); i++)
						{
							if (i == CurrentP.Count() - 1)
							{
								query += $" ('{agregator_code}', '{Device_Code}', {Voltage[i]}, {CurrentP[i]}, {ActivePower[i]}, {ReactivePower[i]}, '{dateTimeList[i]}')";
								break;
							}

							query += $" ('{agregator_code}', '{Device_Code}', {Voltage[i]}, {CurrentP[i]}, {ActivePower[i]}, {ReactivePower[i]}, '{dateTimeList[i]}'), ";

							// brisanje iz bafera nakon upisivanja u bazu
							
						}

						cmd = new SqlCommand(query, con);
						cmd.ExecuteReader();

					}

					//brisanje vremena
					dateTimeList = new List<DateTime>();
				}
			}

			Trace.WriteLine($"Agregat: {agregator_code}, ispraznjen buffer, kraj: {DateTime.Now} !");
			return true;
		}
	}
}
