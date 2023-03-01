/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Contexts.Composed
{
    public interface IComposedTokenParseContext : ITokenParseContext
    {
        IComposedTokenParseContext AddCanStartContext<TToken>(Func<TokenInstance, TToken, bool> canStart, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition;

        IComposedTokenParseContext AddEndsCurrentContext<TToken>(Func<TokenInstance, TToken, bool> canEnd, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition;

        IComposedTokenParseContext AddIsValidInContext<TToken>(Func<TokenInstance, TToken, bool> isValid, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition;

        IComposedTokenParseContext AddCreateParseContextFactory<TToken>(Func<TokenInstance, TToken, TokenParseContext> contextFactory, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition;
    }
}
