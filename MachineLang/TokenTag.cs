using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MachineLang
{
    public class TokenTag
    {
        private readonly HashSet<LangToken> parentTokens = new HashSet<LangToken>();

        public string Key { get; set; }
        public string Value { get; set; }
        public double Priority { get; set; } = 1;

        public IEnumerable<LangToken> Parents
        {
            get
            {
                foreach (var parent in parentTokens)
                {
                    yield return parent;
                }
            }
        }
        public int LeftOffset { get; private set; } = -1;
        public int RightOffset { get; private set; } = -1;

        public void AddParent(LangToken token)
        {
            parentTokens.Add(token);
            if (LeftOffset == -1 || token.LeftOffset < LeftOffset) LeftOffset = token.LeftOffset;
            if (RightOffset == -1 || token.RightOffset > RightOffset) RightOffset = token.RightOffset;
        }
    }
}
