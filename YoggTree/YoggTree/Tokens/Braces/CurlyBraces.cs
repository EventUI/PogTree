using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Braces
{
    public class OpenCurlyBraceToken : TokenDefinitionBase, IBraceToken, IContextStarter
    {
        public string ContextStartKey { get; } = "Brace_Curly";

        public OpenCurlyBraceToken() 
            : base(TokenRegexStore.Brace_OpenCurly, "{")
        {
        }
    }

    public class CloseCurlyBraceToken : TokenDefinitionBase, IBraceToken, IContextEnder
    {
        public string ContextEndKey { get; } = "Brace_Curly";

        public CloseCurlyBraceToken() 
            : base(TokenRegexStore.Brace_CloseCurly, "}")
        {
        }
    }
}
