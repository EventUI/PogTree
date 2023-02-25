using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;

namespace YoggTree.Tokens.Basic
{
    public class BasicToken : TokenDefinitionBase
    {
        public BasicToken(Regex token, string name)
            : base(token, name)
        {
        }
    }
}
