

using YoggTree.Core.Tokens.Interfaces;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "'" characters.
    /// </summary>
    public class StringSingleQuoteToken : TokenDefinition, IStringStartEndToken
    {
        /// <summary>
        /// Creates a new StringSingleQuoteToken.
        /// </summary>
        public StringSingleQuoteToken() 
            : base(TokenRegexStore.SingleQuote, "'", TokenDefinitionFlags.ContextEnder | TokenDefinitionFlags.ContextStarter, "StringSingleQuoteToken") 
        { 
        }
    }
}
