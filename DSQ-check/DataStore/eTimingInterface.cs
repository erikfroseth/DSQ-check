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
        private ETimingDatabase _dbInfo;

        public eTimingInterface(ETimingDatabase dbInfo)
        {
            _dbInfo = dbInfo;

            _oleConnection = new OleDbConnection(GetConnectionString());
            _oleConnection.Open();
        }

        private string GetConnectionString()
        {
            return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source='{0}';Jet OLEDB:Engine Type=4;Mode=Read", _dbInfo.DatabaseFile.FullName);
        }

        public List<Classes.Runner> GetRunners()
        {
            DateTime arrDate;
            List<Classes.Runner> runners = new List<Classes.Runner>();

            using (OleDbCommand command = new OleDbCommand())
            {
                command.Connection = _oleConnection;

                // Get arr date
                command.CommandText = "SELECT firststart FROM arr;";
                object firststartValue = command.ExecuteScalar();

                if (firststartValue == null)
                {
                    arrDate = DateTime.Now.Date;
                }
                else
                {
                    if (DateTime.TryParse(firststartValue.ToString(), out arrDate))
                    {
                        arrDate = arrDate.Date;
                    }
                    else
                    {
                        arrDate = DateTime.Now.Date;
                    }
                }

                command.CommandText = "SELECT Name.name AS first_name, Name.ename AS last_name, Name.cource AS courseId, team.name AS clubName " +
                                        ", Name.startno AS startno, Name.ecard AS ecard, Name.ecard2 AS ecard2, Name.starttime AS starttime " +
                                        ", class.class AS className FROM Name, team, class WHERE Name.team = team.code AND Name.class = class.code;";

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string first_name = reader.GetValue(0).ToString().Trim();
                        string last_name = reader.GetValue(1).ToString().Trim();
                        string clubName = reader.GetValue(3).ToString().Trim();
                        string className = reader.GetValue(8).ToString().Trim();

                        int? courseId = null;
                        uint? startNumber = null, ecard = null, emitag1 = null, emitag2 = null;
                        DateTime? starttime = null;

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
                                switch (_dbInfo.Ecard1As)
                                {
                                    case RunnerIdentifier.Ecard:
                                        ecard = testValue;
                                        break;
                                    case RunnerIdentifier.EmiTag:
                                        emitag1 = testValue;
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }

                        if (!reader.IsDBNull(6))
                        {
                            uint testValue;
                            if (uint.TryParse(reader.GetValue(6).ToString(), out testValue))
                            {
                                switch (_dbInfo.Ecard1As)
                                {
                                    case RunnerIdentifier.Ecard:
                                        ecard = testValue;
                                        break;
                                    case RunnerIdentifier.EmiTag:
                                        emitag2 = testValue;
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }

                        string test = reader.GetDataTypeName(7);

                        if (!reader.IsDBNull(7))
                        {
                            double value = reader.GetDouble(7);

                            if (value <= 1)
                            {
                                starttime = arrDate.Date.Add(DateTime.FromOADate(value).TimeOfDay);
                            }
                            else
                            {
                                starttime = DateTime.FromOADate(value);
                            }
                        }
                        else
                        {
                        }

                        Classes.Runner newRunner = new Classes.Runner(first_name, last_name, courseId, clubName, className, ecard, emitag1, emitag2, startNumber, starttime);
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

