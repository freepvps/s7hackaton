using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MachineLang.TokenProcessors
{
    public class IntegerExport : ITokenProcessor
    {
        public string Name => "integer-export";

        public MLang MLang { get; set; }

        public void Init(XElement node)
        {
        }

        public IEnumerable<LangToken> Process(IEnumerable<LangToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (long.TryParse(token.Value, out long x))
                {
                    var newTag = new TokenTag();
                    newTag.Key = "integer";
                    newTag.Value = x.ToString();

                    token.Tags[newTag.Key] = newTag;
                    newTag.AddParent(token);
                    yield return token;
                }
            }
        }
    }
}
