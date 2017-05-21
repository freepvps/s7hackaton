using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using airbot.TextGeneration;
using MachineLang;
using Telegram.Bot.Types;

namespace airbot.Logic.Commands
{
    public class Subscribe : CommandHandler
    {
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var tickets = tokens.SelectTickets().ToArray();

            foreach (var ticket in tickets)
            {
                Session.Worker.GeneralApi.Subscribe(msg.Chat.Id.ToString(), ticket.Value).Wait();
            }

            Session.SendTextMessage(Messages.Subscribe(tickets.Select(x => x.Value))).Wait();

            Session.Exit(this);
            return true;
        }
    }
}
