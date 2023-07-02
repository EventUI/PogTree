/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "/" characters.
    /// </summary>
    public class ForwardSlashToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new ForwardSlashToken.
        /// </summary>
        public ForwardSlashToken() 
            : base(TokenRegexStore.Forwardslash, "/")
        {
        }
    }
}
