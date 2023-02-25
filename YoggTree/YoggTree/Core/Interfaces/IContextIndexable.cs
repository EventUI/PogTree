/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Interfaces
{
    /// <summary>
    /// Interface for marking something as being able to find the index of something within its context in a different context.
    /// </summary>
    public interface IContextIndexable
    {
        /// <summary>
        /// Gets the index of the local index in a different contextual parent.
        /// </summary>
        /// <param name="localIndex">The local index to find in an parent context.</param>
        /// <param name="targetParent">The target parent to find the index in.</param>
        /// <returns></returns>
        public int GetContextIndex(int localIndex, TokenParseContextBase targetParent);

        /// <summary>
        /// Gets the absolute index of a local index in the root Content of the parse session.
        /// </summary>
        /// <param name="localIndex">The local index to get the absolute index of.</param>
        /// <param name="session">The parent parse session of the index.</param>
        /// <returns></returns>
        public int GetAbsoluteIndex(int localIndex, TokenParseSession session);
    }
}
