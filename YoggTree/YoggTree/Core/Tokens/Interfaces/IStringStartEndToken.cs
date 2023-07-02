/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Tokens.Interfaces
{
    /// <summary>
    /// Represents a token that marks the start or end of a string literal.
    /// </summary>
    public interface IStringStartEndToken : ITokenDefinition
    {
    }
}
