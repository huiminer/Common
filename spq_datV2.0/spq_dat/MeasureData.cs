using System;
using System.Collections.Generic;
using System.Text;

namespace spq_dat
{
    class MeasureData
    {
        private string time;
        public string Time
        {
            get { return time; }
            set { time = value; }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private string data;
        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        private float err;
        public float Err
        {
            get { return err; }
            set { err = value; }
        }
    }
}
