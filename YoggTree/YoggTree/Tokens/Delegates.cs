/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens
{
    public delegate bool CanComeAfterPredicate(TokenInstance previousToken, TokenParseContextBase context);

    public delegate bool CanComeBeforePredicate(TokenInstance nextToken, TokenParseContextBase context);

    public delegate bool IsValidInstancePredicate(TokenInstance instance, TokenParseContextBase context);

    public delegate bool CanComeAfterPredicate<T>(TokenInstance previousToken, T definition, TokenParseContextBase context) where T : ITokenDefinition;

    public delegate bool CanComeBeforePredicate<T>(TokenInstance nextToken, T definition, TokenParseContextBase context) where T : ITokenDefinition;

    public delegate bool IsValidInstancePredicate<T>(TokenInstance instance, T definition, TokenParseContextBase context) where T : ITokenDefinition;
}
