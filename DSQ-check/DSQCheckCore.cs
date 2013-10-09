using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DSQ_check
{
    public class DSQCheckCore : INotifyPropertyChanged
    {
        private DataStore.DatabaseInfo _databaseInfo;
        private ConnectionHealth _connHealth = ConnectionHealth.Unknown;

        private DateTime _lastDatabaseRefresh = DateTime.MinValue;

        private Dictionary<int, DataStore.Classes.Course> _courses = new Dictionary<int,DataStore.Classes.Course>();
        private List<DataStore.Classes.Runner> _runners = new List<DataStore.Classes.Runner>();

        private System.Timers.Timer _refreshTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);

        public DSQCheckCore(DataStore.DatabaseInfo dbInfo)
        {
            _databaseInfo = dbInfo;

            RefreshDataStore();

            _refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(_refreshTimer_Elapsed);
            _refreshTimer.Start();
        }

        private void _refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                RefreshDataStore();
            }
            catch (Exception)
            {
                // Should we do something here?
                _connHealth = ConnectionHealth.NotWorking;
                NotifyPropertyChanged("ConnectionHealth");
                return;
            }
        }

        public void RefreshDataStore()
        {
            DataStore.DatabaseInterface database = null;

            try
            {
                switch (_databaseInfo.DBType)
                {
                    case DatabaseType.eTiming:
                        DataStore.ETimingDatabase etimingDatabase = _databaseInfo as DataStore.ETimingDatabase;
                        database = new DataStore.eTimingInterface(etimingDatabase);
                        break;
                    case DatabaseType.EventSys:
                        DataStore.EventSysDatabase eventsysDatabase = _databaseInfo as DataStore.EventSysDatabase;
                        database = new DataStore.EventSysInterface(eventsysDatabase);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Dictionary<int, DataStore.Classes.Course> courses = database.GetCourses();
                List<DataStore.Classes.Runner> runners = database.GetRunners();

                lock (_runners)
                {
                    lock (_courses)
                    {
                        _courses = courses;
                        _runners = runners;
                        NotifyPropertyChanged("NumCourses");
                        NotifyPropertyChanged("NumRunners");


                        _lastDatabaseRefresh = DateTime.Now;
                        NotifyPropertyChanged("LastDataRefresh");
                    }
                }

                _connHealth = ConnectionHealth.OK;
                NotifyPropertyChanged("ConnectionHealth");
            }
            finally
            {
                if (database != null)
                {
                    database.Dispose();
                }
            }
        }
        
        public Tuple<DataStore.Classes.Runner, DataStore.Classes.Course> GetRunnerAndCourse(uint identifierValue, RunnerIdentifier identifierType)
        {
            lock (this._runners)
            {
                IEnumerable<DataStore.Classes.Runner> foundRunners;
                switch (identifierType)
                {
                    case RunnerIdentifier.Ecard:
                        foundRunners = this._runners.Where(r => r.Ecard.HasValue && r.Ecard.Value == identifierValue);
                        break;
                    case RunnerIdentifier.EmiTag:
                        foundRunners = this._runners.Where(r => (r.EmiTag1.HasValue && r.EmiTag1.Value == identifierValue)
                                || (r.EmiTag2.HasValue && r.EmiTag2.Value == identifierValue)
                            );
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (foundRunners.Any())
                {
                    lock (this._courses)
                    {
                        // Just select the first
                        // TODO: Filter out runners based on starttime and such
                        DataStore.Classes.Runner runner = foundRunners.First();

                        if (runner.CourseId.HasValue && _courses.ContainsKey(runner.CourseId.Value))
                        {
                            return new Tuple<DataStore.Classes.Runner, DataStore.Classes.Course>(runner, _courses[runner.CourseId.Value]);
                        }
                        else
                        {
                            return new Tuple<DataStore.Classes.Runner, DataStore.Classes.Course>(runner, null);
                        }
                    }
                }
                else
                {
                    return new Tuple<DataStore.Classes.Runner, DataStore.Classes.Course>(null, null);
                }
            }
        }
        public static RunnerStatus PerformDSQCheck(IEnumerable<byte> runnerControls, IEnumerable<byte> courseControls)
        {
            int counter = 0;
            int numCorrect = 0;

            foreach (byte control in courseControls)
            {
                for (; counter < runnerControls.Count(); counter++)
                {
                    // Go through runner controls to find if anyone matches.
                    if (control == runnerControls.ElementAt(counter))
                    {
                        // Correct is found, so increase numCorrect-variable and go to next cource control.
                        numCorrect++;
                        counter++;
                        break;
                    }
                }

                // Stop if all runner controls are checked
                if (counter + 1 > courseControls.Count())
                {
                    break;
                }
            }

            if (numCorrect == courseControls.Count())
            {
                return RunnerStatus.OK;
            }
            else
            {
                return RunnerStatus.DSQ;
            }
        }

        public DataStore.DatabaseInfo DatabaseInfo
        {
            get
            {
                return _databaseInfo;
            }
        }
        public ConnectionHealth ConnectionHealth
        {
            get
            {
                return _connHealth;
            }
        }
        public int NumRunners
        {
            get
            {
                return _runners.Count;
            }
        }
        public int NumCourses
        {
            get
            {
                return _courses.Count;
            }
        }
        public DateTime LastDataRefresh
        {
            get
            {
                return _lastDatabaseRefresh;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public enum DatabaseType
    {
        eTiming, EventSys
    }
    public enum ConnectionHealth
    {
        OK, NotWorking, Unknown
    }
    public enum RunnerStatus
    {
        OK, DSQ
    }
    public enum RunnerIdentifier
    {
        Ecard, EmiTag
    }
}
