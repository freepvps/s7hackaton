using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLang.Helpers
{

    // TODO: Optimize
    public class StringFilter
    {
        private struct StringFilterToken
        {
            public string Value;
            public bool IsControl;
        }
        private readonly List<StringFilterToken> tokens = new List<StringFilterToken>();
        public string PrimitiveToken
        {
            get
            {
                if (tokens.Count == 1 && !tokens[0].IsControl)
                {
                    return tokens[0].Value;
                }
                return null;
            }
        }

        public static readonly StringFilter Default = new StringFilter();

        public static StringFilter Parse(string s)
        {
            // ab?cd => [ "ab", "?", "cd" ] : abxcd
            // ab\?\\cd => [ "ab?\cd" ] : a
            // 

            var result = new StringFilter();

            var sb = new StringBuilder();
            var control = false;
            char lastChar = '\0';
            foreach (var ch in s)
            {
                if (control)
                {
                    sb.Append(ch);
                    lastChar = ch;
                }
                else
                {
                    switch (ch)
                    {
                        case '\\':
                            control = true;
                            lastChar = ch;
                            continue;
                        case '*':
                            if (lastChar != ch)
                            {
                                lastChar = ch;
                                if (sb.Length > 0)
                                {
                                    result.tokens.Add(new StringFilterToken
                                    {
                                        Value = sb.ToString(),
                                        IsControl = false
                                    });
                                    sb.Clear();
                                }
                                result.tokens.Add(new StringFilterToken
                                {
                                    Value = "*",
                                    IsControl = true
                                });
                            }
                            continue;
                        case '?':
                            lastChar = ch;
                            if (sb.Length > 0)
                            {
                                result.tokens.Add(new StringFilterToken
                                {
                                    Value = sb.ToString(),
                                    IsControl = false
                                });
                                sb.Clear();
                            }
                            result.tokens.Add(new StringFilterToken
                            {
                                Value = "?",
                                IsControl = true
                            });

                            continue;
                        default:
                            sb.Append(ch);
                            continue;
                    }
                }
            }

            if (sb.Length > 0)
            {
                result.tokens.Add(new StringFilterToken
                {
                    Value = sb.ToString(),
                    IsControl = false
                });
            }
            return result;
        }

        private bool TryCheck(int tokenOffset, string s, int offset)
        {
            if (tokenOffset == tokens.Count)
            {
                return offset == s.Length;
            }

            var tokenValue = tokens[tokenOffset].Value;
            var tokenIsControl = tokens[tokenOffset].IsControl;

            if (tokenIsControl)
            {
                if (tokenValue == "?")
                {
                    return TryCheck(tokenOffset + 1, s, offset + 1);
                }
                if (tokenValue == "*")
                {
                    if (tokenOffset + 1 == tokens.Count)
                    {
                        return true;
                    }
                    for (var i = offset; i < s.Length; i++)
                    {
                        if (TryCheck(tokenOffset + 1, s, i))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (offset + tokenValue.Length > s.Length)
                {
                    return false;
                }
                for (var i = 0; i < tokenValue.Length; i++)
                {
                    if (s[offset + i] != tokenValue[i])
                    {
                        return false;
                    }
                }
                return TryCheck(tokenOffset + 1, s, offset + tokenValue.Length);
            }
            return false;
        }

        public bool Check(string s)
        {
            if (tokens.Count == 0)
            {
                return true;
            }
            return TryCheck(0, s, 0);
        }
    }
}
