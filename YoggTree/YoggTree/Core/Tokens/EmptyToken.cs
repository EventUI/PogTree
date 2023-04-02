/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Represents a token that is an empty placeholder for a TokenDefinition in certain TokenInstances.
    /// </summary>
    public class EmptyToken : TokenDefinition
    {
        public EmptyToken()
            :base(new Regex(""), "<empty>")
        {

        }
    }
}
