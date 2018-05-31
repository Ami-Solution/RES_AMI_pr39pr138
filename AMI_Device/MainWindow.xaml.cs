using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AMI_Device
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static Dictionary<string, AMICharacteristics> availableAMIDevices = new Dictionary<string, AMICharacteristics>();

		public static Dictionary<string, AMICharacteristics> AvailableAMIDevices { get => availableAMIDevices; set => availableAMIDevices = value; }

		private static Random rand = new Random();

		public static IAMI_Agregator defaultProxy; //za pocetku konekciju koja uvek traje

		public MainWindow()
		{
			InitializeComponent();

			AMICharacteristics ami = new AMICharacteristics();

			Connect(); //defaultni se poziva sa ovom

			LoadDevicesFromLocalDatabase(); //isto kao i kod agregatora, ucitavaju se uredjaji za koje vrednosti nisu poslate

			LoadNotAddeDevices(); //ucitavaju se uredjaji koji nemaju nijednu vrednost, a dodati su bili

			this.DataContext = this;
		}

		private void LoadNotAddeDevices()
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				SqlCommand cmd = new SqlCommand();
				con.Open();
				cmd.Connection = con;

				cmd.CommandText = "SELECT Agregator_Code, Device_Code FROM Devices_Table";

				//sada izaberemo sve uredjaje koji pripadaju odredjenim agregatima, i gledamo da li smo ih vec dodali u funkciji
				//koja ucitava uredjaje iz lokalne baze gde se nalaze neposlati podaci
				using (SqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						string agregator_code = rdr["Agregator_Code"].ToString();
						string device_code = rdr["Device_Code"].ToString();

						if (!AvailableAMIDevices.ContainsKey(device_code))
						{
							AMICharacteristics newAmi = new AMICharacteristics(device_code, agregator_code);
							newAmi.Added = false;
							AvailableAMIDevices.Add(device_code, newAmi);
						}

					}
				}
			}

			dataGrid.Items.Refresh();
		}

		private void LoadDevicesFromLocalDatabase()
		{
			//prihvatamo dictionary: agregator_code - device_codes (svi njegovi uredjaji)
			var list = defaultProxy.AgregatorsAndTheirDevices();

			//prolazimo kroz svaki par u recniku
			foreach (var keyValue in list)
			{
				//prolazimo kroz svaki uredjaj u listi
				foreach (var device_code in keyValue.Value)
				{
					AvailableAMIDevices.Add(device_code, new AMICharacteristics(device_code, keyValue.Key, true));
				}
			}

			dataGrid.Items.Refresh();
		}

		private void AddBtn_Click(object sender, RoutedEventArgs e)
		{
			AMICharacteristics ami = new AMICharacteristics();
			ami.GenerateRandomValues();

			AgregatorChoosing choosingWindow = new AgregatorChoosing(); // za biranje Agregata
			choosingWindow.ShowDialog();

			if (AgregatorChoosing.agregatorName != null)
			{
				ami.AgregatorID = AgregatorChoosing.agregatorName; //dodelimo izabrani agregat (agregator1)

				string substraction = ami.AgregatorID.Substring(9);
				int ID = Int32.Parse(substraction);
				ami.Connect(ID);

				if (AvailableAMIDevices.ContainsKey(ami.Device_code))
				{
					if (AvailableAMIDevices[ami.Device_code].AgregatorID != ami.AgregatorID)
					{
						ami.Device_code += 'P';
						AvailableAMIDevices.Add(ami.Device_code, ami); //ako je name isti, ali razl agr, dodaj
					}
				}
				else
				{
					AvailableAMIDevices.Add(ami.Device_code, ami); //ako nema, dodaj ga
					SaveDeviceToDataBase(ami.AgregatorID, ami.Device_code);
				}

				dataGrid.Items.Refresh();
			}
		}

		private void RmvBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMICharacteristics> obj = (KeyValuePair<string, AMICharacteristics>)dataGrid.SelectedItem;
				AvailableAMIDevices.Remove(obj.Key);
				dataGrid.Items.Refresh();
				RemoveDeviceFromDataBase(obj.Key);
				defaultProxy.RemoveDevice(obj.Value.AgregatorID, obj.Key); //mora i agregat da obrise uredjaj iz svog bafera
			}
		}

		private static void Connect()
		{
			var binding = new NetTcpBinding();
			string address = $"net.tcp://localhost:8003/IAMI_Agregator";
			ChannelFactory<IAMI_Agregator> factory = new ChannelFactory<IAMI_Agregator>(binding, new EndpointAddress(address));
			defaultProxy = factory.CreateChannel();


		}

		private void turnOnBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMICharacteristics> keyValue = (KeyValuePair<string, AMICharacteristics>)dataGrid.SelectedItem;

				if (AvailableAMIDevices[keyValue.Key].Status == State.OFF) // ukoliko spamujemo turn on, a vec je ukljucen, da ne pravi vise taskova
				{
					AvailableAMIDevices[keyValue.Key].Status = State.ON;

					Task t = Task.Factory.StartNew(() =>
					{
						while (AvailableAMIDevices.ContainsKey(keyValue.Key))  //dok imamo uredjaj u listi, slace podatke, ali samo -->
						{
							if (AvailableAMIDevices[keyValue.Key].Status == State.OFF) // --> ako je ukljucen. Zato sto ako obrisemo, treba da izadje iz petlje
								break;

							AvailableAMIDevices[keyValue.Key].SendDataToAgregator(AvailableAMIDevices[keyValue.Key].AgregatorID, keyValue.Key, AvailableAMIDevices[keyValue.Key].Measurements);
							this.Dispatcher.Invoke(() =>
							{
								dataGrid.Items.Refresh();
							});

							Thread.Sleep(1000);
						}
					});

				};

				dataGrid.Items.Refresh();
			}
		}

		private void turnOffBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMICharacteristics> keyValue = (KeyValuePair<string, AMICharacteristics>)dataGrid.SelectedItem;

				if (AvailableAMIDevices[keyValue.Key].Status == State.ON) // spam za turn off
				{
					AvailableAMIDevices[keyValue.Key].Status = State.OFF;
				};

				dataGrid.Items.Refresh();

			}

		}

		//prilikom dodavanja uredjaja, cuva se uredjaj u posebnoj tabeli koja ima device_code - agregator_code
		private void SaveDeviceToDataBase(string agregator_code, string device_code)
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO Devices_Table(Agregator_Code, Device_Code) VALUES('{agregator_code}', '{device_code}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}

		//prilikom brisanja uredjaja, brise se iz te tabele
		private void RemoveDeviceFromDataBase(string device_code)
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM Devices_Table WHERE Device_Code like '{device_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

	}
}

