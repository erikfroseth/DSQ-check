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
using System.Globalization;

namespace DSQ_check
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimingUnits.TimingUnit _timingUnit = null;
        private DSQCheckCore _dsqCheckCore;
        public MainWindow(DataStore.DatabaseInfo dbInfo)
        {
            InitializeComponent();

            statusbar.DataContext = _dsqCheckCore;
            _dsqCheckCore = new DSQCheckCore(dbInfo);
            _dsqCheckCore.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_dsqCheckCore_PropertyChanged);
        }

        private void _dsqCheckCore_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           
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
        }

        private void _timingUnit_TimingDataReadEvent(TimingUnits.TimingPackage package)
        {
            if (!System.Threading.Thread.CurrentThread.Equals(this.Dispatcher.Thread))
            {
                this.Dispatcher.Invoke(new TimingUnits.TimingUnit.TimingDataReadDelegatte(_timingUnit_TimingDataReadEvent), package);
                return;
            }
            else
            {
                Tuple<DataStore.Classes.Runner, DataStore.Classes.Course> runnerAndCourse = _dsqCheckCore.GetRunnerAndCourse(package.CardNumber, package.IdentifierType);

                listbox_runner_controls.ItemsSource = package.ControlPunches.Select(r => r.ControlCode);

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
                        RunnerStatus controlCheck = RunnerStatus.OK;
                        RunnerStatus starttimeCheck = RunnerStatus.OK;
                        RunnerStatus activateionCheck = RunnerStatus.OK;

                        if (_dsqCheckCore.CheckControls)
                        {
                            controlCheck = DSQCheckCore.PerformControlsDSQCheck(package.ControlPunches.Select(r => r.ControlCode), runnerAndCourse.Item2.Controls);
                        }

                        if (_dsqCheckCore.CheckEcardActivation)
                        {
                            activateionCheck = DSQCheckCore.PerformActivationCheck(package.ControlPunches.Select(r => r.ControlCode));
                        }

                        if (_dsqCheckCore.CheckStarttime)
                        {
                        }

                        textblock_course.Text = "Løype: " + runnerAndCourse.Item2.CourseName;
                        listbox_course_controls.ItemsSource = runnerAndCourse.Item2.Controls;

                        RunnerStatus overallRunnerstatus;
                        if (controlCheck == RunnerStatus.DSQ || starttimeCheck == RunnerStatus.DSQ || activateionCheck == RunnerStatus.DSQ)
                        {
                            overallRunnerstatus = RunnerStatus.DSQ;
                        }
                        else
                        {
                            overallRunnerstatus = RunnerStatus.OK;
                        }

                        switch (overallRunnerstatus)
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

        private void menuitem_ecu1(object sender, RoutedEventArgs e)
        {
            if (_timingUnit != null && _timingUnit.IsStarted)
            {
                MessageBox.Show("Du må stoppe kommunikasjonen før du kan bytte tidtakingsenhet");
                return;
            }
            Dialogs.ChooseCOMPort comPort = new Dialogs.ChooseCOMPort();
            comPort.ShowDialog();

            if (!string.IsNullOrEmpty(comPort.COMPort))
            {
                _timingUnit = new TimingUnits.ECU1(comPort.COMPort);
                _timingUnit.TimingDataReadEvent += new TimingUnits.TimingUnit.TimingDataReadDelegatte(_timingUnit_TimingDataReadEvent);

            }
        }

        private void menuitem_mtr4(object sender, RoutedEventArgs e)
        {
            if (_timingUnit != null && _timingUnit.IsStarted)
            {
                MessageBox.Show("Du må stoppe kommunikasjonen før du kan bytte tidtakingsenhet");
                return;
            }
            Dialogs.ChooseCOMPort comPort = new Dialogs.ChooseCOMPort();
            comPort.ShowDialog();

            if (!string.IsNullOrEmpty(comPort.COMPort))
            {
                _timingUnit = new TimingUnits.MTR4(comPort.COMPort);
                _timingUnit.TimingDataReadEvent += new TimingUnits.TimingUnit.TimingDataReadDelegatte(_timingUnit_TimingDataReadEvent);
            }
        }

        private void menuitem_startstop_communication_Click(object sender, RoutedEventArgs e)
        {
            if (_timingUnit == null)
            {
                MessageBox.Show("Du må velge en avlesningsenhet før du kan fortsette", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {

                if (_timingUnit.IsStarted)
                {
                    _timingUnit.StopCommunication();

//                    menuitem_startstop_communication.IsEnabled = !_timingUnit.IsStarted;
                    menuitem_startstop_communication.Header = "Start kommunikasjon";
                }
                else
                {
                    try
                    {
                        _timingUnit.StartCommunication();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Klarte ikke å starte kommunikasjonen med tidtakingsenheten. Følgende feilmelding oppstod: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    finally
                    {
  //                      menuitem_startstop_communication.IsEnabled = !_timingUnit.IsStarted;

                        if (_timingUnit.IsStarted)
                        {
                            menuitem_startstop_communication.Header = "Stopp kommunikasjon";
                        }
                        else
                        {
                            menuitem_startstop_communication.Header = "Start kommunikasjon";
                        }
                    }
                }
            }
        }
    }

    public class LastUpdateConverter : IValueConverter
    {
        private const string DATE_FORMAT = "HH:mm:ss";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime lastUpdate = (DateTime)value;
            string returnValue = "Siste oppdatering fra database: ";

            if (lastUpdate.Equals(DateTime.MinValue))
            {
                returnValue += "Aldri";
            }
            else
            {
                returnValue += lastUpdate.ToString(DATE_FORMAT);
            }

            return returnValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NumRunnersAndCoursesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int && values[1] is int)
            {
                int numRunners = (int)values[0];
                int numCourses = (int)values[1];

                return string.Format("Antall løpere/løyper: {0}/{1}", numRunners, numCourses);
            }
            else
            {
                return "Antall løpere/løyper: Ukjent";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
