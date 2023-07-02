/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/




namespace YoggTree
{
    /// <summary>
    /// Interface extracted from the TokenContextDefinition class - this interface is intended to be inherited by other interfaces to create "marker" interfaces to flag TokenContextDefinition-derived types with rather than serve as the foundation for a new type that does not inherit from TokenContextDefinition.
    /// </summary>
    public interface ITokenContextDefinition
    {
        /// <summary>
        /// Flags indicating special behavior to be taken when encountering this context definition.
        /// </summary>
        ContextDefinitionFlags Flags { get; }
        /// <summary>
        /// The unique ID of this ContextDefinition.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// The human-readable name describing this context.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// All of the token definitions that should be processed in this context.
        /// </summary>
        IReadOnlyCollection<TokenDefinition> ValidTokens { get; }
    }
}