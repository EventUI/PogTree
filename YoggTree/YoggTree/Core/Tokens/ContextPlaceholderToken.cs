/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Represents a definition for a token that is a placeholder for a TokenContextInstance.
    /// </summary>
    public sealed class ContextPlaceholderToken : TokenDefinition
    {
        /// <summary>
        /// Global instance of the ContextPlaceholderToken.
        /// </summary>
        public static ContextPlaceholderToken Instance { get; } = new ContextPlaceholderToken();

        /// <summary>
        /// Creates a new ContextPlaceholderToken
        /// </summary>
        public ContextPlaceholderToken()
            :base(new Regex(""), "<context placeholder>")
        {

        }

        /// <summary>
        /// Gets a string that identifies this token as ContextPlaceholderToken.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}-{Name})";
        }
    }
}
