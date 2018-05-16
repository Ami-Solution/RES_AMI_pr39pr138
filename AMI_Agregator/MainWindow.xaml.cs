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

		private static ServiceHost Host;
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
            Host = new ServiceHost(typeof(AMIAgregator));
            Host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            Host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
            Host.AddServiceEndpoint(typeof(IAMI_Agregator), binding, address);

            Start();
        }

        public static void Start()
        {
            try
            {
                Host.Open();
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
                Host.Close();
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
				agregators[keyValue.Key].State = Storage.State.On;

			}

			dataGrid.Items.Refresh();
		}

		private void turnOffBtn_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.SelectedItem != null)
			{
				KeyValuePair<string, AMIAgregator> keyValue = (KeyValuePair<string, AMIAgregator>)dataGrid.SelectedItem;
				agregators[keyValue.Key].State = Storage.State.Off;

			}

			dataGrid.Items.Refresh();
		}
	}
}
