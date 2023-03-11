/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding all "horizontal" (non-line breaking) whitespace characters that are not separated by a non-whitespace character.
    /// </summary>
    public class WhitespaceHorizontal : TokenDefinition
    {
        public WhitespaceHorizontal() 
            : base(TokenRegexStore.Whitespace_Horizontal, "<horizontal whitespace>", 100)
        {
        }
    }
}
