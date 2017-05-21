using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MachineLang;
using Telegram.Bot.Types;
using airbot.TextGeneration;

namespace airbot.Logic.Commands
{
    public class SimpleResponse : CommandHandler
    {
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var simpleResponse = tokens.SelectSimpleResponse().ToArray();
            Session.SendTextMessage(simpleResponse[0].Tags["simple_response"].Value).Wait();
            Session.Exit(this);
            return true;
        }
    }
}
