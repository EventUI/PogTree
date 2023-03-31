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
        /// Default. None.
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

    /// <summary>
    /// Determines what type of token an instance represents.
    /// </summary>
    public enum TokenInstanceType
    {
        /// <summary>
        /// Default. Not set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Token instance is a Regex result found in the source Content.
        /// </summary>
        RegexResult = 1,

        /// <summary>
        /// Token instance is a placeholder that contains the span of text between two consecutive (but not directly adjacent) tokens of other TokenInstanceTypes.
        /// </summary>
        TextPlaceholder = 2,

        /// <summary>
        /// Token instance is a wrapper that contains the complete contents (and a reference to) to a child TokenContextInstance of the token instance's TokenContextInstance.
        /// </summary>
        ContextPlaceholder = 3
    }

    /// <summary>
    /// Bit flags for indicating special behavior for TokenContextDefinitions.
    /// </summary>
    public enum ContextDefinitionFlags
    {
        /// <summary>
        /// Default. Not set.
        /// </summary>
        None = 0
    }
}
