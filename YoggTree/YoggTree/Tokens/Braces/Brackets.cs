/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;
using YoggTree.Core;

namespace YoggTree.Tokens.Braces
{
    public class OpenBracketToken : TokenDefinitionBase, IBraceToken, IContextStarter
    {
        public string ContextStartKey { get; } = "Brace_Bracket";

        public OpenBracketToken()
            : base(TokenRegexStore.Brace_OpenBracket, "[")
        {
        }
    }

    public class CloseBracketToken : TokenDefinitionBase, IBraceToken, IContextEnder
    {
        public string ContextEndKey { get; } = "Brace_Bracket";

        public CloseBracketToken()
            : base(TokenRegexStore.Brace_CloseBracket, "]")
        {
        }
    }
}
