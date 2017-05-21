using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLang.Mapping
{
    public interface ITarget
    {
        MapAction Action { get; set; }
        FilterResult SelectGroup(IEnumerable<LangToken> tokens, bool skipPrefix);
    }
}
