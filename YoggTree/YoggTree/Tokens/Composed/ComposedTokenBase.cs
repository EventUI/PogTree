﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.DelegateSet;
using YoggTree.Core.Interfaces;
using YoggTree.Tokens.Basic;

namespace YoggTree.Tokens.Composed
{
    public class ComposedTokenBase : TokenDefinition, IComposedToken
    {
        private DelegateItemOridinalProvider _counterProvider = new DelegateItemOridinalProvider();
        private DelegateSetCollection<CanComeAfterPredicate<TokenDefinition>, TokenDefinition> _canComeAfters = null;
        private DelegateSetCollection<CanComeBeforePredicate<TokenDefinition>, TokenDefinition> _canComeBefores = null;
        private DelegateSetCollection<IsValidInstancePredicate<TokenDefinition>, TokenDefinition> _isValidInstances = null;

        internal ComposedTokenBase(Regex token, string name) 
            : base(token, name)
        {

        }

        public IComposedToken AddCheckCanComeAfter<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeAfterDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef: TokenDefinition
        {
            if (_canComeAfters == null) _canComeAfters = new DelegateSetCollection<CanComeAfterPredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _canComeAfters.AddHandler<TTokenDef>(comeAfterDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _canComeAfters.AddHandler<TTokenDef>(comeAfterDele);
            }

            return this;
        }

        public IComposedToken AddCheckCanComeBefore<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeBeforeDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_canComeBefores == null) _canComeBefores = new DelegateSetCollection<CanComeBeforePredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _canComeBefores.AddHandler<TTokenDef>(comeBeforeDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _canComeBefores.AddHandler<TTokenDef>(comeBeforeDele);
            }

            return this;
        }

        public IComposedToken AddCheckIsValidTokenInstance<TTokenDef>(Func<TokenInstance, TTokenDef, bool> isValidDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_isValidInstances == null) _isValidInstances = new DelegateSetCollection<IsValidInstancePredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _isValidInstances.AddHandler<TTokenDef>(isValidDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _isValidInstances.AddHandler<TTokenDef>(isValidDele);
            }

            return this;
        }

        public override bool CanComeAfter(TokenInstance previousToken)
        {
            if (_canComeAfters != null)
            {
                var dele = _canComeAfters.GetFirstDelegate(previousToken.TokenDefinition);
                if (dele != null) return dele(previousToken, previousToken.TokenDefinition);                
            }

            return base.CanComeAfter(previousToken);
        }

        public override bool CanComeBefore(TokenInstance nextToken)
        {
            if (_canComeBefores != null)
            {
                var dele = _canComeBefores.GetFirstDelegate(nextToken.TokenDefinition);
                if (dele != null) return dele(nextToken, nextToken.TokenDefinition);
            }

            return base.CanComeBefore(nextToken);
        }

        public override bool IsValidInstance(TokenInstance instance)
        {
            if (_isValidInstances != null)
            {
                var dele = _isValidInstances.GetFirstDelegate(instance.TokenDefinition);
                if (dele != null) return dele(instance, instance.TokenDefinition);
            }

            return base.IsValidInstance(instance);
        }
    }
}
