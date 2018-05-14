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
        private static ServiceHost host;
		public MainWindow()
		{
			InitializeComponent();

			AMIAgregator novi2 = new AMIAgregator();
			AMIAgregator.agregators.Add(novi2.Agregator_code, novi2);
			AMIAgregator novi3 = new AMIAgregator();
			AMIAgregator.agregators.Add(novi3.Agregator_code, novi3);
			AMIAgregator novi4 = new AMIAgregator();

			Connect();
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
    }
}
