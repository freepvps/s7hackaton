using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MachineLang.Mapping
{
    public class FilterResult
    {
        public ITarget Target { get; private set; }
        public IEnumerable<LangToken> Tokens { get; private set; }
        public List<FilterResult> Childs { get; } = new List<FilterResult>();

        private static readonly LangToken[] empty = { };

        public FilterResult(ITarget target, IEnumerable<LangToken> tokens)
        {
            Target = target;
            if (tokens == null)
            {
                Tokens = empty;
            }
            else
            {
                Tokens = tokens.ToArray();
            }
        }

        public IEnumerable<LangToken> Process(MLang lang)
        {
            var used = new HashSet<LangToken>();
            if (Target != null)
            {
                foreach (var x in Target.Action.ProcessGroup(Tokens, lang))
                {
                    if (used.Add(x))
                    {
                        yield return x;
                    }
                }
            }
            foreach (var child in Childs)
            {
                foreach (var x in child.Process(lang))
                {
                    if (used.Add(x))
                    {
                        yield return x;
                    }
                }
            }
        }
    }
}
