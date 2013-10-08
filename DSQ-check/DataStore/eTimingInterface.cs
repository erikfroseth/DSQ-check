using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace DSQ_check.DataStore
{
    public class eTimingInterface : DatabaseInterface 
    {
        private OleDbConnection _oleConnection;
        private System.IO.FileInfo _databaseFile;

        public eTimingInterface(System.IO.FileInfo databaseFile)
        {
            _databaseFile = databaseFile;

            _oleConnection = new OleDbConnection(GetConnectionString());
            _oleConnection.Open();
        }

        private string GetConnectionString()
        {
            return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source='{0}';Jet OLEDB:Engine Type=4;Mode=Read", _databaseFile.FullName);
        }

        public List<Classes.Runner> GetRunners()
        {
            List<Classes.Runner> runners = new List<Classes.Runner>();

            using (OleDbCommand command = new OleDbCommand())
            {
                command.Connection = _oleConnection;
                command.CommandText = "SELECT Name.name AS first_name, Name.ename AS last_name, Name.cource AS courseId, team.name AS clubName " +
                                        ", Name.startno AS startno, Name.ecard AS ecard, Name.ecard2 AS ecard2 FROM Name, team WHERE Name.team = team.code;";

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string first_name = reader.GetValue(0).ToString().Trim();
                        string last_name = reader.GetValue(1).ToString().Trim();
                        string clubName = reader.GetValue(3).ToString().Trim();
                        int? courseId = null;
                        uint? startNumber = null, ecard1 = null, ecard2 = null;

                        if (!reader.IsDBNull(2))
                        {
                            int testValue;
                            if (int.TryParse(reader.GetValue(2).ToString(), out testValue))
                            {
                                courseId = testValue;
                            }
                        }

                        if (!reader.IsDBNull(4))
                        {
                            uint testValue;
                            if (uint.TryParse(reader.GetValue(4).ToString(), out testValue))
                            {
                                startNumber = testValue;
                            }
                        }

                        if (!reader.IsDBNull(5))
                        {
                            uint testValue;
                            if (uint.TryParse(reader.GetValue(5).ToString(), out testValue))
                            {
                                ecard1 = testValue;
                            }
                        }

                        if (!reader.IsDBNull(6))
                        {
                            uint testValue;
                            if (uint.TryParse(reader.GetValue(6).ToString(), out testValue))
                            {
                                ecard2 = testValue;
                            }
                        }

                        Classes.Runner newRunner = new Classes.Runner(first_name, last_name, courseId, clubName, ecard1, ecard2, null, startNumber);
                        runners.Add(newRunner);
                    }
                }
            }
            return runners;
        }
        public Dictionary<int, Classes.Course> GetCourses()
        {
            Dictionary<int, Classes.Course> courses = new Dictionary<int, Classes.Course>();

            using (OleDbCommand command = new OleDbCommand())
            {
                command.Connection = _oleConnection;

                #region GET_COURSES
                command.CommandText = "SELECT code AS courseId, name AS courseName FROM cource;";

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int courseId;
                        string courseName;

                        if (!int.TryParse(reader.GetValue(0).ToString(), out courseId))
                        {
                            continue;
                        }

                        courseName = reader.GetString(1);

                        Classes.Course newCourse = new Classes.Course(courseId, courseName);
                        courses.Add(newCourse.CourseId, newCourse);
                    }
                }
                #endregion

                #region GET_CONTROLS
                command.CommandText = "SELECT courceno AS courseId, code AS code FROM controls ORDER BY courceno ASC, controlno ASC;";

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int courseId;

                        if (!int.TryParse(reader.GetValue(0).ToString(), out courseId))
                        {
                            continue;
                        }
                        else if (!courses.ContainsKey(courseId))
                        {
                            continue;
                        }
                        else
                        {
                            byte controlCode;
                            if (byte.TryParse(reader.GetValue(1).ToString(), out controlCode))
                            {
                                courses[courseId].AddControl(controlCode);
                            }
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
                    _oleConnection.Dispose();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
