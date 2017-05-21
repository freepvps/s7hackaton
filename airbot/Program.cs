using System;
using System.IO;

using Newtonsoft.Json;
using System.Text;

using Telegram.Bot;
using Telegram.Bot.Types;

using System.Threading;
using System.Threading.Tasks;

using File = System.IO.File;
using airbot.Logic;

namespace airbot
{
    class Program
    {
        const string NLogConfig = "./NLog.config";
        const string ConfigPath = "./bot.json";
        const string DatabasePath = "./db.json";

        public static NLog.Logger MainLog = NLog.LogManager.GetLogger("main");
        public static Config Config { get; set; }
        
        public static ITelegramBotClient TelegramBot { get; set; }
        public static AirBotWorker AirBot { get; set; }
        public static MachineLang.MLang MLang { get; set; }

        public static Data.Database DB;
        
        public static void Save(Config config)
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config), Encoding.UTF8);
        }

        public static void Main(string[] args)
        {
            if (File.Exists(NLogConfig))
            {
                NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(NLogConfig);
            }
            MainLog.Trace("Started");
            if (!File.Exists(ConfigPath))
            {
                Save(new Config());
                MainLog.Trace("Config saved");
                return;
            }
            if (!File.Exists(DatabasePath))
            {
                File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(Data.Generatator.Generate()), Encoding.UTF8);
            }
            DB = JsonConvert.DeserializeObject<Data.Database>(File.ReadAllText(DatabasePath, Encoding.UTF8));
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath, Encoding.UTF8));

            TelegramBot = new TelegramBotClient(Config.TelegramToken);
            MLang = MachineLang.MLang.Load(Config.LangBase);
            AirBot = new AirBotWorker(MLang, TelegramBot, Config, DB);

            AirBot.Start();
            new Thread(() =>
            {
                while (true)
                {
                    File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(DB), Encoding.UTF8);
                    Thread.Sleep(10000);
                }
            }).Start();
            Task.Delay(Timeout.Infinite).Wait();
        }
    }
}