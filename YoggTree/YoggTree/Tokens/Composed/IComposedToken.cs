/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.DelegateSet;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Composed
{
    public interface IComposedToken : ITokenDefinition
    {
        IComposedToken AddCheckCanComeAfter<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeAfterDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition;
        IComposedToken AddCheckCanComeBefore<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeAfterDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition;
        IComposedToken AddCheckIsValidTokenInstance<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeBeforeDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition;
    }
}
