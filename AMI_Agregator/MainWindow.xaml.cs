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
		#region static fields/properities

		public static int agregatorNumber = 0;

		public static Dictionary<string, AMIAgregator> agregators { get; set; }

		private static ServiceHost host;

		public static AgregatorDB ADB = new AgregatorDB();

		#endregion

		#region constructors

		public MainWindow()
		{
			InitializeComponent();

			AMIAgregator initialise = new AMIAgregator();

			ADB.LoadAgregatorsFromLocalDataBase(); //ucitavanje podataka iz lokalne baze

			ADB.LoadAllDataFromLocalDataBase(); //ucitavanje agregatora koji nemaju podatke u lokalnoj bazi (poslali su sve, ili nisu ni imali)

			OpenServices();

			this.DataContext = this;
		}

		#endregion constructors

		#region methods

		//sluzi za ucitavanje agregatora i njegovih uredjaja(ako ih ima) koji nemaju podatke u lokalnoj bazi podataka
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

		private void AddBtn_Click(object sender, RoutedEventArgs e)
		{
			AMIAgregator agregator = new AMIAgregator("agregator" + (++agregatorNumber));
			agregators.Add(agregator.Agregator_code, agregator);
			ADB.SaveAgragatorToDataBase(agregator.Agregator_code);

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
				AMIAgregator.ADB.DeleteAgregatorFromLocalDatabase(keyValue.Key);
				agregators.Remove(keyValue.Key);
				//brisanje iz tabele koja sadrzi sve agregatore
				ADB.RemoveAgregatorFromDataBase(keyValue.Key);

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

		#endregion methods
	}
}
