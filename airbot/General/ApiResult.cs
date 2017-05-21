using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace airbot.General
{
    public class ApiResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
        public string[] Required { get; set; }
    }
}
