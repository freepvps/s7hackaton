using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.TextGeneration
{
    public class UserTimeParser
    {
        public static bool TryParseDatetime(string row, out Tuple<DateTime, DateTime> interval)
        {
            var tokens = new Queue<string>(GetTokens(row));

            var ok = false;
            interval = null;
            DateTime left = DateTime.Now;
            DateTime right = left;
            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();

                var br = false;
                switch (token)
                {
                    case "завтра":
                        left = left.AddDays(1).AddHours(-left.Hour).AddMinutes(-left.Minute);
                        right = left.AddDays(1);
                        ok = true;
                        continue;
                    case "послезавтра":
                        left = left.AddDays(2).AddHours(-left.Hour).AddMinutes(-left.Minute);
                        right = left.AddDays(1);
                        ok = true;
                        continue;

                    case "вчера":
                        left = left.AddDays(-1).AddHours(-left.Hour).AddMinutes(-left.Minute);
                        right = left.AddDays(1);
                        ok = true;
                        continue;
                    case "позавчера":
                        left = left.AddDays(-2).AddHours(-left.Hour).AddMinutes(-left.Minute);
                        right = left.AddDays(1);
                        ok = true;
                        continue;

                    case "сегодня":
                        left = left.AddDays(0).AddHours(-left.Hour).AddMinutes(-left.Minute);
                        right = left.AddHours(24);
                        ok = true;
                        continue;
                    case "утром":
                        left = left.AddDays(0).AddHours(-left.Hour).AddMinutes(-left.Minute).AddHours(7);
                        right = left.AddHours(6);
                        ok = true;
                        continue;
                    case "днем":
                        left = left.AddDays(0).AddHours(-left.Hour).AddMinutes(-left.Minute).AddHours(12);
                        right = left.AddHours(6);
                        ok = true;
                        continue;
                    case "ночью":
                        left = left.AddDays(0).AddHours(-left.Hour).AddMinutes(-left.Minute).AddHours(24);
                        right = left.AddHours(6);
                        ok = true;
                        continue;

                    case "в": left = left.AddHours(-left.Hour).AddMinutes(-left.Minute); right = left.AddDays(-2); br = true; break;
                    case "через": right = left.AddHours(24); br = true; break;
                }
                if (br)
                {
                    break;
                }
            }

            if (left >= right) right = left.AddHours(24);

            if (TryParseTime(tokens, out int sec))
            {
                left = left.AddSeconds(sec);
                right = right.AddSeconds(sec);
                ok = true;
            }
            interval = Tuple.Create(left, right);
            return ok;
        }
        private static int GetTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }
            switch (token)
            {
                case "полчаса": return 30 * 60;
                case "полминуты": return 30;
            }
            var set = new HashSet<string>
            {
                "сек", "мин", "час", "дн", "нед", "ден"
            };
            var ok = false;
            foreach (var x in set)
            {
                if (token.StartsWith(x))
                {
                    ok = true; break;
                }
            }
            if (!ok) return 0;
            // с, м, ч, д
            // секунда
            // минута
            // час
            // день
            var ch = token[0];
            switch (ch)
            {
                case 'с': return 1;
                case 'м': return 60;
                case 'ч': return 60 * 60;
                case 'д': return 60 * 60 * 24;
                case 'н': return 60 * 60 * 24 * 7;
            }
            return 0;
        }

        public static IEnumerable<string> GetTokens(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    var j = i;
                    for (; j < text.Length && char.IsLetterOrDigit(text[j]); j++) ;

                    var word = text.Substring(i, j - i);

                    yield return word.ToLower();

                    i = j - 1;
                }
            }
        }
        public static bool TryParseTime(string s, out int time, int defaultTime = 60)
        {
            var tokens = new Queue<string>(GetTokens(s));
            return TryParseTime(tokens, out time, defaultTime);
        }
        public static bool TryParseTime(Queue<string> tokens, out int time, int defaultTime = 60)
        {
            var ok = false;
            var ans = 0;
            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                var tmp = 0;
                if (int.TryParse(token, out tmp))
                {
                    var mod = defaultTime;
                    if (tokens.Count > 0)
                    {
                        mod = GetTime(tokens.Peek());
                        if (mod != 0)
                        {
                            tokens.Dequeue();
                        }
                        else
                        {
                            mod = defaultTime;
                        }
                    }
                    ans += tmp * mod;
                    ok = true;
                }
                else
                {
                    ans += GetTime(token);
                    ok = true;
                }
            }
            time = ans;
            return ok;
        }
        public static string SecondsToString(int seconds)
        {
            var d = new Dictionary<string, int>
            {
                { "сек.", 60 },
                { "мин.", 60 },
                { "ч.", 60 },
                { "дн.", 24 },
                { "нед.", 7 }
            };

            var ans = string.Empty;
            foreach (var x in d)
            {
                var t = seconds % x.Value;
                if (x.Value == 7) t = seconds;

                seconds /= x.Value;

                if (t != 0)
                {
                    if (!string.IsNullOrWhiteSpace(ans)) ans = " " + ans;
                    ans = string.Format("{0} {1}", t, x.Key) + ans;
                }
            }
            if (string.IsNullOrWhiteSpace(ans)) return "0 сек";
            return ans;
        }
    }
}
