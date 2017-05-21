using System;
using System.Collections.Generic;
using System.Text;
using MachineLang.TokenProcessors;

namespace MachineLang.Mapping
{
    public class MapAction
    {
        public Dictionary<string, string> TagExport { get; } = new Dictionary<string, string>();
        public Dictionary<string, double> TagPriority { get; } = new Dictionary<string, double>();
        public List<string> Handlers { get; } = new List<string>();

        public IEnumerable<LangToken> ProcessGroup(IEnumerable<LangToken> group, MLang lang)
        {
            foreach (var tagExport in TagExport)
            {
                var tag = new TokenTag
                {
                    Key = tagExport.Key,
                    Value = tagExport.Value
                };
                foreach (var token in group)
                {
                    token.Tags[tag.Key] = tag;
                    tag.AddParent(token);
                }
            }
            foreach (var tagPriority in TagPriority)
            {
                foreach (var token in group)
                {
                    if (token.Tags.TryGetValue(tagPriority.Key, out TokenTag tag))
                    {
                        tag.Priority = tagPriority.Value;
                    }
                }
            }
            if (Handlers.Count == 0)
            {
                foreach (var token in group)
                {
                    yield return token;
                }
            }
            else
            {
                var used = new HashSet<LangToken>();
                foreach (var handler in Handlers)
                {
                    if (lang.TextProcessors.TryGetValue(handler, out ITokenProcessor processor))
                    {
                        foreach (var r in processor.Process(group))
                        {
                            used.Add(r);
                        }
                    }
                }
                foreach (var result in group)
                {
                    if (used.Contains(result))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
