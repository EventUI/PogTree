/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogTree.Core.Enumerators
{
    /// <summary>
    /// Holds the position and context one of the token/context enumerables is currently at. 
    /// </summary>
    internal class TokenContextInstanceLocation
    {
        /// <summary>
        /// The context that the enumerable is currently inside of.
        /// </summary>
        public TokenContextInstance ContextInstance { get; set; } = null;

        /// <summary>
        /// The position in the enumerable - for tokens this is the index of the token in the Tokens list, for contexts this is the index of the context in the ChildContext's list.
        /// </summary>
        public int Position { get; set; } = 0;

        /// <summary>
        /// How many layers deep this context is from the root.
        /// </summary>
        public int Depth { get; set; } = 0;
    }
}
