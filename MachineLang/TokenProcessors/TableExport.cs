using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MachineLang.TokenProcessors
{
    public class TableExport : ITokenProcessor
    {
        public string Name => "table-export";

        public MLang MLang { get; set; }

        public Dictionary<string, Dictionary<string, string>> ModifiedTable { get; } = new Dictionary<string, Dictionary<string, string>>();

        public void Init(XElement node)
        {
            var tableName = node.Attribute("table").Value;
            foreach (var row in MLang.Tables[tableName])
            {
                if (!ModifiedTable.TryGetValue(row["value"], out Dictionary<string, string> dict))
                {
                    ModifiedTable[row["value"]] = dict = new Dictionary<string, string>();
                }

                foreach (var key in row.Keys)
                {
                    dict[key] = row[key];
                }
            }
        }

        public IEnumerable<LangToken> Process(IEnumerable<LangToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (ModifiedTable.TryGetValue(token.Value, out Dictionary<string, string> dict))
                {
                    foreach (var key in dict)
                    {
                        if (key.Key == "value")
                        {
                            continue;
                        }
                        var tag = new TokenTag
                        {
                            Key = key.Key,
                            Value = key.Value
                        };
                        token.Tags[tag.Key] = tag;
                        tag.AddParent(token);
                    }
                    yield return token;
                }
            }
        }
    }
}
