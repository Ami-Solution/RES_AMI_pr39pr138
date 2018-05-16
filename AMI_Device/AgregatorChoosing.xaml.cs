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
using System.Windows.Shapes;

namespace AMI_Device
{
    /// <summary>
    /// Interaction logic for AgregatorChoosing.xaml
    /// </summary>
    public partial class AgregatorChoosing : Window
    {
        static List<string> existingAgregators=new List<string>();
        public static string agregatorName;
        public AgregatorChoosing()
        {
            InitializeComponent();
        }

        private void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            //ovde smestim agregate koji treba da se izlistaju
            existingAgregators = MainWindow.defaultProxy.ListOfAgregatorIDs();
            IDcmbBox.ItemsSource = existingAgregators;
            IDcmbBox.SelectedIndex = 0;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //treba da se storuje izabrani agregat iz liste
            int selected = IDcmbBox.SelectedIndex;
			if (selected != -1)
			{
				agregatorName = existingAgregators[selected];
			}
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }
    }
}
