using System;
using System.Collections.Generic;
using System.Text;
using MachineLang;
using Telegram.Bot.Types;
using airbot.TextApi;
using airbot.TextGeneration;
using System.Linq;
using airbot.Data;

namespace airbot.Logic.Commands
{
    public class TicketsSearch : CommandHandler
    {
        public string Cost;
        public string From;
        public string To;
        public DateTime LeftTime = DateTime.Now;
        public DateTime RightTime = DateTime.Now.AddDays(1);

        private Message unhandled = null;

        public override void Start(Message msg)
        {
            Cost = Session.UserInfo.LowCost ? "low" : "high";
            if (string.IsNullOrWhiteSpace(Session.UserInfo.CurrentCity))
            {
                var askProfile = new AskProfile();
                askProfile.AskingCity = true;
                askProfile.AskingName = askProfile.AskingPassport = askProfile.AskingSurname = false;
                askProfile.FormWork = false;
                unhandled = msg;
                Session.StartHandler(askProfile, msg);
                return;
            }
            else
            {
                From = Session.UserInfo.CurrentCity;
            }
            base.Start(msg);
        }

        public override void Attach()
        {
            if (unhandled != null)
            {
                if (string.IsNullOrWhiteSpace(Session.UserInfo.CurrentCity))
                {
                    Session.Exit(this);
                    return;
                }
                From = Session.UserInfo.CurrentCity;
                var msg = unhandled;
                unhandled = null;
                Process(msg);
            }
        }

        public IEnumerable<Flight> Search()
        {
            if (RightTime < LeftTime) RightTime = LeftTime.AddHours(3);
            var res = new List<Flight>();
            var flights = Session.Worker.Database.Flights.Where(x => x.From == From && x.To == To).ToArray();
            foreach (var flight in flights)
            {
                if (flight.StartTime >= LeftTime && flight.StartTime < RightTime)
                {
                    res.Add(flight);
                }
            }
            if (Cost == "high")
            {
                res = res.OrderBy(x => Tuple.Create(x.StartTime, -x.Cost)).ToList();
            }
            else
            {
                res = res.OrderBy(x => Tuple.Create(x.StartTime, x.Cost)).ToList();
            }

            return res.Take(3);
        }

        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            if (int.TryParse(msg.Text, out int target))
            {
                if (target >= 1 && target <= 3)
                {
                    var lastSearch = Search().ToArray();
                    if (lastSearch.Length >= target)
                    {
                        var selected = lastSearch[target - 1];
                        var buy = new BuyTicket();
                        buy.TargetFlight = selected;
                        Session.Exit(this, false);
                        Session.StartHandler(buy);

                        if (Cost == "high") Session.UserInfo.LowCost = false;
                        if (Cost == "low") Session.UserInfo.LowCost = true;

                        return true;
                    }
                }
            }


            var handled = false;
            if (UserTimeParser.TryParseDatetime(msg.Text, out Tuple<DateTime, DateTime> datetime))
            {
                LeftTime = datetime.Item1;
                if (DateTime.Now.AddHours(2) > LeftTime)
                {
                    LeftTime = DateTime.Now.AddHours(2);
                }
                RightTime = datetime.Item2;
                if (RightTime < LeftTime)
                {
                    RightTime = LeftTime.AddHours(3);
                }

                handled = true;
            }
            var cost = tokens.Select("cost").ToArray();
            var cityFrom = tokens.Select("cityFrom").ToArray();
            var cityTo = tokens.Select("cityTo").ToArray();

            foreach (var c in cost)
            {
                Cost = c.Tags["cost"].Value;
                handled = true;
            }
            foreach (var cf in cityFrom)
            {
                From = cf.Tags["city"].Value;
                handled = true;
            }
            foreach (var ct in cityTo)
            {
                To = ct.Tags["city"].Value;
                handled = true;
            }

            var flights = Search();
            if (flights.Count() == 0)
            {
                Session.SendTextMessage(Messages.NothingFlightFound).Wait();
                return false;
            }
            else
            {
                var list = string.Join(Environment.NewLine, flights.Select(x => x.HumanLike()));
                Session.SendTextMessage(list, Enumerable.Range(1, flights.Count()).Select(x => x.ToString()).ToArray()).Wait();
            }

            return handled;
        }
    }
}
