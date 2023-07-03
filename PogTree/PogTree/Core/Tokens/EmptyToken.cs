/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree.Core.Tokens
{
    /// <summary>
    /// Represents a token that is an empty placeholder for a TokenDefinition in certain TokenInstances.
    /// </summary>
    public sealed class EmptyToken : TokenDefinition
    {
        /// <summary>
        /// Singleton instance of the EmptyToken.
        /// </summary>
        public static EmptyToken Instance { get; } = new EmptyToken();

        /// <summary>
        /// Creates a new EmptyToken.
        /// </summary>
        public EmptyToken()
            :base(new Regex(""), "<empty>")
        {

        }
    }
}
