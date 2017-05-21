using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MachineLang.Mapping
{
    public class MapGroup
    {
        private readonly Dictionary<string, Dictionary<string, List<ITarget>>> TagTargets = new Dictionary<string, Dictionary<string, List<ITarget>>>();
        private readonly List<ITarget> AnotherTargets = new List<ITarget>();

        public MLang Lang { get; }
        public MapGroup(MLang lang)
        {
            Lang = lang;
        }

        public bool Replace { get; set; }

        public void AddTarget(ITarget target)
        {
            if (target is MapTarget)
            {
                var mapTarget = (MapTarget)target;
                var ok = false;
                foreach (var x in mapTarget.TagFilter)
                {
                    var primitiveToken = x.Value.PrimitiveToken;
                    if (!string.IsNullOrWhiteSpace(primitiveToken))
                    {
                        if (!TagTargets.TryGetValue(x.Key, out Dictionary<string, List<ITarget>> targetsDict))
                        {
                            TagTargets[x.Key] = targetsDict = new Dictionary<string, List<ITarget>>();
                        }
                        if (!targetsDict.TryGetValue(primitiveToken, out List<ITarget> targets))
                        {
                            targetsDict[primitiveToken] = targets = new List<ITarget>();
                        }
                        targets.Add(target);
                        ok = true;
                    }
                }
                if (!ok)
                {
                    AnotherTargets.Add(target);
                }
            }
            else
            {
                AnotherTargets.Add(target);
            }
        }
        
        private bool ProcessTarget(ITarget target, Queue<LangToken> nextQueue, HashSet<LangToken> used, HashSet<LangToken> handled)
        {
            var ok = false;

            var rOffset = nextQueue.Peek().RightOffset - 1;
            while (nextQueue.Count > 0)
            {
                if (handled.Contains(nextQueue.Peek()) || rOffset >= nextQueue.Peek().RightOffset)
                {
                    nextQueue.Dequeue();
                    continue;
                }
                var filtered = target.SelectGroup(nextQueue, true);
                if (filtered.Tokens.Count() == 0)
                {
                    break;
                }
                foreach (var q in filtered.Tokens)
                {
                    rOffset = Math.Max(rOffset, q.RightOffset);
                    handled.Add(q);
                }
                ok = true;
                foreach (var x in filtered.Process(Lang))
                {
                    used.Add(x);
                }
            }
            return ok;
        }
        public IEnumerable<LangToken> ProcessTokens(IEnumerable<LangToken> tokens)
        {
            var handled = new HashSet<LangToken>();
            var used = new HashSet<LangToken>();
            var queue = new Queue<LangToken>(tokens);
            var nextQueue = new Queue<LangToken>(tokens);
            while (queue.Count > 0)
            {
                var token = queue.Peek();
                var tags = token.Tags.ToArray();
                foreach (var tag in tags)
                {
                    if (TagTargets.TryGetValue(tag.Key, out Dictionary<string, List<ITarget>> tagBag))
                    {
                        if (tagBag.TryGetValue(tag.Value.Value, out List<ITarget> targets))
                        {
                            foreach (var target in targets)
                            {
                                if (ProcessTarget(target, nextQueue, used, handled))
                                {
                                    nextQueue = new Queue<LangToken>(queue.Where(t => !handled.Contains(t)));
                                }
                            }
                        }
                    }
                }
                if (nextQueue.Count == queue.Count)
                {
                    if (nextQueue.Count <= 1)
                    {
                        break;
                    }
                    nextQueue.Dequeue();
                    queue.Dequeue();
                }
                else
                {
                    queue = new Queue<LangToken>(nextQueue);
                }
            }


            queue = new Queue<LangToken>(tokens.Where(t => !handled.Contains(t)));
            nextQueue = new Queue<LangToken>(queue);
            foreach (var another in AnotherTargets)
            {
                if (nextQueue.Count == 0)
                {
                    break;
                }
                if (ProcessTarget(another, nextQueue, used, handled))
                {
                    nextQueue = new Queue<LangToken>(queue.Where(t => !handled.Contains(t)));
                }
            }

            return tokens.Where(x => !Replace || used.Contains(x)).ToArray();
        }
    }
}
