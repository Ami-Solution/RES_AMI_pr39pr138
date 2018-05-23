using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
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

namespace AMI_System_Management
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static ServiceHost host;

		public static IAMI_Agregator defaultProxy;

		public MainWindow()
		{
			InitializeComponent();

			AMISystem_Management ami_sys_man = new AMISystem_Management();

			OpenServices(); 

		}

		private static void OpenServices()
		{
			NetTcpBinding binding = new NetTcpBinding();
			string address = $"net.tcp://localhost:8004/IAMI_System_Management";
			host = new ServiceHost(typeof(AMISystem_Management));
			host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
			host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
			host.AddServiceEndpoint(typeof(IAMI_System_Management), binding, address);

			Start();
		}

		public static void Start()
		{
			try
			{
				host.Open();
				Console.WriteLine($"Default AMI System Management connection started succesffully.");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Default AMI System Management connection didn't started succesffully - error: {e.Message}");
			}

		}

		public static void Stop()
		{
			try
			{
				host.Close();
				Console.WriteLine($"Default AMI System Management connection closed succesffully.");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Default AMI System Management connection didn't close succesffully - error: {e.Message}");
			}

		}

		private void devicesLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allDevices = AMISystem_Management.GetAllDevicesFromDataBase();
			allDevices.Insert(0, "ALL DEVICES");
			deviceComboBox.ItemsSource = allDevices;
			deviceComboBox.SelectedIndex = 0;
		}

		private void agregatorsLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allAgregators = AMISystem_Management.GetAllAgregatorsFromDataBase();
			allAgregators.Insert(0, "SELECT AGREGATOR");
			agregatorsComboBox.ItemsSource = allAgregators;
			agregatorsComboBox.SelectedIndex = 0;
		}

		private void deviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<string> selectDate = new List<string>() { "SELECT DATE" };
			List<DateTime> dates = new List<DateTime>();

			if (deviceComboBox.SelectedValue.ToString() == "ALL DEVICES")
			{
				deviceDatesComboBox.ItemsSource = selectDate;
				deviceDatesComboBox.SelectedIndex = 0;
			}
			else
			{
				DateTime minDate = AMISystem_Management.GetEarliestDateFromDatabase(deviceComboBox.SelectedValue.ToString());

				while (minDate.Date <= DateTime.Now.Date)
				{
					dates.Add(minDate.Date);
					minDate = minDate.Date.AddDays(1);
				}

				deviceDatesComboBox.ItemsSource = dates;
				deviceDatesComboBox.SelectedIndex = 0;
			}

		}

		private void agregatorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<string> selectDate = new List<string>() { "SELECT DATE" };
			List<DateTime> dates = new List<DateTime>();

			if (agregatorsComboBox.SelectedValue.ToString() == "SELECT AGREGATOR")
			{
				agregatorDatesComboBox.ItemsSource = selectDate;
				agregatorDatesComboBox.SelectedIndex = 0;
			}
			else
			{
				DateTime minDate = AMISystem_Management.GetEarliestDateFromDatabase(agregatorsComboBox.SelectedValue.ToString());

				while (minDate.Date <= DateTime.Now.Date)
				{
					dates.Add(minDate.Date);
					minDate = minDate.Date.AddDays(1);
				}

				agregatorDatesComboBox.ItemsSource = dates;
				agregatorDatesComboBox.SelectedIndex = 0;
			}
		}

		private void typemeasurmentComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> measurments = new List<string>() { "SELECT TYPE", "CurrentP", "Voltage", "ActivePower", "ReactivePower" };
			((ComboBox)sender).ItemsSource = measurments;
			((ComboBox)sender).SelectedIndex = 0;
		}

		private void plotDeviceGraph_Click(object sender, RoutedEventArgs e)
		{
			if (deviceComboBox.SelectedValue.ToString() == "ALL DEVICES")
			{
				errorLabel.Content = "YOU MUST SELECT DEVICE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			errorLabel.Content = "";

			string device_code = deviceComboBox.SelectedValue.ToString();
			string typeMeasurment = typemeasurmentComboBox.SelectedItem.ToString();
			DateTime selectedDate = Convert.ToDateTime(deviceDatesComboBox.SelectedItem).Date;

			if (typemeasurmentComboBox.SelectedItem.ToString() == "SELECT TYPE")
			{
				//vrsi se Iscrtavanje prosečnog merenja za izabrani uređaj za izabrani datum 
				//dobavljeni podaci za odredjeni uredjaj, sva njegova merenja, i vremena tih merenja
				Dictionary<DateTime, List<double>> DatesAndValues = AMISystem_Management.GetDatesAndValuesFromDataBase(device_code, selectedDate); 

			}
			else
			{
				
				//vrsi se Izcrtavanje izabranog merenja za izabrani uređaj za izabrani datum
				//dobavljeni odredjeni podaci(tipovi-struja ili napon ili snaga) za odredjeni uredjaj
				Dictionary<DateTime, double> DatesAndValues = AMISystem_Management.GetDatesAndValuesFromDataBase(device_code, typeMeasurment, selectedDate);

			}


		}

		private void plotAgregatorGraph_Click(object sender, RoutedEventArgs e)
		{
			//vrstaMerenja je suma ili prosek
			string vrstaMerenja = ((Button)sender).Content.ToString();

			//TODO implementirati da se iscrta graph za agregator
			if (vrstaMerenja == "PLOT SUM")
			{

			}
			else
			{

			}

			
		}
	}
}
