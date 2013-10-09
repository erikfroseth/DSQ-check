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
    /// Interaction logic for ChooseEventSysEvent.xaml
    /// </summary>
    public partial class ChooseEventSysEvent : Window
    {
        private MySqlConnectionStringBuilder _connString;
        private bool exit = true;

        public ChooseEventSysEvent(MySqlConnectionStringBuilder connString)
        {
            _connString = connString;

            InitializeComponent();
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_open_Click(object sender, RoutedEventArgs e)
        {
            if(combobox_event.SelectedValue == null)
            {
                MessageBox.Show("Du må velge en event før du kan fortsette.", "Velg en event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else 
            {
                DataStore.EventSysDatabase dbinfo = new DataStore.EventSysDatabase(_connString, (ushort)combobox_event.SelectedValue);

                MainWindow mainWindow = new MainWindow(dbinfo, null);
                mainWindow.Show();

                exit = false;
                this.Close();
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connString.ConnectionString))
                {
                    conn.Open();

                    using (MySqlCommand command = new MySqlCommand("SHOW DATABASES;", conn))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string dbName = reader.GetString(0);

                                if (dbName.StartsWith("eventsys_"))
                                {
                                    combobox_database.Items.Add(dbName.Remove(0, 9));
                                }
                            }
                        }
                    }
                }

                if (combobox_database.Items.Count > 0)
                {
                    combobox_database.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
                combobox_event.Items.Clear();
            }
            finally
            {
                combobox_event.IsEnabled = (combobox_database.SelectedValue != null);
            }
        }

        private void combobox_database_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            combobox_event.Items.Clear();

            try
            {
                _connString.Database = "eventsys_" + combobox_database.SelectedValue.ToString();

                using (MySqlConnection conn = new MySqlConnection(_connString.ConnectionString))
                {
                    conn.Open();

                    using (MySqlCommand command = new MySqlCommand("SELECT event_id, event_descr FROM events_tbl WHERE event_id <> 1;", conn))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string event_descr = reader.GetString("event_descr");
                                ushort event_id = reader.GetUInt16("event_id");

                                combobox_event.Items.Add(new KeyValuePair<ushort, string>(event_id, event_descr));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                combobox_event.Items.Clear();
            }
            finally
            {
                if (combobox_event.Items.Count > 0)
                {
                    combobox_event.SelectedIndex = 0;
                }
            }
        }

        private void combobox_event_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            button_open.IsEnabled = (combobox_event.SelectedValue != null);
        }
    }
}
