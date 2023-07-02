/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "(" characters.
    /// </summary>
    public class OpenParenthesisToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new OpenParenthesisToken.
        /// </summary>
        public OpenParenthesisToken()
            :base(TokenRegexStore.Parenthesis_Open, "(", TokenDefinitionFlags.ContextStarter, "Parenthesis")
        {

        }
    }

    /// <summary>
    /// Token for finding ")" characters.
    /// </summary>
    public class CloseParenthesisToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new CloseParenthesisToken.
        /// </summary>
        public CloseParenthesisToken()
            : base(TokenRegexStore.Parenthesis_Close, ")", TokenDefinitionFlags.ContextEnder, "Parenthesis")
        {

        }
    }
}
