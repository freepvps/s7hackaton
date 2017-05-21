using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace airbot.General.Types
{
    public class Message
    {
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(airbot.General.Ext.UnixTimeConverter))]
        public DateTime Time { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
