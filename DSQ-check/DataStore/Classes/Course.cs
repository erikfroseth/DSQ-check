using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.DataStore.Classes
{
    public class Course
    {
        private int _courseId;
        private string _courseName;
        private List<byte> _controls;

        public Course(int courseId, string courseName)
        {
            _courseId = courseId;
            _courseName = courseName;

            _controls = new List<byte>();
        }

        public void AddControl(byte controlCode)
        {
            if (_controls.Count > 0 && _controls.Last() == controlCode)
            {
                throw new InvalidOperationException();
            }
            else
            {
                _controls.Add(controlCode);
            }
        }

        public int CourseId
        {
            get
            {
                return _courseId;
            }
        }
        public string CourseName
        {
            get 
            {
                return _courseName;
            }
        }
        public IEnumerable<byte> Controls
        {
            get
            {
                return _controls;
            }
        }
    }
}
