using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MachineLang.TokenProcessors
{
    public class LowerCaseProcessor : ITokenProcessor
    {
        public string Name => "lower-case";

        public MLang MLang { get; set; }

        public void Init(XElement node)
        {
        }

        public IEnumerable<LangToken> Process(IEnumerable<LangToken> tokens)
        {
            foreach (var token in tokens)
            {
                token.Value = token.Value.ToLower();
                yield return token;
            }
        }
    }
}
