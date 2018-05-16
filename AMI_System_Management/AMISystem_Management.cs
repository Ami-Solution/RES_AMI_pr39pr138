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
			Trace.WriteLine($"Agregat: {agregator_code}, broj merenja: {dateTimeList.Count()*4}, pocetak: {DateTime.Now} !");
			if (buffer.Count != 0)
			{
				string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
				using (SqlConnection con = new SqlConnection(CS))
				{
					con.Open();
					SqlCommand cmd;

					string Device_Code = "";
					double Voltage = 0;
					double CurrentP = 0;
					double ActivePower = 0;
					double ReactivePower = 0;
					DateTime dateTime;

					//ne salje dobro u bazu podataka, vreme je lose, i raspored vrednosti
					foreach (var keyValue in buffer)
					{
						Device_Code = keyValue.Key;

						foreach (var pair in keyValue.Value)
						{
							dateTime = dateTimeList[0];
							dateTimeList.RemoveAt(0);

							for (int i = 0; i < pair.Value.Count(); i++)
							{
								
								Voltage = pair.Value[i];
								CurrentP = pair.Value[i];
								ActivePower = pair.Value[i];
								ReactivePower = pair.Value[i];

								cmd = new SqlCommand($"INSERT INTO AMI_Tables(Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) VALUES('{agregator_code}', '{Device_Code}', {Voltage}, {CurrentP}, {ActivePower}, {ReactivePower}, '{dateTime}')", con);
								cmd.ExecuteReader();

							}
						}
					}

					// brisanje iz bafera nakon upisivanja u bazu
					buffer[Device_Code].Remove(TypeMeasurement.ActivePower);
					buffer[Device_Code].Remove(TypeMeasurement.ReactivePower);
					buffer[Device_Code].Remove(TypeMeasurement.CurrentP);
					buffer[Device_Code].Remove(TypeMeasurement.Voltage);
				}
			}

			Trace.WriteLine($"Agregat: {agregator_code}, ispraznjen buffer, kraj: {DateTime.Now} !");
			return true;
		}
	}
}
