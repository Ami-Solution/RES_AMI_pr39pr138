using AMI_Agregator;
using AMI_System_Management;
using System;
using System.Collections.Generic;
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

namespace User
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void devBtn_Click(object sender, RoutedEventArgs e)
        {
            AMI_Device.MainWindow window = new AMI_Device.MainWindow();
            window.Show();
        }

        private void agrBtn_Click(object sender, RoutedEventArgs e)
        {
            AMI_Agregator.MainWindow window = new AMI_Agregator.MainWindow();
            window.Show();
        }

        private void SystBtn_Click(object sender, RoutedEventArgs e)
        {
            AMI_System_Management.MainWindow window = new AMI_System_Management.MainWindow();
            window.Show();
        }
    }
}
