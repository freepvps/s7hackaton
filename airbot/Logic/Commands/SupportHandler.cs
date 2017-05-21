using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MachineLang;
using Telegram.Bot.Types;
using airbot.TextGeneration;

namespace airbot.Logic.Commands
{
    public class StartSupport : CommandHandler
    {
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            Session.Worker.GeneralApi.StartSupport(msg.Chat.Id.ToString()).Wait();
            Session.SendTextMessage("Начато общение с оператором").Wait();
            Session.Exit(this);
            return true;
        }
    }
    
    public class StopSupport : CommandHandler
    {
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            Session.Worker.GeneralApi.StopSupport(msg.Chat.Id.ToString()).Wait();
            Session.SendTextMessage("Общение с оператором завершено").Wait();
            Session.Exit(this);
            return true;
        }
    }

}
