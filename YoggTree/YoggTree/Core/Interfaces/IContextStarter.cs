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
    /// Interface meant to be applied to a TokenDefinitionBase indicating that it can start a new sub-context to the current context.
    /// </summary>
    public interface IContextStarter : ITokenDefinition
    {
        /// <summary>
        /// A string value that matches the ContextEndKey on TokenDefinitionBase that starts the intended TokenParseContext.
        /// </summary>
        string ContextStartKey { get; }
    }
}
