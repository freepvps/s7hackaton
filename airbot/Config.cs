using System;
using System.Collections.Generic;
using System.Text;

namespace airbot
{
    public class Config
    {
        public string TelegramToken { get; set; }
        public string LangBase { get; set; }

        public string GeneralApi { get; set; }
        public string TextApi { get; set; }
    }
}
