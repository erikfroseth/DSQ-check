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

namespace DSQ_check.Dialogs
{
    /// <summary>
    /// Interaction logic for ChooseCOMPort.xaml
    /// </summary>
    public partial class ChooseCOMPort : Window
    {
        private string _result;

        public ChooseCOMPort()
        {
            InitializeComponent();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            _result = null;
            this.Close();
        }

        private void button_choose_Click(object sender, RoutedEventArgs e)
        {
            if (combobox_port.Text == null)
            {
                MessageBox.Show("Vennligst velg en COM-port før du fortsetter.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                _result = combobox_port.Text;
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string port in System.IO.Ports.SerialPort.GetPortNames())
            {
                combobox_port.Items.Add(port);
            }

            if (combobox_port.Items.Count > 0)
            {
                combobox_port.SelectedIndex = 0;
            }
        }

        public string COMPort
        {
            get
            {
                return _result;
            }
        }
    }
}
