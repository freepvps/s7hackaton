using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.Data
{
    public class UserInfo
    {
        public string Name;
        public string Surname;
        public string Passport;
        public string CurrentCity;
        public bool LowCost = true;

        public List<Ticket> Tickets { get; } = new List<Ticket>();
    }
}
