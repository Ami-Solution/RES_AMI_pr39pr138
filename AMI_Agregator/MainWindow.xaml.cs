using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows;

namespace AMI_Agregator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static string CS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AMI_Agregator.mdf;Integrated Security=True;MultipleActiveResultSets=True";
		//predefinisan dictionari koji sadrzi: agregator_id i sam agregator.
		//sam agregator sadrzi svoj id, i buffer
		//buffer je dictionari i cine ga: device_id i novi dictionari: typeMeasurment i lista vrednost
		public static Dictionary<string, AMIAgregator> agregators { get; set; }

		public static int agregatorNumber = 0;

		private static ServiceHost host;
		public MainWindow()
		{
			InitializeComponent();
			AMIAgregator initialise = new AMIAgregator();

			LoadFromLocalDataBase(); //ucitavanje podataka iz lokalne baze

			LoadAgregatorsFromLocalDataBase(); //ucitavanje agregatora koji nemaju podatke u lokalnoj bazi (poslali su sve, ili nisu ni imali)

			OpenServices();

			this.DataContext = this;
		}

		//sluzi za ucitavanje agregatora i njegovih uredjaja(ako ih ima) koji nemaju podatke u lokalnoj bazi podataka
		private void LoadAgregatorsFromLocalDataBase()
		{
			//string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
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

						if (!agregators.ContainsKey(agregator_code))
						{
							MainWindow.agregatorNumber++;
							agregators.Add(agregator_code, new AMIAgregator(agregator_code));
						}
					}
				}

			}
		}

		//sluzi za ucitavanje agregatora i njegovih uredjaja koji nisu poslati u globanu bazu podataka
		private void LoadFromLocalDataBase()
		{
			//string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				SqlCommand cmd = new SqlCommand();
				con.Open();
				cmd.Connection = con;
				cmd.CommandText = "SELECT DISTINCT(Agregator_Code) FROM AMI_LocalData_Table";

				Dictionary<string, Dictionary<TypeMeasurement, List<double>>> loadedBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();

				List<string> agregator_codes = new List<string>();

				//svi agregati se izdvoje ovde
				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						MainWindow.agregatorNumber++;
						string agregator_code = rdr["Agregator_Code"].ToString();
						agregators.Add(agregator_code, new AMIAgregator(agregator_code));
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
							agregators[agregator_codes[i]].Buffer.Add(device_code, new Dictionary<TypeMeasurement, List<double>>());
							agregators[agregator_codes[i]].listOfDevices.Add(device_code);
						}
					}
				}

				// prolazimo kroz recnik: agregator_code, agregator(objekat)
				foreach (var keyValue in agregators)
				{
					//prolazimo kroz sve agregate(objekte) iz gore navedenog recnika
					foreach (var keyValue2 in agregators.Values)
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

		private static void OpenServices()
		{
			NetTcpBinding binding = new NetTcpBinding();
			string address = $"net.tcp://localhost:8003/IAMI_Agregator";
			host = new ServiceHost(typeof(AMIAgregator));
			host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
			host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
			host.AddServiceEndpoint(typeof(IAMI_Agregator), binding, address);

			Start();
		}

		public static void Start()
		{
			try
			{
				host.Open();
				Console.WriteLine($"Default AMI Agregator connection started succesffully.");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Default AMI Agregator connection didn't started succesffully - error: {e.Message}");
			}

		}

		public static void Stop()
		{
			try
			{
				host.Close();
				Console.WriteLine($"Default AMI Agregator connection closed succesffully.");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Default AMI Agregator connection didn't close succesffully - error: {e.Message}");
			}

		}

		//prilikom dodavanja novo agregatora, doda se i u bazu koja sadrzi sve agregatore
		private void SaveAgragatorToDataBase(string agregator_code)
		{
			//string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO Agregators_Table(Agregator_Code) VALUES('{agregator_code}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}

		//prilikom brisanja agregatora
		private void RemoveAgregatorFromDataBase(string agregator_code)
		{
			//string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM Agregators_Table WHERE Agregator_Code like '{agregator_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		private void AddBtn_Click(object sender, RoutedEventArgs e)
		{
			AMIAgregator agregator = new AMIAgregator("agregator" + (++agregatorNumber));
			agregators.Add(agregator.Agregator_code, agregator);
			SaveAgragatorToDataBase(agregator.Agregator_code);

			dataGrid.Items.Refresh();
		}

		private void RmvBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMIAgregator> keyValue = (KeyValuePair<string, AMIAgregator>)dataGrid.SelectedItem;

				//prinudno slanje podataka u bazu, posto se agregator brise
				agregators[keyValue.Key].Proxy.SendDataToSystemDataBase(keyValue.Key, agregators[keyValue.Key].Dates, agregators[keyValue.Key].Buffer);
				//brisanje iz lokalne baze
				agregators[keyValue.Key].DeleteFromLocalDatabase(keyValue.Key);
				agregators.Remove(keyValue.Key);
				//brisanje iz tabele koja sadrzi sve agregatore
				RemoveAgregatorFromDataBase(keyValue.Key);

			}

			dataGrid.Items.Refresh();
		}

		private void turnOnBtn_Click(object sender, RoutedEventArgs e)
		{

			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMIAgregator> keyValue = (KeyValuePair<string, AMIAgregator>)dataGrid.SelectedItem;

				if (agregators[keyValue.Key].State == Storage.State.OFF)
				{
					agregators[keyValue.Key].State = Storage.State.ON;

					//task, zbog toga sto ce da se freezuje, i na svakih 5 minuta da salje podatke u local storage
					Task t = Task.Factory.StartNew(() =>
					{
						agregators[keyValue.Key].SendToSystemMenagement(agregators[keyValue.Key]);

					});
				}

			}

			dataGrid.Items.Refresh();
		}

		private void turnOffBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMIAgregator> keyValue = (KeyValuePair<string, AMIAgregator>)dataGrid.SelectedItem;
				if (agregators[keyValue.Key].State == Storage.State.ON)
				{
					agregators[keyValue.Key].State = Storage.State.OFF;
				}

			}

			dataGrid.Items.Refresh();
		}

	}
}
