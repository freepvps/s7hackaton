using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLang
{
    public class LangToken
    {
        public int LeftOffset { get; set; }
        public int RightOffset { get; set; }

        public string Value
        {
            get
            {
                if (Tags.TryGetValue("value", out TokenTag token))
                {
                    return token.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                Tags["value"] = new TokenTag
                {
                    Key = "value",
                    Value = value
                };
            }
        }
        public Dictionary<string, TokenTag> Tags { get; } = new Dictionary<string, TokenTag>();
    }
}
