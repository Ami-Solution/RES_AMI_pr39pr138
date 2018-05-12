using Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
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

            AvailableAMIDevices.Add(ami.Name, ami);
        }

        private void RmvBtn_Click(object sender, RoutedEventArgs e)
        {
            object key = dataGrid.SelectedItem; //nisam proverio, jer nisam jos bindovao
            AvailableAMIDevices.Remove((string)key);
        }
    }
}
