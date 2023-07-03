/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "\" characters.
    /// </summary>
    public class BackslashToken : TokenDefinition
    {
        /// <summary>
        /// Makes a new BackslashToken.
        /// </summary>
        public BackslashToken() 
            : base(TokenRegexStore.Backslash, "\\")
        {
        }
    }
}
