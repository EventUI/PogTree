/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens
{
    public class OpenBracketToken : TokenDefinition, IBraceToken
    {
        public OpenBracketToken()
            : base(TokenRegexStore.Brace_OpenBracket, "[", TokenTypeFlags.ContextStarter, "Brace_Bracket")
        {
        }
    }

    public class CloseBracketToken : TokenDefinition, IBraceToken
    {
        public CloseBracketToken()
            : base(TokenRegexStore.Brace_CloseBracket, "]", TokenTypeFlags.ContextEnder, "Brace_Bracket")
        {
        }
    }
}
