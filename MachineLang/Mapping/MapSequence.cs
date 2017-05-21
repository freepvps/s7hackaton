using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MachineLang.Mapping
{
    public class MapSequence : ITarget
    {
        public class SequenceToken
        {
            public ITarget Target;
            public int MinCount = 1;
            public int MaxCount = 1;

            public bool ActionHandling = true;
            public bool Maximize = false;
        }

        public readonly List<SequenceToken> Sequence = new List<SequenceToken>();

        public MapAction Action { get; set; }

        private bool TrySelect(Stack<LangToken> tokens, Stack<LangToken> temp, Stack<FilterResult> result, int seqId, int seqItemCount)
        {
            if (seqId == Sequence.Count)
            {
                return true;
            }
            var seqToken = Sequence[seqId];
            if (!seqToken.Maximize && seqItemCount >= seqToken.MinCount)
            {
                if (TrySelect(tokens, temp, result, seqId + 1, 0))
                {
                    return true;
                }
            }
            if (seqItemCount < seqToken.MaxCount && tokens.Count > 0)
            {
                var group = seqToken.Target.SelectGroup(tokens, false);
                if (group.Tokens.Count() == 0)
                {
                    return false;
                }
                var rOffset = group.Tokens.Select(x => x.RightOffset).Max();

                var lastCnt = temp.Count;
                while (tokens.Count > 0 && tokens.Peek().RightOffset <= rOffset)
                {
                    temp.Push(tokens.Pop());
                }
                if (TrySelect(tokens, temp, result, seqId, seqItemCount + 1))
                {
                    if (seqToken.ActionHandling)
                    {
                        result.Push(group);
                    }
                    return true;
                }
                if (temp.Count > lastCnt)
                {
                    tokens.Push(temp.Pop());
                }
                return false;
            }
            if (seqToken.Maximize && seqItemCount >= seqToken.MinCount)
            {
                if (TrySelect(tokens, temp, result, seqId + 1, 0))
                {
                    return true;
                }
            }
            return false;
        }
        public FilterResult SelectGroup(IEnumerable<LangToken> tokens, bool skipPrefix)
        {
            var tokensStack = new Stack<LangToken>();
            var tempStack = new Stack<LangToken>();
            var resultStack = new Stack<FilterResult>();

            foreach (var token in tokens.Reverse())
            {
                tokensStack.Push(token);
            }

            while (tokensStack.Count > 0)
            {
                var result = TrySelect(tokensStack, tempStack, resultStack, 0, 0);
                if (result)
                {
                    var resTokens = resultStack.SelectMany(x => x.Tokens).Distinct().OrderBy(x => x.LeftOffset);
                    var filterResult = new FilterResult(this, resTokens);
                    foreach (var sub in resultStack)
                    {
                        filterResult.Childs.Add(sub);
                    }
                    return filterResult;
                }
                if (tokensStack.Count > 0)
                {
                    tokensStack.Pop();
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
