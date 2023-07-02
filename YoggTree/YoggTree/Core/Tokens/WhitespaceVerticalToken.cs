

using YoggTree.Core.Tokens.Interfaces;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding all "vertical" (line breaking) whitespace characters that are not separated by a non-line breaking whitespace character.
    /// </summary>
    public class WhitespaceVerticalToken : TokenDefinition, IWhitespaceToken
    {
        /// <summary>
        /// Mkaes a new WhitepsaceVerticalToken.
        /// </summary>
        public WhitespaceVerticalToken() 
            : base(TokenRegexStore.Whitespace_Vertical, "<vertical whitespace>", 100)
        {
        }
    }
}
