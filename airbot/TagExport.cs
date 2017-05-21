using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MachineLang;

namespace airbot
{
    public static class TagExport
    {
        public static IEnumerable<LangToken> SelectTickets(this IEnumerable<LangToken> tokens) => tokens.Select("ticket");
        public static IEnumerable<LangToken> SelectSubscribe(this IEnumerable<LangToken> tokens) => tokens.Select("subscribe");
        public static IEnumerable<LangToken> SelectBagageCheck(this IEnumerable<LangToken> tokens) => tokens.Select("bagageCheck");
        public static IEnumerable<LangToken> SelectSimpleResponse(this IEnumerable<LangToken> tokens) => tokens.Select("simple_response");
        public static IEnumerable<LangToken> SelectNames(this IEnumerable<LangToken> tokens) => tokens.Select("name");
        public static IEnumerable<LangToken> SelectCities(this IEnumerable<LangToken> tokens) => tokens.Select("city");
        public static bool ShouldStartSupport(this IEnumerable<LangToken> tokens) => tokens.Select("start_support").Count() > 0;
        public static bool ShouldStopSupport(this IEnumerable<LangToken> tokens) => tokens.Select("stop_support").Count() > 0;
        public static bool? Yes(this IEnumerable<LangToken> tokens)
        {
            var yesGroup = Select(tokens, "yes");
            var yes = 0;
            var no = 0;
            foreach (var y in yesGroup)
            {
                if (y.Tags["yes"].Value == "yes")
                {
                    yes++;
                }
                else
                {
                    no++;
                }
            }

            if ((yes == 0) == (no == 0))
            {
                return null;
            }
            if (yes == 0) return false;
            if (no == 0) return true;
            return null;
        }

        public static IEnumerable<LangToken> Select(this IEnumerable<LangToken> tokens, string target)
        {
            return tokens.Where(x => x.Tags.ContainsKey(target));
        }
    }
}
