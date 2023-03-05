/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens
{
    public delegate bool CanComeAfterPredicate<T>(TokenInstance previousToken, T definition) where T : TokenDefinition;

    public delegate bool CanComeBeforePredicate<T>(TokenInstance nextToken, T definition) where T : TokenDefinition;

    public delegate bool IsValidInstancePredicate<T>(TokenInstance instance, T definition) where T : TokenDefinition;

    public delegate bool ShouldApplyTokenPredicate<T>(T definition) where T : TokenDefinition;

    public delegate TokenContextInstance CreateTokenParseContext<T>(TokenContextInstance parentContext, TokenInstance instance, T definition) where T : TokenDefinition;
}
