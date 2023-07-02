/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "[" characters.
    /// </summary>
    public class OpenBracketToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new OpenBracketToken.
        /// </summary>
        public OpenBracketToken()
            : base(TokenRegexStore.Brace_OpenBracket, "[", TokenDefinitionFlags.ContextStarter, "Brace_Bracket")
        {
        }
    }

    /// <summary>
    /// Token for finding "]" characters.
    /// </summary>
    public class CloseBracketToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new CloseBracketToken.
        /// </summary>
        public CloseBracketToken()
            : base(TokenRegexStore.Brace_CloseBracket, "]", TokenDefinitionFlags.ContextEnder, "Brace_Bracket")
        {
        }
    }
}
