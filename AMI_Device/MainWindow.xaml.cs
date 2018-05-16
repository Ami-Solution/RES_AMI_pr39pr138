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
            ami.Measurements.Add(TypeMeasurement.Current, I);
            ami.Measurements.Add(TypeMeasurement.ActivePower, P);
            ami.Measurements.Add(TypeMeasurement.ReactivePower, S);

            AgregatorChoosing choosingWindow = new AgregatorChoosing(); // za biranje Agregata
            choosingWindow.ShowDialog();
            ami.AgregatorID = AgregatorChoosing.agregatorName; //dodelimo izabrani agregat (agregator1)

            string substraction = ami.AgregatorID.Substring(9);
            int ID = Int32.Parse(substraction);
            ami.Connect(ID);
            substraction = "agregator" + ID;

            AvailableAMIDevices.Add(ami.Device_code, ami);

            if (!ami.Proxy.AddDevice(substraction, ami.Device_code))
            {
                AddBtn_Click(sender, e);
            }

            dataGrid.Items.Refresh();

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
				
				if (AvailableAMIDevices[keyValue.Key].Status == State.Off) // ukoliko spamujemo tur on, a vec je ukljucen, da ne pravi vise taskova
				{
					AvailableAMIDevices[keyValue.Key].Status = State.On;
					Task t = Task.Factory.StartNew(() => AvailableAMIDevices[keyValue.Key].worker_DoWork(AvailableAMIDevices[keyValue.Key]));

				};

                dataGrid.Items.Refresh();
            }
        }

        

        private void turnOffBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
				KeyValuePair<string, AMICharacteristics> keyValue = (KeyValuePair<string, AMICharacteristics>)dataGrid.SelectedItem;

				if (AvailableAMIDevices[keyValue.Key].Status == State.On) // spam za turn off
				{
					AvailableAMIDevices[keyValue.Key].Status = State.Off;
				};

                dataGrid.Items.Refresh();

            }
            
        }

    }
}

