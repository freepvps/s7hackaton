using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.Data
{
    public class Database
    {
        public List<Flight> Flights { get; } = new List<Flight>();
        public Dictionary<string, UserInfo> Users { get; } = new Dictionary<string, UserInfo>();
    }
}
