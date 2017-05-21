using System;
using System.Collections.Generic;
using System.Text;
using airbot.TextGeneration;
using airbot.Data;
using Telegram.Bot.Types;
using MachineLang;

namespace airbot.Logic.Commands
{
    public class BuyTicket : CommandHandler
    {
        public Flight TargetFlight { get; set; }

        public override void Start(Message msg)
        {
            var userInfo = Session.UserInfo;
            if (string.IsNullOrWhiteSpace(userInfo.CurrentCity) || 
                string.IsNullOrWhiteSpace(userInfo.Name) || 
                string.IsNullOrWhiteSpace(userInfo.Passport) ||
                string.IsNullOrWhiteSpace(userInfo.Surname))
            {
                Session.StartHandler<AskProfile>();
            }
            else
            {
                Attach();
            }
        }

        public override void Attach()
        {
            var userInfo = Session.UserInfo;
            if (string.IsNullOrWhiteSpace(userInfo.CurrentCity) ||
                string.IsNullOrWhiteSpace(userInfo.Name) ||
                string.IsNullOrWhiteSpace(userInfo.Passport) ||
                string.IsNullOrWhiteSpace(userInfo.Surname))
            {
                Session.Exit(this);
                return;
            }
            Session.SendTextMessage(Messages.AskBuy(TargetFlight), Messages.YesNoPair).Wait();
        }
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var yes = tokens.Yes();
            if (yes == null)
            {
                Session.SendTextMessage(Messages.BuyUnknown(), Messages.YesNoPair).Wait();
                return false;
            }
            if (yes.Value)
            {
                var ticket = new Ticket();
                ticket.Flight = TargetFlight;
                ticket.TicketId = "s7110";//new Random().Next(100000, 1000000).ToString();
                Session.Worker.Database.Users[Session.ChatId.ToString()].Tickets.Add(ticket);

                Session.Worker.GeneralApi.Subscribe(msg.Chat.Id.ToString(), ticket.TicketId).Wait();
                Session.SendTextMessage(Messages.BuyComplete(TargetFlight, ticket)).Wait();

                Session.Exit(this, false);
                var askTransfer = new Transfer();
                askTransfer.Ticket = ticket;
                Session.StartHandler(askTransfer);
                return true;
            }
            else
            {
                Session.SendTextMessage(Messages.BuyFail(TargetFlight)).Wait();
                Session.Exit(this);
                return true;
            }
        }
    }
}
