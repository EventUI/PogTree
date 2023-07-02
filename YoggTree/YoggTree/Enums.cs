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
        None = 0,
        /// <summary>
        /// Context does not have an end token and can go to the end of the RootContext's Content.
        /// </summary>
        Unbounded = 1,
    }

    /// <summary>
    /// The location to where the TokenReader should seek to in the current context. If seeking recursively, this will seek towards the start/end of the entire context hierarchy.
    /// </summary>
    public enum ReaderSeekLocation
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,
        /// <summary>
        /// Seek to the first TokenInstance in the current context (or context hierarchy if seeking recursively).
        /// </summary>
        FirstToken = 1,
        /// <summary>
        /// Seek to the last TokenInstance in the current context (or context hierarchy if seeking recursively).
        /// </summary>
        LastToken = 2,
        /// <summary>
        /// Seek to the first TokenContextInstance in the current context (or context hierarchy if seeking recursively).
        /// </summary>
        FirstContext = 3,
        /// <summary>
        /// Seek to the last TokenContextInstance in the current context (or context hierarchy if seeking recursively).
        /// </summary>
        LastContext = 4
    }
}
