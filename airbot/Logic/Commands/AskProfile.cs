using System;
using System.Collections.Generic;
using System.Text;
using airbot.TextGeneration;
using MachineLang;
using System.Linq;
using Telegram.Bot.Types;

namespace airbot.Logic.Commands
{
    public class AskProfile : CommandHandler
    {
        private string Name { get; set; }
        private bool NameAsk { get; set; }

        public bool AskingName { get; set; } = true;
        public bool AskingSurname { get; set; } = true;
        public bool AskingPassport { get; set; } = true;
        public bool AskingCity { get; set; } = true;

        public bool FormWork { get; set; } = true;

        public override void Start(Message msg)
        {
            if (FormWork)
            {
                Session.SendTextMessage(Messages.AskForm).Wait();
            }
            AskSomething();
            return;
        }

        public void AskSomething()
        {
            if (string.IsNullOrWhiteSpace(Session.UserInfo.Name) && AskingName)
            {
                Session.SendTextMessage(Messages.AskName).Wait();
                return;
            }
            if (string.IsNullOrWhiteSpace(Session.UserInfo.Surname) && AskingSurname)
            {
                Session.SendTextMessage(Messages.AskSurname).Wait();
                return;
            }
            if (string.IsNullOrWhiteSpace(Session.UserInfo.Passport) && AskingPassport)
            {
                Session.SendTextMessage(Messages.AskPassport).Wait();
                return;
            }
            if (string.IsNullOrWhiteSpace(Session.UserInfo.CurrentCity) && AskingCity)
            {
                Session.SendTextMessage(Messages.AskCity).Wait();
                return;
            }
            if (FormWork)
            {
                Session.SendTextMessage(Messages.FormComplete).Wait();
            }
            Session.Exit(this);
        }

        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var yes = tokens.Yes();
            if (yes != null && !yes.Value && !NameAsk)
            {
                Session.SendTextMessage(Messages.OK()).Wait();
                Session.Exit(this);
                return true;
            }
            if (string.IsNullOrEmpty(Session.UserInfo.Name) && AskingName)
            {
                if (NameAsk)
                {
                    if (yes == null)
                    {
                        Session.UserInfo.Name = msg.Text.Trim();
                    }
                    else
                    {
                        if (yes.Value)
                        {
                            Session.UserInfo.Name = Name;
                            AskSomething();
                            return true;
                        }
                        else
                        {
                            NameAsk = false;
                            AskSomething();
                            return true;
                        }
                    }
                }
                var names = tokens.SelectNames();
                if (names.Count() != 1)
                {
                    NameAsk = true;
                    if (names.Count() > 0)
                    {
                        Name = names.First().Tags["name"].Value;
                    }
                    else
                    {
                        Name = msg.Text.Trim();
                    }
                    Session.SendTextMessage(Messages.NameReask(Name), Messages.YesNoPair).Wait();
                }
                else
                {
                    Session.UserInfo.Name = names.First().Tags["name"].Value;
                    AskSomething();
                }
                return true;
            }
            NameAsk = false;
            if (string.IsNullOrEmpty(Session.UserInfo.Surname) && AskingSurname)
            {
                Session.UserInfo.Surname = msg.Text.Trim();
                AskSomething();
                return true;
            }
            if (string.IsNullOrWhiteSpace(Session.UserInfo.Passport) && AskingPassport)
            {
                Session.UserInfo.Passport = msg.Text.Trim();
                AskSomething();
                return true;
            }
            if (string.IsNullOrEmpty(Session.UserInfo.CurrentCity) && AskingCity)
            {
                var cities = tokens.SelectCities();
                if (cities.Count() > 0)
                {
                    var city = cities.First().Tags["city"].Value;
                    Session.UserInfo.CurrentCity = city;
                    Session.SendTextMessage(Messages.CityComplete(city)).Wait();
                    Session.Exit(this);
                    return true;
                }
                else
                {
                    Session.SendTextMessage(Messages.CityFail()).Wait();
                    Session.Exit(this);
                    return true;
                }
            }
            Session.Exit(this);
            return true;
        }
    }
}
