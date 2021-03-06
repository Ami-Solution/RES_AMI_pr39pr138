﻿using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
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

		private void devicesLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allDevices = AMISystem_Management.SDB.GetAllDevicesFromDataBase();
			allDevices.Insert(0, "ALL DEVICES");
			deviceComboBox.ItemsSource = allDevices;
			deviceComboBox.SelectedIndex = 0;
			deviceComboBox.Items.Refresh();
		}

		private void agregatorsLoadedComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> allAgregators = AMISystem_Management.SDB.GetAllAgregatorsFromDataBase();
			allAgregators.Insert(0, "SELECT AGREGATOR");
			agregatorsComboBox.ItemsSource = allAgregators;
			agregatorsComboBox.SelectedIndex = 0;
			agregatorsComboBox.Items.Refresh();
		}

		private void buttonRefresh_Click(object sender, RoutedEventArgs e)
		{
			LoadDevicesAndAgregators();
		}

		private void LoadDevicesAndAgregators()
		{
			List<string> allAgregators = AMISystem_Management.SDB.GetAllAgregatorsFromDataBase();
			agregatorsComboBox.ItemsSource = allAgregators;
			allAgregators.Insert(0, "SELECT AGREGATOR");
			agregatorsComboBox.SelectedIndex = 0;
			agregatorsComboBox.Items.Refresh();

			List<string> allDevices = AMISystem_Management.SDB.GetAllDevicesFromDataBase();
			deviceComboBox.ItemsSource = allDevices;
			allDevices.Insert(0, "ALL DEVICES");
			deviceComboBox.SelectedIndex = 0;
			deviceComboBox.Items.Refresh();
		}

		private void deviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (deviceComboBox.SelectedValue != null)
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
					DateTime minDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase(deviceComboBox.SelectedValue.ToString());

					while (minDate.Date <= DateTime.Now.Date)
					{
						dates.Add(minDate.Date);
						minDate = minDate.Date.AddDays(1);
					}

					deviceDatesComboBox.ItemsSource = dates;
					deviceDatesComboBox.SelectedIndex = 0;
				}

			}
		}

		private void agregatorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (agregatorsComboBox.SelectedValue != null)
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
					DateTime minDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase(agregatorsComboBox.SelectedValue.ToString());

					while (minDate.Date <= DateTime.Now.Date)
					{
						dates.Add(minDate.Date);
						minDate = minDate.Date.AddDays(1);
					}

					agregatorDatesComboBox.ItemsSource = dates;
					agregatorDatesComboBox.SelectedIndex = 0;
				}
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

			CGraph.Children.RemoveRange(0, CGraph.Children.Count);
            alarmDataGrid.Visibility = Visibility.Hidden;
			GrafTab.Visibility = Visibility.Visible;

			errorLabel.Content = "";
			LMaxValue.Content = "300";
			LAvgValue.Content = "200";
			LMinValue.Content = "100";

			string device_code = deviceComboBox.SelectedValue.ToString();
			string typeMeasurment = typemeasurmentDeviceComboBox.SelectedItem.ToString();
			DateTime selectedDate = Convert.ToDateTime(deviceDatesComboBox.SelectedItem).Date;

			if (typemeasurmentDeviceComboBox.SelectedItem.ToString() == "SELECT TYPE")
			{

				//vrsi se Iscrtavanje prosečnog merenja za izabrani uređaj za izabrani datum. List<double> sadrzi redom struju-napon-aktivnu-reaktivnu
				Dictionary<DateTime, List<double>> DatesAndValues = new Dictionary<DateTime, List<double>>();
				DatesAndValues = AMISystem_Management.SDB.GetDatesAndValuesFromDataBase(device_code, selectedDate);

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
					cur += keyValue.Value[0];
					vol += keyValue.Value[1];
					act += keyValue.Value[2];
					rea += keyValue.Value[3];
				}

				typeLabel.Content = "YELLOW = I(A), ORANGE = U(V), BLUE = P(W), BLACK = Q(W)";

				List<double> curVolActRea = new List<double>() { cur / 86400, vol / 86400, act / 86400, rea / 86400 }; //ovo je prosecno merenje za ceo dan (kada nije radio, vrednost je 0)
				List<SolidColorBrush> colors = new List<SolidColorBrush>()
				{
					new SolidColorBrush(Colors.Yellow), // struja je zuta
					new SolidColorBrush(Colors.Orange), // napon je narandzast
					new SolidColorBrush(Colors.Blue), // akt snaga je plava
					new SolidColorBrush(Colors.Black) // reak snaga je crna
				};

				for (int i = 0; i < curVolActRea.Count; i++)
				{
					Line line = new Line
					{
						X1 = 0, // koordinate na x osi, od X do X+1 (po sekundama se pomeramo)
						X2 = 47715,

						Y1 = 330 - curVolActRea[i], //330 je 0, (0,330) --> koordinatni pocetak
						Y2 = 330 - curVolActRea[i],

						Stroke = colors[i],
					};

					CGraph.Children.Add(line);
				}

			}
			else
			{
				if (typeMeasurment == "Voltage") {
					typeLabel.Content = "U(V)";
				} else if (typeMeasurment == "CurrentP") {
					typeLabel.Content = "I(A)";
				} else if (typeMeasurment == "ActivePower") {
					typeLabel.Content = "P(W)";
				} else {
					typeLabel.Content = "P(W)";
				}
				
				//vrsi se Izcrtavanje izabranog merenja za izabrani uređaj za izabrani datum
				Dictionary<DateTime, double> DatesAndValues =
					AMISystem_Management.SDB.GetDatesAndValuesFromDataBase(device_code, typeMeasurment, selectedDate);

				Dictionary<int, double> secondsValues = new Dictionary<int, double>(); //svaki sekund u danu ima vrednost svoju

				foreach (var keyValue in DatesAndValues)
				{
					int seconds = keyValue.Key.Hour * 60 * 60 + keyValue.Key.Minute * 60 + keyValue.Key.Second;
					secondsValues.Add(seconds, keyValue.Value);

				}

				for (int i = 0; i < 86400; i++)
				{
					if (!secondsValues.ContainsKey(i))//ako ne postoji merenje za dati sekund, vrednost je 0
					{
						secondsValues.Add(i, 0);
					}
				}

				for (int i = 0; i < secondsValues.Count - 1; i++)
				{
					Line line = new Line
					{
						X1 = 0 + i * 0.552, // koordinate na x osi, od X do X+1 (po sekundama se pomeramo)
						X2 = 0.552 + i * 0.552,

						Y1 = 330 - secondsValues[i], //330 je 0, (0,330) --> koordinatni pocetak
						Y2 = 330 - secondsValues[i + 1],

						Stroke = new SolidColorBrush(Colors.Black)
					};

					CGraph.Children.Add(line);
				}

			}
		}

		private void plotAgregatorGraph_Click(object sender, RoutedEventArgs e)
		{
			if (agregatorsComboBox.SelectedValue.ToString() == "SELECT AGREGATOR")
			{
				errorLabel.Content = "YOU MUST SELECT AGREGATOR";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			if (typemeasurmentAgregatorComboBox.SelectedValue.ToString() == "SELECT TYPE")
			{
				errorLabel.Content = "YOU MUST SELECT TYPE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			errorLabel.Content = "";
			CGraph.Children.RemoveRange(0, CGraph.Children.Count);
            alarmDataGrid.Visibility = Visibility.Hidden;
            GrafTab.Visibility = Visibility.Visible;

			string agregator_code = agregatorsComboBox.SelectedValue.ToString();
			string typeMeasurment = typemeasurmentAgregatorComboBox.SelectedItem.ToString();
			DateTime selectedDate = Convert.ToDateTime(agregatorDatesComboBox.SelectedItem).Date;

			if (((Button)sender).Content.ToString() == "PLOT SUM")
			{
				if (typeMeasurment == "Voltage") //napon se ne sabira, nego se srednja vrednost se gleda. Jedina razlike je da ne povecavamo vrednosti na Y osi
				{

					typeLabel.Content = "U(V)";

					LMaxValue.Content = "300";
					LAvgValue.Content = "200";
					LMinValue.Content = "100";

					int devicesCount = 0;
					//imamo vreme, i vrednost u tom vremenu
					List<Tuple<DateTime, double>> retrievedValues = AMISystem_Management.SDB.GetValuesFromDatabase(agregator_code, typeMeasurment, selectedDate, out devicesCount);

					Dictionary<int, double> secondsValues = new Dictionary<int, double>();

					int seconds = 0;
					foreach (var tuple in retrievedValues)
					{
						seconds = tuple.Item1.Hour * 60 * 60 + tuple.Item1.Minute * 60 + tuple.Item1.Second;

						//proveravamo da li u recinku postoji vec taj trenutak u danu, ako postoji, dodamo vrednost na njega
						if (secondsValues.ContainsKey(seconds))
						{
							secondsValues[seconds] += tuple.Item2;
						}
						else //ako ne postoji, dodamo tu vrednost u tom trneutku u danu
						{
							secondsValues.Add(seconds, tuple.Item2);
						}

					}

					for (int i = 0; i < 86400; i++) //ako imamo neki treuntak u danu u kojem nema merenja, dodamo 0 tu
					{
						if (!secondsValues.ContainsKey(i))//ako ne postoji merenje za dati sekund, vrednost je 0
						{
							secondsValues.Add(i, 0);
						}
					}

					for (int i = 0; i < secondsValues.Count - 1; i++)
					{
						Line line = new Line
						{
							X1 = 0 + i * 0.552, // koordinate na x osi, od X do X+1 (po sekundama se pomeramo)
							X2 = 0.552 + i * 0.552,

							Y1 = 330 - (secondsValues[i]) / devicesCount, //330 je 0, (0,330) --> koordinatni pocetak.
							Y2 = 330 - (secondsValues[i + 1]) / devicesCount,

							Stroke = new SolidColorBrush(Colors.Black)
						};

						CGraph.Children.Add(line);
					}

				}
				else
				{
					if (typeMeasurment == "CurrentP")
					{
						typeLabel.Content = "I(A)";
					}
					else if (typeMeasurment == "ActivePower")
					{
						typeLabel.Content = "P(W)";
					}
					else
					{
						typeLabel.Content = "P(W)";
					}

					//broj uredjaja koji pripadaju trazenom agregatu
					int devicesCount = 0;
					//imamo vreme, i vrednost u tom vremenu
					List<Tuple<DateTime, double>> retrievedValues = AMISystem_Management.SDB.GetValuesFromDatabase(agregator_code, typeMeasurment, selectedDate, out devicesCount);

					LMaxValue.Content = devicesCount * 300;
					LAvgValue.Content = devicesCount * 200;
					LMinValue.Content = devicesCount * 100;

					//vreme cemo predstaviti u sekundama
					Dictionary<int, double> secondsValues = new Dictionary<int, double>();

					int seconds = 0;
					foreach (var tuple in retrievedValues)
					{
						seconds = tuple.Item1.Hour * 60 * 60 + tuple.Item1.Minute * 60 + tuple.Item1.Second;

						//proveravamo da li u recinku postoji vec taj trenutak u danu, ako postoji, dodamo vrednost na njega
						if (secondsValues.ContainsKey(seconds))
						{
							secondsValues[seconds] += tuple.Item2;
						}
						else //ako ne postoji, dodamo tu vrednost u tom trneutku u danu
						{
							secondsValues.Add(seconds, tuple.Item2);
						}

					}

					for (int i = 0; i < 86400; i++) //ako imamo neki treuntak u danu u kojem nema merenja, dodamo 0 tu
					{
						if (!secondsValues.ContainsKey(i))//ako ne postoji merenje za dati sekund, vrednost je 0
						{
							secondsValues.Add(i, 0);
						}
					}

					for (int i = 0; i < secondsValues.Count - 1; i++)
					{
						Line line = new Line
						{
							X1 = 0 + i * 0.552, // koordinate na x osi, od X do X+1 (po sekundama se pomeramo)
							X2 = 0.552 + i * 0.552,

							Y1 = 330 - (secondsValues[i]) / devicesCount, //330 je 0, (0,330) --> koordinatni pocetak.
							Y2 = 330 - (secondsValues[i + 1]) / devicesCount,

							Stroke = new SolidColorBrush(Colors.Black)
						};

						CGraph.Children.Add(line);
					}
				}
			}
			else //prosecno merenje za izabrani tip, isto kao i gore kod napona. Ne menjamo samo vrednosti na Y osi
			{
				if (typeMeasurment == "Voltage")
				{
					typeLabel.Content = "U(V)";
				}
				else if (typeMeasurment == "CurrentP")
				{
					typeLabel.Content = "I(A)";
				}
				else if (typeMeasurment == "ActivePower")
				{
					typeLabel.Content = "P(W)";
				}
				else
				{
					typeLabel.Content = "P(W)";
				}

				LMaxValue.Content = "300";
				LAvgValue.Content = "200";
				LMinValue.Content = "100";

				int devicesCount = 0;
				//imamo vreme, i vrednost u tom vremenu
				List<Tuple<DateTime, double>> retrievedValues = AMISystem_Management.SDB.GetValuesFromDatabase(agregator_code, typeMeasurment, selectedDate, out devicesCount);

				Dictionary<int, double> secondsValues = new Dictionary<int, double>();

				int seconds = 0;
				foreach (var tuple in retrievedValues)
				{
					seconds = tuple.Item1.Hour * 60 * 60 + tuple.Item1.Minute * 60 + tuple.Item1.Second;

					//proveravamo da li u recinku postoji vec taj trenutak u danu, ako postoji, dodamo vrednost na njega
					if (secondsValues.ContainsKey(seconds))
					{
						secondsValues[seconds] += tuple.Item2;
					}
					else //ako ne postoji, dodamo tu vrednost u tom trneutku u danu
					{
						secondsValues.Add(seconds, tuple.Item2);
					}

				}

				for (int i = 0; i < 86400; i++) //ako imamo neki treuntak u danu u kojem nema merenja, dodamo 0 tu
				{
					if (!secondsValues.ContainsKey(i))//ako ne postoji merenje za dati sekund, vrednost je 0
					{
						secondsValues.Add(i, 0);
					}
				}

				for (int i = 0; i < secondsValues.Count - 1; i++)
				{
					Line line = new Line
					{
						X1 = 0 + i * 0.552, // koordinate na x osi, od X do X+1 (po sekundama se pomeramo)
						X2 = 0.552 + i * 0.552,

						Y1 = 330 - (secondsValues[i]) / devicesCount, //330 je 0, (0,330) --> koordinatni pocetak.
						Y2 = 330 - (secondsValues[i + 1]) / devicesCount,

						Stroke = new SolidColorBrush(Colors.Black)
					};

					CGraph.Children.Add(line);
				}

			}

		}

		/* //za testiranje grafa       
		private static string CS_AMI_SYSTEM = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Dalibor\Desktop\GithubRepos\RES_AMI_pr39pr138\Enums\AMI_System.mdf;Integrated Security=True";
		private static System.Random rand = new System.Random();
		*/

		private void clearButton_Click(object sender, RoutedEventArgs e)
		{
			alarmDataGrid.Visibility = Visibility.Hidden;
			GrafTab.Visibility = Visibility.Hidden;

			deviceComboBox.SelectedIndex = 0;
			agregatorsComboBox.SelectedIndex = 0;

			deviceDatesComboBox.SelectedIndex = 0;
			agregatorDatesComboBox.SelectedIndex = 0;

			typemeasurmentAgregatorComboBox.SelectedIndex = 0;
			typemeasurmentAlarmComboBox.SelectedIndex = 0;
			typemeasurmentDeviceComboBox.SelectedIndex = 0;

			CGraph.Children.RemoveRange(0, CGraph.Children.Count);
			LMaxValue.Content = "";
			LAvgValue.Content = "";
			LMinValue.Content = "";
			errorLabel.Content = "";
			alarmTextBox.Text = "";
			typeLabel.Content = "";

			//ubacivanje u bazu podataka, radi testiranja grafa 
			/*
			using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				SqlCommand cmd;

				TimeSpan time;
				StringBuilder s;

				for (int i = 1200; i < 3000; i++)
				{
					s = new StringBuilder((DateTime.Now.Date).ToShortDateString());
					time = TimeSpan.FromSeconds(i);

					StringBuilder answer = new StringBuilder(string.Format("{0:D2}:{1:D2}:{2:D2}",
						time.Hours,
						time.Minutes,
						time.Seconds));

					s.Append(" " + answer);

					string query = $"INSERT INTO [AMI_Tables](Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
					$"VALUES('agregator10', 'deviceqwey', {rand.Next(300)}, {rand.Next(300)}, {rand.Next(300)}, {rand.Next(300)}, '{s.ToString()}')";

					cmd = new SqlCommand(query, con);
					cmd.ExecuteNonQuery();

				}

			}
			*/


		}

		private void deviceStartDatesAlarmComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			List<string> selectDate = new List<string>() { "START DATE" };
			List<DateTime> dates = new List<DateTime>();

			DateTime minDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("EARLIEST");
			DateTime maxDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("LATEST");

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

			DateTime minDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("EARLIEST");
			DateTime maxDate = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("LATEST");

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
			string typeMeasurment = (typemeasurmentAlarmComboBox.SelectedValue.ToString());
			string greatOrLower = greaterOrLowerComboBox.SelectedValue.ToString();

			if (typeMeasurment == "SELECT TYPE")
			{
				errorLabel.Content = "YOU MUST SELECT TYPE";
				errorLabel.FontSize = 20;
				errorLabel.Foreground = Brushes.Red;
				return;
			}

			DateTime startDate = (DateTime)deviceStartDatesAlarmComboBox.SelectedItem;
			DateTime endDate = (DateTime)deviceEndDatesAlarmComboBox.SelectedItem;

			int greater = (startDate.CompareTo(endDate));

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

			GrafTab.Visibility = Visibility.Hidden;
			alarmDataGrid.Visibility = Visibility.Visible;
			errorLabel.Content = "";

            string CS_AMI_SYSTEM = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\Enums\AMI_System.mdf;Integrated Security=True";
            using (SqlConnection con = new SqlConnection(CS_AMI_SYSTEM))
			{
				con.Open();
				string query = $"SELECT * FROM [AMI_Tables] WHERE {typeMeasurment} {greatOrLower} {rez} AND DateAndTime >= '{startDate}' AND DateAndTime < '{endDate.AddDays(1)}'";

				SqlCommand cmd = new SqlCommand(query, con);
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				DataTable dt = new DataTable("Informatons");
				da.Fill(dt);

				alarmDataGrid.ItemsSource = dt.DefaultView;

			}

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
