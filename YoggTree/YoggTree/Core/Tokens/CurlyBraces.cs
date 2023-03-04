/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens
{
    public class OpenCurlyBraceToken : TokenDefinition, IBraceToken
    {
        public OpenCurlyBraceToken()
            : base(TokenRegexStore.Brace_OpenCurly, "{", TokenTypeFlags.ContextStarter, "Brace_Curly")
        {
        }
    }

    public class CloseCurlyBraceToken : TokenDefinition, IBraceToken
    {
        public CloseCurlyBraceToken()
            : base(TokenRegexStore.Brace_CloseCurly, "}", TokenTypeFlags.ContextEnder, "Brace_Curly")
        {
        }
    }
}
