using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MachineLang;
using Telegram.Bot;
using Telegram.Bot.Types;
using airbot.TextGeneration;

namespace airbot.Logic
{
    public abstract class CommandHandler
    {
        public TgSession Session { get; set; }

        public virtual void Start(Message msg)
        {
            Process(msg);
        }
        public virtual bool Process(Message msg)
        {
            if (Command(msg))
            {
                return true;
            }
            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                var tokens = Session.MLang.ParseTokens(msg.Text).ToArray();
                foreach (var token in tokens)
                {
                    token.Value = Session.Worker.TextApi.Stem(token.Value).Result;
                }
                tokens = Session.MLang.ProcessTokens(tokens).ToArray();
                return Tokens(msg, tokens.ToArray());
            }
            return false;
        }
        public virtual void Attach() { }
        public virtual bool Command(Message msg)
        {
            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                switch (msg.Text)
                {
                    case "/start":
                    case "/help":
                        Session.SendTextMessage(Messages.Help).Wait();
                        return true;
                }
            }
            return false;
        }
        public virtual bool Tokens(Message msg, LangToken[] tokens) { return false; }
        public virtual void Stop()
        {
            Session.Exit(this);
        }

        public virtual void Initialize() {  }
    }
}
