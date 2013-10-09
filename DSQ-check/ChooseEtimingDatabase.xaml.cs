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
    /// Interaction logic for ChooseEtimingDatabase.xaml
    /// </summary>
    public partial class ChooseEtimingDatabase : Window
    {
        private bool exit = true;

        public ChooseEtimingDatabase()
        {
            InitializeComponent();
        }

        private void button_open_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(textbox_filepath.Text))
            {
                MessageBox.Show("Finner ikke databasefilen du spesifiserte. Vennligst korriger dette før du fortsetter.", "Finner ikke databasen.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (combobox_ecard1.SelectedValue == null)
            {
                MessageBox.Show("Du må velge hva du skal sette ecard1 (gult brikkefelt) til før du kan fortsette", "Mangler ecard1", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (combobox_ecard2.SelectedValue == null)
            {
                MessageBox.Show("Du må velge hva du skal sette ecard2 (blått brikkefelt) til før du kan fortsette", "Mangler ecard2", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                RunnerIdentifier ecard1 = (RunnerIdentifier)combobox_ecard1.SelectedValue;
                RunnerIdentifier ecard2 = (RunnerIdentifier)combobox_ecard2.SelectedValue;

                if (ecard1 == RunnerIdentifier.Ecard && ecard2 == RunnerIdentifier.Ecard)
                {
                    MessageBox.Show("Du kan ikke sette begge brikkefeltene til å være brikkenr.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    MainWindow mainWindow = new MainWindow(new DataStore.ETimingDatabase(textbox_filepath.Text, ecard1, ecard2));
                    mainWindow.Show();

                    exit = false;
                    this.Close();
                }
            }
        }

        private void button_choose_Click(object sender, RoutedEventArgs e)
        {
            textbox_filepath.Text = OpenFileDialog(textbox_filepath.Text);
        }

        public static string OpenFileDialog(string originalString)
        {
            // Get a dialogfilter, according to file extension
            string filter = "MDB-fil";
            filter += string.Format(" ({0})|*{0}", ".mdb");
            filter += "|Alle filer (*.*)|*.*";

            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> dialogResult;

            // Initialize filetype filter
            // fileDialog.DefaultExt = defaultExt;
            fileDialog.Filter = filter;

            // Show dialog
            dialogResult = fileDialog.ShowDialog();

            // Print out text if dialog closed normally
            if (dialogResult == true)
            {
                return fileDialog.FileName;
            }
            else
            {
                return originalString;
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (exit)
            {
                MessageBoxResult result;
                result = MessageBox.Show("Vil du avslutte programmet?", "Avslutte", MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Application.Current.Shutdown();
                        return;
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            combobox_ecard1.Items.Add(new KeyValuePair<RunnerIdentifier, string>(RunnerIdentifier.Ecard, "Brikkenr"));
            combobox_ecard1.Items.Add(new KeyValuePair<RunnerIdentifier, string>(RunnerIdentifier.EmiTag, "EmiTag"));

            combobox_ecard2.Items.Add(new KeyValuePair<RunnerIdentifier, string>(RunnerIdentifier.Ecard, "Brikkenr"));
            combobox_ecard2.Items.Add(new KeyValuePair<RunnerIdentifier, string>(RunnerIdentifier.EmiTag, "EmiTag"));

            combobox_ecard1.SelectedIndex = 0;
            combobox_ecard2.SelectedIndex = 1;
        }

    }
}
