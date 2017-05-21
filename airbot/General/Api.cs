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

namespace airbot.General
{
    public class Api
    {
        public string ApiUrl { get; set; }
        
        public Api()
        {
        }
        public Api(string apiUrl = "") : this()
        {
            ApiUrl = apiUrl;
        }
        
        public async Task Send(DateTime time, string text, string chatId, string sender, string username)
        {
            await Send(new Message
            {
                Time = time,
                Text = text,
                ChatId = chatId,
                Sender = sender,
                Username = username
            });
        }
        public async Task Send(Message message)
        {
            await Send("message", message);
        }
        public async Task Subscribe(string chatId, string flight)
        {
            await Send("subscribe", arg: new Dictionary<string, string>
            {
                ["chat_id"] = chatId,
                ["flight"] = flight
            });
        }

        public async Task StartSupport(string chatId)
        {
            await Send("proxy", arg : new Dictionary<string, string> 
            {
                ["chat_id"] = chatId
            });
        }
        
        public async Task StopSupport(string chatId)
        {
            await Send("proxy/finish", arg : new Dictionary<string, string> 
            {
                ["chat_id"] = chatId
            });
        }

        public async Task<bool> CheckSupportEnabled(string chatId)
        {
            try {
                await Send("proxy/status", arg : new Dictionary<string, string> 
                {
                    ["chat_id"] = chatId
                });
            } catch {
                return false;
            }
            return true;
        }

        public async Task Send(string method, Dictionary<string, object> parameters = null)
        {
            await Send<object>(method, null, parameters);
        }

        public async Task Send<TArg>(string method, TArg arg, Dictionary<string, object> parameters = null)
        {
            using (var httpClient = new HttpClient())
            {
                var query = string.Empty;
                if (parameters != null)
                {
                    query = string.Join("&", parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value.ToString())}"));
                }
                var url = $"{ApiUrl}/{method}?{query}";
                NLog.LogManager.GetLogger("general-api").Info($"General result url {url}");

                var msg = new HttpRequestMessage(HttpMethod.Post, url);
                if (arg != null)
                {
                    msg.Content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8);
                    msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                }
                var resp = await httpClient.SendAsync(msg);
                var text = await resp.Content.ReadAsStringAsync();
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ApiResult>(text);
                    if ((result?.Required?.Length ?? 0) > 0)
                    {
                        NLog.LogManager.GetLogger("general-api").Info($"{result.Error} : [ {string.Join(", ", result.Required)} ]");
                        throw new Exception($"Require {string.Join(", ", result.Required)}");
                    }
                    throw new Exception(resp.StatusCode.ToString());
                }
            }
        }
    }
}
