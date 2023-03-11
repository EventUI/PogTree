﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding " characters.
    /// </summary>
    public class StringDoubleQuote : TokenDefinition
    {
        public StringDoubleQuote() 
            : base(TokenRegexStore.DoubleQuote, "\"", TokenTypeFlags.ContextEnder | TokenTypeFlags.ContextStarter, "StringDoubleQuote") 
        { 
        }
    }
}
