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
        public MainWindow()
        {
            InitializeComponent();

           

            try
            {
                System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                w.Start();

                using (DataStore.eTimingInterface data = new DataStore.eTimingInterface(new System.IO.FileInfo(@"C:\Users\Erik\Google Drive\tidtaking\Ferdig løp\kretsstafett_hl06\etime.mdb")))
                {
                    List<DataStore.Classes.Runner> runners = data.GetRunners();
                    Dictionary<int, DataStore.Classes.Course> courses = data.GetCourses();
                }

                w.Stop();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
