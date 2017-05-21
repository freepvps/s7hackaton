using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.Data
{
    public class Ticket
    {
        public string TicketId { get; set; }
        public Flight Flight { get; set; }
        public bool InTransfer { get; set; }
        public bool OutTransfer { get; set; }
    }
}
