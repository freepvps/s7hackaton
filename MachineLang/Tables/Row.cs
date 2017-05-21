using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLang.Tables
{
    public class Row
    {
        private readonly Dictionary<string, string> row = new Dictionary<string, string>();
        public string this[string key]
        {
            get
            {
                if (!row.TryGetValue(key, out string value))
                {
                    return string.Empty;
                }
                return value;
            }
            set
            {
                row[key] = value;
            }
        }

        public bool Contains(string key)
        {
            return row.ContainsKey(key);
        }
        public IEnumerable<string> Keys
        {
            get
            {
                return row.Keys;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Pairs
        {
            get
            {
                foreach (var pair in row)
                {
                    yield return pair;
                }
            }
        }
    }
}
