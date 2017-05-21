using System;
using System.Collections.Generic;
using System.Text;
using airbot.Data;
using Telegram.Bot.Types;
using airbot.TextGeneration;
using MachineLang;
using System.Linq;

namespace airbot.Logic.Commands
{
    public class Transfer : CommandHandler
    {
        public Ticket Ticket { get; set; }

        public override void Start(Message msg)
        {
            Session.SendTextMessage(Messages.TransferAsk, Messages.Transfering).Wait();
        }
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var fromCnt = tokens.Where(t => t.Tags.ContainsKey("base") && t.Tags["base"].Value == "from").Count();
            var toCnt = tokens.Where(t => t.Tags.ContainsKey("base") && t.Tags["base"].Value == "to").Count();

            Ticket.InTransfer = toCnt > 0;
            Ticket.OutTransfer = fromCnt > 0;

            var ans = Messages.TransferResult(toCnt > 0, fromCnt > 0, Ticket);
            Session.SendTextMessage(ans).Wait();
            Session.Exit(this);
            return true;
        }

    }
}
