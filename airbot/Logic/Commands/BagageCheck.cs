using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MachineLang;
using Telegram.Bot.Types;
using airbot.TextGeneration;

namespace airbot.Logic.Commands
{
    public class BagageCheck : CommandHandler
    {
        public override bool Tokens(Message msg, LangToken[] tokens)
        {
            var bagageCheck = tokens.SelectBagageCheck().ToArray();

            var positive = new List<string>();
            var negative = new List<string>();
            
            foreach (var token in bagageCheck)
            {
                if (token.Tags["bagage"].Value == "true")
                {
                    positive.Add(token.Value);
                }
                else
                {
                    negative.Add(token.Value);
                }
            }

            positive = positive.Distinct().ToList();
            negative = negative.Distinct().ToList();

            var answer = Messages.AboardResult(positive, negative);
            Session.SendTextMessage(answer).Wait();
            Session.Exit(this);
            return true;
        }
    }
}
