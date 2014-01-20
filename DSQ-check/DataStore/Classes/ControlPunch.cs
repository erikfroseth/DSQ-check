using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore.Classes
{
    public class ControlPunch
    {
        private byte _controlCode, _controlNumber;
        private TimeSpan _usedTime;

        public ControlPunch(byte code, byte number, TimeSpan usedTime)
        {
            _controlCode = code;
            _controlNumber = number;
            _usedTime = usedTime;
        }

        public byte ControlCode
        {
            get
            {
                return _controlCode;
            }
        }
        public byte ControlNumber
        {
            get
            {
                return _controlNumber;
            }
        }
        public TimeSpan UsedTime
        {
            get
            {
                return _usedTime;
            }
        }
    }
}
