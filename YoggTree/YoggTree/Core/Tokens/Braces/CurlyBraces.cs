/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens.Braces
{
    public class OpenCurlyBraceToken : TokenDefinition, IBraceToken, IContextStarter
    {
        public string ContextStartKey { get; } = "Brace_Curly";

        public OpenCurlyBraceToken() 
            : base(TokenRegexStore.Brace_OpenCurly, "{")
        {
        }
    }

    public class CloseCurlyBraceToken : TokenDefinition, IBraceToken, IContextEnder
    {
        public string ContextEndKey { get; } = "Brace_Curly";

        public CloseCurlyBraceToken() 
            : base(TokenRegexStore.Brace_CloseCurly, "}")
        {
        }
    }
}
