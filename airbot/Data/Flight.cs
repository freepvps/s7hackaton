using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.Data
{
    public class Flight
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Cost { get; set; }

        public Flight()
        {

        }
        public Flight(string from, string to, DateTime start, DateTime end)
        {
            From = from;
            To = to;
            StartTime = start;
            EndTime = end;
        }
    }
}
