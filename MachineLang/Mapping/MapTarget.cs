using System;
using System.Collections.Generic;
using System.Text;
using MachineLang.Helpers;

namespace MachineLang.Mapping
{
    /*
     * Content filter
     * Tag filter
     */
    public class MapTarget : ITarget
    {
        public Dictionary<string, StringFilter> TagFilter { get; } = new Dictionary<string, StringFilter>();
        public MapAction Action { get; set; }

        public FilterResult SelectGroup(IEnumerable<LangToken> tokens, bool skipPrefix)
        {
            foreach (var token in tokens)
            {
                var ok = true;
                foreach (var tagFilter in TagFilter)
                {
                    if (
                        !token.Tags.TryGetValue(tagFilter.Key, out TokenTag tokenTag) 
                        || !tagFilter.Value.Check(tokenTag.Value))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    return new FilterResult(this, new LangToken[] { token });
                }
                if (!skipPrefix)
                {
                    break;
                }
            }
            return new FilterResult(this, null);
        }
    }
}
