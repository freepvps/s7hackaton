using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;

using airbot.General.Types;

namespace airbot.TextApi
{
    public class Api
    {
        public string ApiUrl { get; set; }

        private Dictionary<string, string> Cache = new Dictionary<string, string>();

        public Api()
        {
        }
        public Api(string apiUrl = "") : this()
        {
            ApiUrl = apiUrl;
        }

        public async Task<string> Faq(string s)
        {
            return await Send("", new Dictionary<string, object>
            {
                ["q"] = s
            });
        }
        public async Task<string> Stem(string s)
        {
            if (!Cache.TryGetValue(s, out string result))
            {
                result = await Send("stem", new Dictionary<string, object>
                {
                    ["q"] = s
                });
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = s;
                }
                Cache[s] = result;
            }
            return result;
        }

        public async Task<string> Send(string method, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            using (var httpClient = new HttpClient())
            {
                var url = $"{ApiUrl}/{method}";

                var msg = new HttpRequestMessage(HttpMethod.Post, url);
                msg.Content = new FormUrlEncodedContent(parameters.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));
                var resp = await httpClient.SendAsync(msg);
                var text = await resp.Content.ReadAsStringAsync();

                return text;
            }
        }
    }
}
