using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace DSQ_check.DataStore
{
    public class EventSysInterface: DatabaseInterface
    {
        private MySqlConnection _connection;
        private EventSysDatabase _connInfo;

        public EventSysInterface(EventSysDatabase connInfo)
        {
            _connInfo = connInfo;

            _connection = new MySqlConnection(connInfo.ConnectionString);
            _connection.Open();
        }

        public List<Classes.Runner> GetRunners()
        {
            List<Classes.Runner> runners = new List<Classes.Runner>();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = _connection;
                command.CommandText = "SELECT runners_general.first_name, runners_general.last_name, runners_specific.cource_id " +
                                    ", runners_specific.startno, runners_specific.ecard, runners_specific.emitag1, runners_specific.emitag2 " +
                                    ", clubs.name AS clubName, runners_specific.starttime AS starttime, classes.name AS className " +
                                    " FROM runners_specific " +
                                    "JOIN runners_general USING (runner_id) " +
                                    "JOIN clubs USING (club_id) " +
                                    "JOIN classes USING (class_id) " +
                                    "WHERE event_id = ?event_id;";
                command.Parameters.AddWithValue("?event_id", _connInfo.EventId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string first_name = reader.GetString("first_name");
                        string last_name = reader.GetString("last_name");
                        string clubName = reader.GetString("clubName");
                        string className = reader.GetString("className");

                        ushort courseId = reader.GetUInt16("cource_id");
                        uint? startNumber = null, ecard = null, emitag1 = null, emitag2 = null;
                        DateTime? starttime = null;

                        if (!reader.IsDBNull(3))
                        {
                            startNumber = reader.GetUInt32("startno");
                        }

                        if (!reader.IsDBNull(4))
                        {
                            ecard = reader.GetUInt32("ecard");
                        }

                        if (!reader.IsDBNull(5))
                        {
                            emitag1 = reader.GetUInt32("emitag1");
                        }

                        if (!reader.IsDBNull(6))
                        {
                            emitag2 = reader.GetUInt32("emitag2");
                        }

                        if (!reader.IsDBNull(7))
                        {
                            starttime = reader.GetDateTime("starttime");
                        }

                        DataStore.Classes.Runner newRunner;
                        newRunner = new Classes.Runner(first_name, last_name, courseId, clubName, className, ecard, emitag1, emitag2, startNumber, starttime);

                        runners.Add(newRunner);
                    }
                }
            }

            return runners;
        }

        public Dictionary<int, Classes.Course> GetCourses()
        {
            Dictionary<int, Classes.Course> courses = new Dictionary<int, Classes.Course>();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = _connection;

                #region GET_COURSES
                command.CommandText = "SELECT cource_id, name FROM cources WHERE event_id = ?event_id OR event_id = 1;";
                command.Parameters.AddWithValue("?event_id", _connInfo.EventId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ushort courseId = reader.GetUInt16("cource_id");
                        string courseName = reader.GetString("name");

                        Classes.Course newCourse = new Classes.Course(courseId, courseName);
                        courses.Add(newCourse.CourseId, newCourse);
                    }
                }
                #endregion

                #region GET_CONTROLS
                command.CommandText = "SELECT cource_id, control_code FROM controls WHERE event_id = ?event_id OR event_id = 1 ORDER BY cource_id, control_no;";

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ushort courseId = reader.GetUInt16("cource_id");

                        if (!courses.ContainsKey(courseId))
                        {
                            continue;
                        }
                        else
                        {
                            byte controlCode = reader.GetByte("control_code");
                            courses[courseId].AddControl(controlCode);
                        }
                    }
                }
                #endregion
            }

            return courses;
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _connection.Dispose();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

    }
}
