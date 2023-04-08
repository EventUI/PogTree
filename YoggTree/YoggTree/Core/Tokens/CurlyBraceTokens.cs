﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "{" characters.
    /// </summary>
    public class OpenCurlyBraceToken : TokenDefinition
    {
        public OpenCurlyBraceToken()
            : base(TokenRegexStore.Brace_OpenCurly, "{", TokenDefinitionFlags.ContextStarter, "Brace_Curly")
        {
        }
    }

    /// <summary>
    /// Token for finding "}" characters.
    /// </summary>
    public class CloseCurlyBraceToken : TokenDefinition
    {
        public CloseCurlyBraceToken()
            : base(TokenRegexStore.Brace_CloseCurly, "}", TokenDefinitionFlags.ContextEnder, "Brace_Curly")
        {
        }
    }
}