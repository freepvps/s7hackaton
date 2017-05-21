using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MachineLang.TokenProcessors
{
    public interface ITokenProcessor
    {
        MLang MLang { get; set; }
        string Name { get; }
        void Init(XElement node);
        IEnumerable<LangToken> Process(IEnumerable<LangToken> tokens);
    }
}
