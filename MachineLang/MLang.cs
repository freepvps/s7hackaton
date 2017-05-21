using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MachineLang.Helpers;
using MachineLang.TokenProcessors;
using MachineLang.Mapping;
using MachineLang.Tables;

namespace MachineLang
{
    public class MLang
    {
        public Dictionary<string, Table> Tables { get; } = new Dictionary<string, Table>();
        public Dictionary<string, ITokenProcessor> TextProcessors { get; } = new Dictionary<string, ITokenProcessor>();
        public List<MapGroup> Maps { get; } = new List<MapGroup>();

        #region LOADING
        private static MapAction LoadAction(XElement element)
        {
            var action = new MapAction();
            foreach (var attr in element.Attributes())
            {
                var name = attr.Name.LocalName;
                if (name == "h")
                {
                    action.Handlers.Add(attr.Value);
                }
                else if (name.StartsWith("p-"))
                {
                    var tag = name.Substring(2);
                    action.TagPriority[tag] = double.Parse(attr.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (name.StartsWith("t-"))
                {
                    var tag = name.Substring(2);
                    action.TagExport[tag] = attr.Value;
                }
            }
            return action;
        }
        private static ITarget LoadTarget(XElement element)
        {
            if (element.Name == "seq")
            {
                var target = new MapSequence();
                target.Action = LoadAction(element);
                foreach (var sub in element.Elements())
                {
                    var minCnt = 1;
                    var maxCnt = 1;
                    var handle = true;
                    var maximize = false;

                    foreach (var attr in sub.Attributes())
                    {
                        var name = attr.Name.LocalName;
                        if (name.StartsWith("s-"))
                        {
                            name = name.Substring(2);
                            if (name == "min") minCnt = int.Parse(attr.Value);
                            else if (name == "max") maxCnt = int.Parse(attr.Value);
                            else if (name == "handle") handle = bool.Parse(attr.Value);
                            else if (name == "maximize") maximize = bool.Parse(attr.Value);
                        }
                    }

                    var subTarget = LoadTarget(sub);

                    target.Sequence.Add(new MapSequence.SequenceToken
                    {
                        MinCount = minCnt,
                        MaxCount = maxCnt,
                        ActionHandling = handle,
                        Target = subTarget,
                        Maximize = maximize
                    });
                }
                return target;
            }
            else
            {
                var target = new MapTarget();
                target.Action = LoadAction(element);
                foreach (var attr in element.Attributes())
                {
                    var name = attr.Name.LocalName;
                    if (!name.Contains("-") && name != "h")
                    {
                        target.TagFilter[name] = StringFilter.Parse(attr.Value);
                    }
                }
                return target;
            }
            throw new Exception("Unknown target");
        }
        private void LoadMap(XElement node)
        {
            var map = new MapGroup(this);
            foreach (var nextNode in node.Elements())
            {
                var target = LoadTarget(nextNode);
                map.AddTarget(target);
            }
            Maps.Add(map);
        }
        private void LoadMaps(XElement node)
        {
            foreach (var element in node.Elements("map"))
            {
                LoadMap(element);
            }
        }
        private void LoadTable(XElement node)
        {
            var table = new Table();
            var name = node.Attribute("name").Value;

            foreach (var rowNode in node.Elements("row"))
            {
                var row = new Row();
                foreach (var attr in rowNode.Attributes())
                {
                    row[attr.Name.LocalName] = attr.Value;
                }
                table.Add(row);
            }
            Tables[name] = table;
        }
        private void LoadTables(XElement node)
        {
            foreach (var tableNode in node.Elements("table"))
            {
                LoadTable(tableNode);
            }
        }

        private KeyValuePair<string, ITokenProcessor> LoadProcessor(XElement element)
        {
            ITokenProcessor processor = null;
            var name = element.Attribute("name")?.Value ?? null;
            var target = element.Attribute("target")?.Value ?? null;

            if (string.IsNullOrWhiteSpace(name)) name = target;
            if (string.IsNullOrWhiteSpace(target)) target = name;
            
            switch (target)
            {
                case "integer-export": processor = new IntegerExport(); break;
                case "lower-case": processor = new LowerCaseProcessor(); break;
                case "ticket-export": processor = new TicketsExport(); break;
                case "table-export": processor = new TableExport(); break;
            }

            if (processor != null)
            {
                processor.MLang = this;
                processor.Init(element);
            }
            return new KeyValuePair<string, ITokenProcessor>(name, processor);
        }

        public static MLang Load(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var xml = XDocument.Load(streamReader);
                    return Load(xml);
                }
            }
        }
        public static MLang Load(XDocument xml)
        {
            return Load(xml.Elements().First());
        }
        public static MLang Load(XElement node)
        {
            var mlang = new MLang();
            foreach (var tableNode in node.Elements("tables"))
            {
                mlang.LoadTables(tableNode);
            }
            foreach (var element in node.Elements("mapping"))
            {
                mlang.LoadMaps(element);
            }
            foreach (var element in node.Elements("processing"))
            {
                foreach (var nextElement in element.Elements("register"))
                {
                    var processor = mlang.LoadProcessor(nextElement);
                    if (processor.Key != null && processor.Value != null)
                    {
                        mlang.TextProcessors[processor.Key] = processor.Value;
                    }
                }
            }

            return mlang;
        }

        #endregion
        
        public IEnumerable<LangToken> ProcessTokens(IEnumerable<LangToken> tokens)
        {
            foreach (var map in Maps)
            {
                var next = map.ProcessTokens(tokens).ToArray();
                tokens = next;
            }
            return tokens;
        }
        public IEnumerable<LangToken> ProcessTokens(string text)
        {
            return ProcessTokens(ParseTokens(text).ToArray());
        }
        public IEnumerable<LangToken> ParseTokens(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var cls = CharClassify.ClassifyChar(text[i]);
                if (cls > 0)
                {
                    var j = i;
                    for (; j < text.Length && (CharClassify.ClassifyChar(text[j]) & cls) > 0; j++) ;

                    var word = text.Substring(i, j - i);

                    var result = new LangToken
                    {
                        Value = word,
                        LeftOffset = i,
                        RightOffset = j - 1
                    };
                    yield return result;

                    i = j - 1;
                }
            }
        }
    }
}
