using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Telegram.Bot.Types;
using airbot.TextGeneration;
using MachineLang;
using airbot.Logic.Commands;

namespace airbot.Logic
{
    public class StartupCommand : CommandHandler
    {
        public override void Start(Message msg)
        {
            Session.SendTextMessage(Messages.Hello()).Wait();
            Session.SendTextMessage(Messages.AskQuestion()).Wait();
        }

        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var bagageCheck = tokens.SelectBagageCheck().ToArray();
            var subscribe = tokens.SelectSubscribe().ToArray();
            var tickets = tokens.SelectTickets().ToArray();
            var simple_response = tokens.SelectSimpleResponse().ToArray();
            var should_start_support = tokens.ShouldStartSupport();
            var should_stop_support = tokens.ShouldStopSupport();
            if (should_stop_support) {
                Session.StartHandler<StopSupport>(msg);
                return true;
            } 
            if (should_start_support) {
                Session.StartHandler<StartSupport>(msg);
                return true;
            }
            if (Session.Worker.GeneralApi.CheckSupportEnabled(Session.ChatId).Result) {
                return true;
            }
            var searchTickets = tokens.Select("searchTicket").ToArray();
            if (simple_response.Length > 0)
            {
                Session.StartHandler<SimpleResponse>(msg);
                return true;
            }
            if (searchTickets.Length > 0)
            {
                Session.StartHandler<TicketsSearch>(msg);
                return true;
            }
            if (bagageCheck.Length > 0)
            {
                Session.StartHandler<BagageCheck>(msg);
                return true;
            }
            if (subscribe.Count() > 0)
            {
                Session.StartHandler<Subscribe>(msg);
                return true;
            }

            try
            {
                if (msg.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    var faqResult = Session.Worker.TextApi.Faq(msg.Text).Result;
                    Session.SendTextMessage(faqResult).Wait();
                    return true;
                }
            }
            catch
            {

            }

            Session.SendTextMessage(Messages.ErrorSorry()).Wait();
            return false;
        }
    }
}
