/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree
{
    /// <summary>
    /// Bit flags for indicating special behavior for TokenDefinitions.
    /// </summary>
    public enum TokenTypeFlags
    {
        /// <summary>
        /// None. Default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Token marks the start of a new child TokenContextInstance.
        /// </summary>
        ContextStarter = 1,
        /// <summary>
        /// Token marks the end of a TokenContextInstance.
        /// </summary>
        ContextEnder = 2
    }
}
