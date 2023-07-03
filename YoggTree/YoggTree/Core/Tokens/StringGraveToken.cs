

using PogTree.Core.Tokens.Interfaces;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace PogTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "`" characters.
    /// </summary>
    public class StringGraveToken : TokenDefinition, IStringStartEndToken
    {
        /// <summary>
        /// Creates a new StringGraveToken.
        /// </summary>
        public StringGraveToken()
            : base(TokenRegexStore.Grave, "`", TokenDefinitionFlags.ContextEnder | TokenDefinitionFlags.ContextStarter, "StringGraveToken")
        {
        }
    }
}
