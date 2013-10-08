using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore
{
    public class DatabaseInfo
    {
        private DatabaseType _dbType;

        public DatabaseInfo(DatabaseType dbType)
        {
        }

        public DatabaseType DBType
        {
            get
            {
                return _dbType;
            }
        }
    }

    public class ETimingDatabase : DatabaseInfo
    {
        private System.IO.FileInfo _dbFile;

        public ETimingDatabase(string filepath)
            : base(DatabaseType.eTiming)
        {
            _dbFile = new System.IO.FileInfo(filepath);
        }

        public System.IO.FileInfo DatabaseFile
        {
            get
            {
                return _dbFile;
            }
        }
    }

    public class EventSysDatabase : DatabaseInfo
    {
        public EventSysDatabase()
            : base(DatabaseType.EventSys)
        {
        }
    }
}
