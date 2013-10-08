using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore
{
    interface DatabaseInterface
    {
        string ConnectionString
        {
            get;
            set;
        }

        List<Classes.Runner> GetRunners();
        Dictionary<string, Classes.Course> GetCourses();
    }
}
