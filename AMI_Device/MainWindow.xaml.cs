using Common;
using Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
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

            this.DataContext = this;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AMICharacteristics ami = new AMICharacteristics();
            double V = rand.Next(300);
            double I = rand.Next(300);
            double P = rand.Next(300);
            double S = rand.Next(300);

            ami.Measurements.Add(TypeMeasurement.Voltage, V);
            ami.Measurements.Add(TypeMeasurement.CurrentP, I);
            ami.Measurements.Add(TypeMeasurement.ActivePower, P);
            ami.Measurements.Add(TypeMeasurement.ReactivePower, S);

            AgregatorChoosing choosingWindow = new AgregatorChoosing(); // za biranje Agregata
            choosingWindow.ShowDialog();

			if (AgregatorChoosing.agregatorName != null)
			{

				ami.AgregatorID = AgregatorChoosing.agregatorName; //dodelimo izabrani agregat (agregator1)

				string substraction = ami.AgregatorID.Substring(9);
				int ID = Int32.Parse(substraction);
				ami.Connect(ID);
				substraction = "agregator" + ID;

                if(AvailableAMIDevices.ContainsKey(ami.Device_code))
                {
                    if(AvailableAMIDevices[ami.Device_code].AgregatorID != ami.AgregatorID)
                    {
                        ami.Device_code += P;
                        AvailableAMIDevices.Add(ami.Device_code, ami); //ako je name isti, ali razl agr, dodaj
                    }
                }else
                {
                    AvailableAMIDevices.Add(ami.Device_code, ami); //ako nema, dodaj ga
                }
				

				if (!ami.Proxy.AddDevice(substraction, ami.Device_code))
				{
					AddBtn_Click(sender, e);
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
						AvailableAMIDevices[keyValue.Key].SendDataToAgregator(AvailableAMIDevices[keyValue.Key]);
						this.Dispatcher.Invoke(() =>
						{
							dataGrid.Items.Refresh();
						});
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

    }
}

