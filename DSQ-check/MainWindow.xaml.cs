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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSQ_check
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimingUnits.TimingUnit _timingUnit;
        private DSQCheckCore _dsqCheckCore;
        public MainWindow(DataStore.DatabaseInfo dbInfo, TimingUnits.TimingUnit timingUnit)
        {
            _dsqCheckCore = new DSQCheckCore(dbInfo);
            _timingUnit = timingUnit;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset all texts
            textblock_class.Text = "Klasse:";
            textblock_club.Text = string.Empty;
            textblock_course.Text = "Løype:";
            textblock_name.Text = "Ingen løper avlest";
            textblock_status.Text = string.Empty;

            grid_dsqControl.Background = Brushes.Transparent;

            _timingUnit.TimingDataReadEvent += new TimingUnits.TimingUnit.TimingDataReadDelegatte(_timingUnit_TimingDataReadEvent);
        }

        private void _timingUnit_TimingDataReadEvent(TimingUnits.TimingPackage package)
        {
            Tuple<DataStore.Classes.Runner, DataStore.Classes.Course> runnerAndCourse = _dsqCheckCore.GetRunnerAndCourse(package.CardNumber, package.IdentifierType);

            listbox_runner_controls.ItemsSource = package.Controls;

            if (runnerAndCourse.Item1 == null)
            {
                // NO RUNNER FOUND!
                textblock_class.Text = "Klasse:";
                textblock_club.Text = string.Empty;
                textblock_course.Text = "Løype:";
                textblock_name.Text = "Ukjent løper";
                textblock_status.Text = "Ukjent løper";

                listbox_course_controls.ItemsSource = null;

                grid_dsqControl.Background = Brushes.Orange;
            }
            else
            {
                textblock_name.Text = string.Format("{0}, {1}", runnerAndCourse.Item1.LastName, runnerAndCourse.Item1.FirstName);
                textblock_class.Text = "Klasse:";
                textblock_club.Text = runnerAndCourse.Item1.ClubName;

                if (runnerAndCourse.Item2 == null)
                {
                    // NO COURSE FOUND
                    textblock_course.Text = "Løype: UKJENT";
                    textblock_status.Text = "Ukjent løype";

                    listbox_course_controls.ItemsSource = null;

                    grid_dsqControl.Background = Brushes.Orange;
                }
                else
                {
                    // Both runner and course found. Perform DSQ-check
                    RunnerStatus runnerStatus = DSQCheckCore.PerformDSQCheck(package.Controls, runnerAndCourse.Item2.Controls);

                    textblock_course.Text = "Løype: " + runnerAndCourse.Item2.CourseName;
                    listbox_course_controls.ItemsSource = runnerAndCourse.Item2.Controls;

                    switch (runnerStatus)
                    {
                        case RunnerStatus.DSQ:
                            grid_dsqControl.Background = Brushes.Pink;
                            textblock_status.Text = "Diskvalifisert";
                            break;
                        case RunnerStatus.OK:
                            grid_dsqControl.Background = Brushes.LightGreen;
                            textblock_status.Text = "Godkjent";
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        private void menuitem_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
