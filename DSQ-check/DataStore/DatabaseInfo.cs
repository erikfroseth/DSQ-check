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
            _dbType = dbType;
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
        private RunnerIdentifier _ecard1As, _ecard2As;

        public ETimingDatabase(string filepath, RunnerIdentifier ecard1As, RunnerIdentifier ecard2As)
            : base(DatabaseType.eTiming)
        {
            _ecard1As = ecard1As;
            _ecard2As = ecard2As;
            
            _dbFile = new System.IO.FileInfo(filepath);
        }

        public System.IO.FileInfo DatabaseFile
        {
            get
            {
                return _dbFile;
            }
        }
        public RunnerIdentifier Ecard1As
        {
            get
            {
                return _ecard1As;
            }
        }
        public RunnerIdentifier Ecard2As
        {
            get
            {
                return _ecard2As;
            }
        }
    }

    public class EventSysDatabase : DatabaseInfo
    {
        private MySql.Data.MySqlClient.MySqlConnectionStringBuilder _connString;
        private ushort _eventId;

        public EventSysDatabase(MySql.Data.MySqlClient.MySqlConnectionStringBuilder connstring, ushort eventId)
            : base(DatabaseType.EventSys)
        {
            _connString = connstring;
            _eventId = eventId;
        }

        public string ConnectionString
        {
            get
            {
                return _connString.ConnectionString;
            }
        }

        public ushort EventId
        {
            get
            {
                return _eventId;
            }
        }
    }
}
