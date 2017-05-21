using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using MachineLang;
using airbot.Data;
using Telegram.Bot.Types.ReplyMarkups;

namespace airbot.Logic
{
    public class TgSession
    {
        public ChatId ChatId { get; }
        public MLang MLang { get; }
        
        public AirBotWorker Worker { get; }

        public CommandHandler MainHandler { get;}

        public UserInfo UserInfo { get; }

        private Stack<CommandHandler> HandlersStack { get; } = new Stack<CommandHandler>();
        private DateTime UpdateTime { get; set; }
        private DateTime LastHello { get; set; }
        private int TimeLimit { get; set; } = 120000;

        public TgSession(ChatId chatId, AirBotWorker worker, UserInfo user)
        {
            Worker = worker;
            UserInfo = user;

            ChatId = chatId;
            MainHandler = new StartupCommand();

            HandlersStack = new Stack<CommandHandler>();

            MLang = worker.MLang;

            Initialize(MainHandler);
        }

        public async Task SendTextMessage(string message, params string[] items)
        {
            try
            {
                IReplyMarkup replyMarkup;
                if (items.Count() > 0)
                {
                    replyMarkup = new ReplyKeyboardMarkup(
                        items.Select(x => new KeyboardButton(x)).Select(x => new[] { x }).ToArray(),
                        oneTimeKeyboard: true
                        );
                }
                else
                {
                    replyMarkup = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove
                    {
                        RemoveKeyboard = true
                    };
                }
                var msg = await Worker.TelegramApi.SendTextMessageAsync(ChatId, message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyMarkup: replyMarkup
                    );

                var task = Worker.GeneralApi.Send(msg.Date, msg.Text, msg.Chat.Id.ToString(), Worker.BotInfo.Username, msg.Chat.Username);
                task.Wait();
            }
            catch (Exception ex){
                Console.WriteLine(ex);
            }
        }

        public T StartHandler<T>(Message msg = null) where T : CommandHandler, new()
        {
            return StartHandler<T>(new T(), msg);
        }
        public T StartHandler<T>(T tgCmdHandler, Message msg = null) where T : CommandHandler
        {
            StartHandler((CommandHandler)tgCmdHandler, msg);
            return tgCmdHandler;
        }
        public CommandHandler StartHandler(CommandHandler tgCmdHandler, Message msg = null)
        {
            lock (HandlersStack)
            {
                HandlersStack.Push(tgCmdHandler);
                Initialize(tgCmdHandler);
                tgCmdHandler.Start(msg);
                UpdateTime = DateTime.Now;
            }
            return tgCmdHandler;
        }
        public void Exit(CommandHandler tgCmdHandler, bool pulse = true)
        {
            lock (HandlersStack)
            {
                if (HandlersStack.Contains(tgCmdHandler))
                {
                    while (HandlersStack.Count > 0)
                    {
                        var last = HandlersStack.Pop();
                        UpdateTime = DateTime.Now;
                        if (last == tgCmdHandler)
                        {
                            break;
                        }
                    }
                    if (pulse)
                    {
                        if (HandlersStack.Count > 0)
                        {
                            HandlersStack.Peek().Attach();
                        }
                        else
                        {
                            MainHandler.Attach();
                        }
                    }
                }
            }
        }
        
        public void ExitAll()
        {
            lock (HandlersStack)
            {
                while (HandlersStack.Count > 0)
                {
                    Exit(HandlersStack.Peek());
                }
            }
        }

        protected void Initialize(CommandHandler tgCmdHandler)
        {
            tgCmdHandler.Session = this;
            tgCmdHandler.Initialize();
        }
        
        public void Process(Message msg)
        {
            //if (UpdateTime.AddMilliseconds(HelloLimit) < DateTime.Now && LastHello.AddMilliseconds(HelloLimit) < DateTime.Now)
            //{
            //    LastHello = DateTime.Now;
            //    SendTextMessage(TextGeneration.Messages.Hello()).Wait();
            //}
            Worker.TelegramApi.SendChatActionAsync(msg.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing).Wait();
            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                Worker.GeneralApi.Send(msg.Date, msg.Text, msg.Chat.Id.ToString(), msg.Chat.Username, Worker.BotInfo.Username).Wait();
            }

            if (UpdateTime.AddMilliseconds(TimeLimit) < DateTime.Now)
            {
                ExitAll();
            }
            if (HandlersStack.Count > 0)
            {
                var handled = HandlersStack.Peek().Process(msg);
                UpdateTime = DateTime.Now;
                return;
            }
            MainHandler.Process(msg);
        }
    }
}
