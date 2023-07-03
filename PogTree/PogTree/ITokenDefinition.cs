/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree
{
    /// <summary>
    /// Interface extracted from the TokenDefinition class - this interface is intended to be inherited by other interfaces to create "marker" interfaces to flag TokenDefinition-derived types with rather than serve as the foundation for a new type that does not inherit from TokenDefinition.
    /// </summary>
    public interface ITokenDefinition
    {
        /// <summary>
        /// In the event that this token has a ContextStarter and/or a ContextEnder flag, this is the key used by the TokenContextInstance used to determine if the start token and end token of a context match.
        /// </summary>
        string ContextKey { get; }

        /// <summary>
        /// Flags indicating special behavior to be taken when this token is encountered.
        /// </summary>
        TokenDefinitionFlags Flags { get; }

        /// <summary>
        /// The unique ID of this token definition.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// The human-readable name of the token.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Flags indicating special behavior to be taken when this token is encountered.
        /// </summary>
        int SpoolSize { get; }

        /// <summary>
        /// The regular expression used to find the token in the string.
        /// </summary>
        Regex Token { get; }
    }
}