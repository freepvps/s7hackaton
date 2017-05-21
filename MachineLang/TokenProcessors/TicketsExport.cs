using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using MachineLang;
using MachineLang.TokenProcessors;
using System.Text.RegularExpressions;

namespace MachineLang.TokenProcessors
{
    public class TicketsExport : ITokenProcessor
    {
        public string Name => "ticket-export";

        public MLang MLang { get; set; }

        public void Init(XElement node)
        {
        }

        public IEnumerable<LangToken> Process(IEnumerable<LangToken> tokens)
        {
            Regex rgx = new Regex(@"^[a-zA-Z0-9]{5}$");
            foreach (var token in tokens)
            {
                if (rgx.IsMatch(token.Value))
                {
                    var tag = new TokenTag
                    {
                        Key = "ticket",
                        Value = token.Value
                    };
                    token.Tags[tag.Key] = tag;
                    tag.AddParent(token);
                    yield return token;
                }
            }
        }
    }
}
