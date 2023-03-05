/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Contexts
{
    public delegate bool CanStartNewContextPredicate<T>(TokenInstance instance, T definition) where T : TokenDefinition;

    public delegate bool EndsCurrentContextPredicate<T>(TokenInstance instance, T definition) where T : TokenDefinition;

    public delegate bool IsValidInContextPredicate<T>(TokenInstance instance, T definition) where T : TokenDefinition;

    public delegate TokenContextInstance CreateParseContextFactory<T>(TokenInstance instance, T definition) where T : TokenDefinition;
}
