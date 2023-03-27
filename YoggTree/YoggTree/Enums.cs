/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree
{
    /// <summary>
    /// Bit flags for indicating special behavior for TokenDefinitions.
    /// </summary>
    [Flags]
    public enum TokenDefinitionFlags
    {
        /// <summary>
        /// None. None.
        /// </summary>
        None = 0,
        /// <summary>
        /// Token marks the start of a new child TokenContextInstance.
        /// </summary>
        ContextStarter = 1,
        /// <summary>
        /// Token marks the end of a TokenContextInstance.
        /// </summary>
        ContextEnder = 2
    }

    public enum TokenInstanceType
    {
        None = 0,
        RegexResult = 1,
        TextPlaceholder = 2,
        ContextPlaceholder = 3,
        BoundingPlaceholder = 4
    }

    public enum ContextDefinitionFlags
    {
        None = 0
    }

    [Flags]
    public enum TokenSeekFlags
    {
        None = 0,
        IgnoreTextPlaceholders = 1,
        IgnoreContextPlaceholders = 2,
        IgnorePlaceholders = IgnoreTextPlaceholders | IgnoreContextPlaceholders,
    }
}
