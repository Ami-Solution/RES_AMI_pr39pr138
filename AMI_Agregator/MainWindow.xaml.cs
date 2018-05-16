using Common;
using System;
using System.Collections.Generic;
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
		//predefinisan dictionari koji sadrzi: agregator_id i sam agregator.
		//sam agregator sadrzi svoj id, i buffer
		//buffer je dictionari i cine ga: device_id i novi dictionari: typeMeasurment i lista vrednost
		public static Dictionary<string, AMIAgregator> agregators { get; set; }

		public static int agregatorNumber = 1;

		private static ServiceHost host;
		public MainWindow()
		{
			InitializeComponent();

			AMIAgregator a = new AMIAgregator(); // to initalise predefined agregators

			Connect();

			this.DataContext = this;
		}

        private static void Connect()
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

			dataGrid.Items.Refresh();
		}

		private void RmvBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMIAgregator> keyValue = (KeyValuePair<string, AMIAgregator>)dataGrid.SelectedItem;
				agregators.Remove(keyValue.Key);

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
					Task t = Task.Factory.StartNew(() => agregators[keyValue.Key].SendToLocalStorage(agregators[keyValue.Key]));
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
