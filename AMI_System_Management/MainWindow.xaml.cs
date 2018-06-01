using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
		#region static fields

		private static ServiceHost host;

		public static IAMI_Agregator defaultProxy;

		//u amisystem managment nam treba instanca mainwindowa (kako bi osvezili listu agregata i uredjaja u comboboxu kada se upisu novi);
		private static bool servicesOpened = false;

		#endregion static fields

		public MainWindow()
		{
			InitializeComponent();

			AMISystem_Management ami_sys_man = new AMISystem_Management();

			if (!servicesOpened)
			{
				OpenServices();
				servicesOpened = true;
			}

		}

		#region methods

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

		public void RefreshAgregatorsAndDevices()
		{
			RoutedEventArgs e = new RoutedEventArgs();
			devicesLoadedComboBox_Loaded(deviceComboBox, e);
			agregatorsLoadedComboBox_Loaded(agregatorsComboBox, e);
		}

		private void devicesLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allDevices = AMISystem_Management.GetAllDevicesFromDataBase();
			allDevices.Insert(0, "ALL DEVICES");
			deviceComboBox.ItemsSource = allDevices;
			deviceComboBox.SelectedIndex = 0;
			deviceComboBox.Items.Refresh();
		}

		private void agregatorsLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allAgregators = AMISystem_Management.GetAllAgregatorsFromDataBase();
			allAgregators.Insert(0, "SELECT AGREGATOR");
			agregatorsComboBox.ItemsSource = allAgregators;
			agregatorsComboBox.SelectedIndex = 0;
			agregatorsComboBox.Items.Refresh();
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
				DateTime minDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase(deviceComboBox.SelectedValue.ToString());

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
				DateTime minDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase(agregatorsComboBox.SelectedValue.ToString());

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
			string typeMeasurment = typemeasurmentDeviceComboBox.SelectedItem.ToString();
			DateTime selectedDate = Convert.ToDateTime(deviceDatesComboBox.SelectedItem).Date;

			if (typemeasurmentDeviceComboBox.SelectedItem.ToString() == "SELECT TYPE")
			{

				//vrsi se Iscrtavanje prosečnog merenja za izabrani uređaj za izabrani datum 
				Dictionary<DateTime, List<double>> DatesAndValues = new Dictionary<DateTime, List<double>>();
				DatesAndValues = AMISystem_Management.GetDatesAndValuesFromDataBase(device_code, selectedDate);

				List<DateTime> dates = new List<DateTime>();
				List<double> CurrentP = new List<double>();
				List<double> Voltage = new List<double>();
				List<double> ActivePower = new List<double>();
				List<double> ReactivePower = new List<double>();

				double cur = 0;
				double vol = 0;
				double act = 0;
				double rea = 0;

				//redom ce ici vrednosti -> struja, napon, aktivna snaga, reaktivna snaga
				foreach (var keyValue in DatesAndValues)
				{
					dates.Add(keyValue.Key);
					/* ovde dobijamo sve vrednosti za struju, napon, snagu, reaktivnu snagu
					CurrentP.Add(keyValue.Value[0]);
					Voltage.Add(keyValue.Value[1]);
					ActivePower.Add(keyValue.Value[2]);
					ReactivePower.Add(keyValue.Value[3]);
					*/
					cur += keyValue.Value[0];
					vol += keyValue.Value[1];
					act += keyValue.Value[2];
					rea += keyValue.Value[3];
				}

				cur = cur / dates.Count();
				vol = vol / dates.Count();
				act = act / dates.Count();
				rea = rea / dates.Count();

				for (int i = 0; i < dates.Count(); i++)
				{
					CurrentP.Add(cur);
					Voltage.Add(vol);
					ActivePower.Add(act);
					ReactivePower.Add(rea);
				}

				IEnumerable<double> CurrentPEnumerable = CurrentP.Cast<double>();
				IEnumerable<double> VoltageEnumerable = Voltage.Cast<double>();
				IEnumerable<double> ActivePowerEnumerable = ActivePower.Cast<double>();
				IEnumerable<double> ReactivePowerEnumerable = ReactivePower.Cast<double>();

				List<IEnumerable<double>> vrednostiEnumerable = new List<IEnumerable<double>>()
					{
						CurrentPEnumerable,
						VoltageEnumerable,
						ActivePowerEnumerable,
						ReactivePowerEnumerable
					};
				List<string> measurmentsNames = new List<string>() { "CurrentP", "Voltage", "ActivePower", "ReactivePower" };
				List<Color> colors = new List<Color>() { Colors.Yellow, Colors.Red, Colors.Orange, Colors.Purple };
				var x = Enumerable.Range(0, dates.Count());


			}
			else
			{

				//vrsi se Izcrtavanje izabranog merenja za izabrani uređaj za izabrani datum
				Dictionary<DateTime, double> DatesAndValues = 
					AMISystem_Management.GetDatesAndValuesFromDataBase(device_code, typeMeasurment, selectedDate);

				List<double> vrednosti = new List<double>();

				int i = 0;
				foreach (var keyValue in DatesAndValues)
				{
					vrednosti.Add(keyValue.Value);
					i++;
				}

				
				


			}
		}

		private void plotAgregatorGraph_Click(object sender, RoutedEventArgs e)
		{
			//TODO implementirati da se iscrta graph za agregator
			//lines.Children.RemoveRange(0, lines.Children.Count);

			if (agregatorsComboBox.SelectedValue.ToString() == "SELECT AGREGATOR")
			{
				errorLabel.Content = "YOU MUST SELECT AGREGATOR";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			errorLabel.Content = "";

			if (typemeasurmentAgregatorComboBox.SelectedValue.ToString() == "SELECT TYPE")
			{
				errorLabel.Content = "YOU MUST SELECT TYPE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			errorLabel.Content = "";
			//plotter.Visibility = Visibility.Visible;

			string agregator_code = agregatorsComboBox.SelectedValue.ToString();
			string typeMeasurment = typemeasurmentAgregatorComboBox.SelectedItem.ToString();
			DateTime selectedDate = Convert.ToDateTime(agregatorDatesComboBox.SelectedItem).Date;

			if (((Button)sender).Content.ToString() == "PLOT SUM")
			{
				List<double> retrievedValues = AMISystem_Management.GetValuesFromDatabase(agregator_code, typeMeasurment, selectedDate);
				double valueSum = 0;
				List<double> valueSumRepeated = new List<double>();

				foreach (var number in retrievedValues)
				{
					valueSum += number;
				}

				for (int i = 0; i < retrievedValues.Count(); i++)
				{
					valueSumRepeated.Add(valueSum);
				}

				IEnumerable<double> vrednostiEnumerable = valueSumRepeated.Cast<double>();
				var x = Enumerable.Range(0, retrievedValues.Count());

				

			}
			else
			{
				List<double> retrievedValues = AMISystem_Management.GetValuesFromDatabase(agregator_code, typeMeasurment, selectedDate);
				double valueSum = 0;
				List<double> valueSumRepeated = new List<double>();

				foreach (var number in retrievedValues)
				{
					valueSum += number;
				}

				for (int i = 0; i < retrievedValues.Count(); i++)
				{
					valueSumRepeated.Add(valueSum / retrievedValues.Count());
				}

				IEnumerable<double> vrednostiEnumerable = valueSumRepeated.Cast<double>();
				var x = Enumerable.Range(0, retrievedValues.Count());

				
			}

		}

		private void clearButton_Click(object sender, RoutedEventArgs e)
		{
			//lines.Children.RemoveRange(0, lines.Children.Count);
			//plotter.Visibility = Visibility.Hidden;

			alarmStateDataGrid.Items.Clear();
			alarmStateDataGrid.Visibility = Visibility.Hidden;

			errorLabel.Content = "";
			alarmTextBox.Text = "";
		}

		private void deviceStartDatesAlarmComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> selectDate = new List<string>() { "START DATE" };
			List<DateTime> dates = new List<DateTime>();

			DateTime minDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase("EARLIEST");
			DateTime maxDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase("LATEST");

			while (minDate.Date <= maxDate)
			{
				dates.Add(minDate.Date);
				minDate = minDate.Date.AddDays(1);
			}

			deviceStartDatesAlarmComboBox.ItemsSource = dates;
			deviceStartDatesAlarmComboBox.SelectedIndex = 0;
		}

		private void deviceEndDatesAlarmComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> selectDate = new List<string>() { "START DATE" };
			List<DateTime> dates = new List<DateTime>();

			DateTime minDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase("EARLIEST");
			DateTime maxDate = AMISystem_Management.GetEarliesOrLatesttDateFromDatabase("LATEST");

			while (minDate.Date <= maxDate)
			{
				dates.Add(minDate.Date);
				minDate = minDate.Date.AddDays(1);
			}

			deviceEndDatesAlarmComboBox.ItemsSource = dates;
			deviceEndDatesAlarmComboBox.SelectedIndex = dates.Count() - 1;
		}

		private void alarmButton_Click(object sender, RoutedEventArgs e)
		{
			if (typemeasurmentAlarmComboBox.SelectedValue.ToString() == "SELECT TYPE")
			{
				errorLabel.Content = "YOU MUST SELECT TYPE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			int greater = ((DateTime)deviceStartDatesAlarmComboBox.SelectedItem).CompareTo((DateTime)deviceEndDatesAlarmComboBox.SelectedItem);

			if (greater == 1)
			{
				errorLabel.Content = "START DATE CAN'T BE GREATER THAN END DATE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			if (alarmTextBox.Text.ToString() == "")
			{
				errorLabel.Content = "YOU MUST ENTER NUMBER TO COMPARE TO";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			double rez = 0;
			if (!(Double.TryParse(alarmTextBox.Text.ToString(), out rez)))
			{
				errorLabel.Content = "YOU MUST ENTER NUMBER";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			errorLabel.Content = "";
			//plotter.Visibility = Visibility.Hidden;

			//TODO finish, zavrsiti izlistavanje dobijenih rezultata u datagrid
			

		}

		private void alarmComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> GreaterOrLess = new List<string>() { "<", ">" };
			greaterOrLowerComboBox.ItemsSource = GreaterOrLess;
			greaterOrLowerComboBox.SelectedIndex = 0;
		}
		
		#endregion methods

	}
}
