using Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Device
{
	public class DeviceDB : IDeviceDB
	{
		private static string CS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\Enums\AMI_Agregator.mdf;Integrated Security=True";

		public DeviceDB()
		{

		}

		public void LoadNotAddedDevices()
		{
			using (SqlConnection con = new SqlConnection(CS))
			{
				SqlCommand cmd = new SqlCommand();
				con.Open();
				cmd.Connection = con;

				cmd.CommandText = "SELECT Agregator_Code, Device_Code FROM Devices_Table";

				//sada izaberemo sve uredjaje koji postoje u sistemu, i gledamo da li smo ih vec dodali 
				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						string agregator_code = rdr["Agregator_Code"].ToString();
						string device_code = rdr["Device_Code"].ToString();

						if (!MainWindow.AvailableAMIDevices.ContainsKey(device_code))
						{
							AMICharacteristics newAmi = new AMICharacteristics(device_code, agregator_code);
							newAmi.Added = false;
							MainWindow.AvailableAMIDevices.Add(device_code, newAmi);
						}

					}
				}
			}
		}


		public void RemoveDeviceFromDataBase(string device_code)
		{
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM Devices_Table WHERE Device_Code like '{device_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		public void SaveDeviceToDataBase(string agregator_code, string device_code)
		{
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO [Devices_Table](Agregator_Code, Device_Code) VALUES('{agregator_code}', '{device_code}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}
	}
}
