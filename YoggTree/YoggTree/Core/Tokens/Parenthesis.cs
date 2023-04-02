/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "(" characters.
    /// </summary>
    public class Parenthesis_OpenToken : TokenDefinition
    {
        public Parenthesis_OpenToken()
            :base(TokenRegexStore.Parenthesis_Open, "(", TokenDefinitionFlags.ContextStarter, "Parenthesis")
        {

        }
    }

    /// <summary>
    /// Token for finding ")" characters.
    /// </summary>
    public class Parenthesis_CloseToken : TokenDefinition
    {
        public Parenthesis_CloseToken()
            : base(TokenRegexStore.Parenthesis_Close, ")", TokenDefinitionFlags.ContextEnder, "Parenthesis")
        {

        }
    }
}
