using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.TextGeneration
{
    public class TextList<T> : List<T>
    {
        public string EndJoin { get; set; }
        public TextList(string endJoin = "и")
        {
            EndJoin = endJoin;
        }

        public override string ToString()
        {
            if (this.Count == 0) return string.Empty;
            if (this.Count == 1) return this[0].ToString();

            var sb = new StringBuilder();
            for (var i = 0; i < this.Count - 1; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(this[i].ToString());
            }
            sb.Append(" " + EndJoin + " ");
            sb.Append(this[this.Count - 1].ToString());
            return sb.ToString();
        }
    }
}
