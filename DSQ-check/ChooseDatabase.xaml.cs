using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DSQ_check
{
    /// <summary>
    /// Interaction logic for ChooseDatabase.xaml
    /// </summary>
    public partial class ChooseDatabase : Window
    {
        public ChooseDatabase()
        {
            InitializeComponent();
        }

        private void button_etiming_Click(object sender, RoutedEventArgs e)
        {
            ChooseEtimingDatabase choose = new ChooseEtimingDatabase();
            choose.Show();

            this.Close();
        }

        private void button_eventsys_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
