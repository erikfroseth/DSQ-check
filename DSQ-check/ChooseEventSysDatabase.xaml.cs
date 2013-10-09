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
using MySql.Data.MySqlClient;

namespace DSQ_check
{
    /// <summary>
    /// Interaction logic for ChooseEventSysDatabase.xaml
    /// </summary>
    public partial class ChooseEventSysDatabase : Window
    {
        private bool exit = true;

        public ChooseEventSysDatabase()
        {
            InitializeComponent();
        }

        private void button_open_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder();
            connString.Server = textbox_server.Text;
            connString.UserID = textbox_username.Text;
            connString.Password = textbox_password.Password;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString.ConnectionString))
                {
                    conn.Open();

                    using (MySqlCommand command = new MySqlCommand("SELECT VERSION();", conn))
                    {
                        command.ExecuteScalar();
                    }
                }

                ChooseEventSysEvent chooseEvent = new ChooseEventSysEvent(connString);
                chooseEvent.Show();

                this.exit = false;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klarte ikke å koble til databasen. Følgende feilmelding oppstod: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.exit)
            {
                MessageBoxResult result;
                result = MessageBox.Show("Vil du avslutte diskkontroll?", "Avslutte", MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.Yes:
                        Application.Current.Shutdown();
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
