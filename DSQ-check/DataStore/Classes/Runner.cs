using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore.Classes
{
    public class Runner
    {
        private string _firstName, _lastName, _clubName;
        private uint? _ecard = null, _emitag1 = null, _emitag2 = null, _startNo = null;
        private int? _courseId;
        public Runner(string firstName, string lastName, int? courseId, string clubName, uint? ecard, uint? emitag1, uint? emitag2, uint? startno)
        {
            _firstName = firstName;
            _lastName = lastName;
            _courseId = courseId;
            _clubName = clubName;

            _ecard = ecard;
            _emitag1 = emitag1;
            _emitag2 = emitag2;
            _startNo = startno;
        }

        public string ClubName
        {
            get
            {
                return _clubName;
            }
        }
        public string FirstName
        {
            get
            {
                return _firstName;
            }
        }
        public string LastName
        {
            get
            {
                return _lastName;
            }
        }
        public int? CourseId
        {
            get
            {
                return _courseId;
            }
        }
        public uint? Ecard
        {
            get
            {
                return _ecard;
            }
        }
        public uint? EmiTag1
        {
            get
            {
                return _emitag1;
            }
        }
        public uint? EmiTag2
        {
            get
            {
                return _emitag2;
            }
        }
        public uint? StartNumber
        {
            get
            {
                return _startNo;
            }
        }

    }
}
