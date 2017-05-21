using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using MachineLang;
using airbot.Data;

namespace airbot.Logic
{
    public class AirBotWorker
    {
        private object workingLock = new object();
        
        public ITelegramBotClient TelegramApi { get; }
        public General.Api GeneralApi { get; }
        public TextApi.Api TextApi { get; }

        public Data.Database Database { get; }

        public bool Started { get; private set; }

        public MLang MLang { get; }
        public Config Config { get; }
        public User BotInfo { get; }

        private int UpdateId = 0;

        public Dictionary<string, TgSession> SessionsTable { get; private set; }

        public AirBotWorker(MLang mlang, ITelegramBotClient telegramApi, Config config, Database database)
        {
            SessionsTable = new Dictionary<string, TgSession>();
            Database = database;

            MLang = mlang;
            TelegramApi = telegramApi;
            Config = config;
            GeneralApi = new General.Api(config.GeneralApi);
            TextApi = new airbot.TextApi.Api(config.TextApi);
            BotInfo = telegramApi.GetMeAsync().Result;
        }

        private TgSession CreateSession(Message msg)
        {
            if (!Database.Users.TryGetValue(msg.Chat.Id.ToString(), out UserInfo userInfo))
            {
                userInfo = new UserInfo();
                userInfo.Name = msg.Chat.FirstName;
                userInfo.Surname = msg.Chat.LastName;
                Database.Users[msg.Chat.Id.ToString()] = userInfo;
            }
            return new TgSession(msg.Chat.Id, this, userInfo);
        }

        private Task WorkTask;
        private CancellationTokenSource TokenSource;
        public void Start()
        {
            lock (workingLock)
            {
                if (Started)
                {
                    return;
                }
                TokenSource = new CancellationTokenSource();
                Started = true;
                WorkTask = Task.Run(Working, TokenSource.Token);
            }
        }
        public void Stop()
        {
            lock (workingLock)
            {
                if (!Started)
                {
                    return;
                }

                TokenSource.Cancel();
                WorkTask = null;
                TokenSource = null;
                Started = false;
            }
        }
        private async Task Working()
        {
            var tasks = new Queue<Task>();
            while (Started)
            {
                try
                {
                    ITelegramBotClient api = TelegramApi;
                    foreach (var update in await api.GetUpdatesAsync(UpdateId))
                    {
                        UpdateId = update.Id + 1;
                        if (update.Message == null)
                        {
                            continue;
                        }
                        TgSession session;
                        if (!SessionsTable.TryGetValue(update.Message.Chat.Id.ToString(), out session))
                        {
                            session = CreateSession(update.Message);
                            SessionsTable[update.Message.Chat.Id] = session;
                        }
                        if (session == null)
                        {
                            continue;
                        };
                        try
                        {
                            session.Process(update.Message);
                            //var task = Task.Run(() => session.Process(update.Message));
                            //tasks.Enqueue(task);
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Source);
                }
                while (tasks.Count > 0)
                {
                    try
                    {
                        var q = tasks.Dequeue();
                        q.Wait();
                    }
                    catch
                    {

                    }
                }
                await Task.Delay(1);
            }
        }

    }
}
