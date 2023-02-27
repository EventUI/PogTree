/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace YoggTree.Core.Interfaces
{
    /// <summary>
    /// Interface that marks TokenDefinitionBase as being a token that ends a TokenParseContext.
    /// </summary>
    public interface IContextEnder : ITokenDefinition
    {
        /// <summary>
        /// A string value that matches the ContextStartKey on TokenDefinitionBase that starts the intended TokenParseContext.
        /// </summary>
        string ContextEndKey { get; }
    }
}
