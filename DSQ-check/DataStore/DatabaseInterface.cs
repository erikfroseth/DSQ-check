﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore
{
    interface DatabaseInterface :IDisposable
    {
        List<Classes.Runner> GetRunners();
        Dictionary<int, Classes.Course> GetCourses();
    }
}
