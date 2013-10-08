using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check
{
    public class DSQCheckCore
    {
        private DatabaseType _dbType;
        private ConnectionHealth _connHealth;

        private DateTime _lastDatabaseRefresh = DateTime.MinValue;

        private Dictionary<int, DataStore.Classes.Course> _courses;
        private List<DataStore.Classes.Runner> _runners;

        private System.Timers.Timer _refreshTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);

        public DSQCheckCore(DatabaseType dbType)
        {
            _dbType = dbType;

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
                return;
            }
        }

        public void RefreshDataStore()
        {
            DataStore.DatabaseInterface database = null;

            try
            {
                switch (_dbType)
                {
                    case DatabaseType.eTiming:
                        database = new DataStore.eTimingInterface(null);
                        break;
                    case DatabaseType.EventSys:
                    // break;
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

                        _lastDatabaseRefresh = DateTime.Now;
                    }
                }

                _connHealth = ConnectionHealth.OK;
            }
            finally
            {
                if (database != null)
                {
                    database.Dispose();
                }
            }
        }
        
        public Tuple<DataStore.Classes.Runner, DataStore.Classes.Course> GetRunnerAndCourse(uint identifierValue)
        {
            lock (this._runners)
            {
                IEnumerable<DataStore.Classes.Runner> foundRunners = this._runners.Where(r => r.Ecard.HasValue && r.Ecard.Value == identifierValue);

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

        public DatabaseType DBType
        {
            get
            {
                return _dbType;
            }
        }
    }

    public enum DatabaseType
    {
        eTiming, EventSys
    }
    public enum ConnectionHealth
    {
        OK, NotWorking
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
