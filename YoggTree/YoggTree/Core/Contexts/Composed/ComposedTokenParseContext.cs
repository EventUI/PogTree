/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.DelegateSet;
using YoggTree.Core.Tokens.Strings;

namespace YoggTree.Core.Contexts.Composed
{
    public class ComposedTokenParseContext : TokenParseContext, IComposedTokenParseContext
    {
        private DelegateItemOridinalProvider _provider = new DelegateItemOridinalProvider();
        private DelegateSetCollection<CanStartNewContextPredicate<TokenDefinition>, TokenDefinition> _canStartNewContexts = null;
        private DelegateSetCollection<EndsCurrentContextPredicate<TokenDefinition>, TokenDefinition> _endsCurrentContexts = null;
        private DelegateSetCollection<IsValidInContextPredicate<TokenDefinition>, TokenDefinition> _isValidInContexts = null;
        private DelegateSetCollection<CreateParseContextFactory<TokenDefinition>, TokenDefinition> _parseContextFactories = null;

        public ComposedTokenParseContext(TokenParseSession session, string name = null)
            : base(session, name)
        {

        }

        public ComposedTokenParseContext(TokenParseContext parent, TokenInstance start, string name = null)
            : base(parent, start, name)
        {

        }

        public IComposedTokenParseContext AddCanStartContext<TToken>(Func<TokenInstance, TToken, bool> canStart, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
        {
            if (_canStartNewContexts == null) _canStartNewContexts = new DelegateSetCollection<CanStartNewContextPredicate<TokenDefinition>, TokenDefinition>(_provider);

            if (shouldHandle != null)
            {
                _canStartNewContexts.AddHandler<TToken>(canStart, token => shouldHandle((TToken)token));
            }
            else
            {
                _canStartNewContexts.AddHandler<TToken>(canStart);
            }

            return this;
        }

        public IComposedTokenParseContext AddCreateParseContextFactory<TToken>(Func<TokenInstance, TToken, TokenParseContext> contextFactory, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
        {
            if (_parseContextFactories == null) _parseContextFactories = new DelegateSetCollection<CreateParseContextFactory<TokenDefinition>, TokenDefinition>(_provider);

            if (shouldHandle != null)
            {
                _parseContextFactories.AddHandler<TToken>(contextFactory, token => shouldHandle((TToken)token));
            }
            else
            {
                _parseContextFactories.AddHandler<TToken>(contextFactory);
            }

            return this;
        }

        public IComposedTokenParseContext AddEndsCurrentContext<TToken>(Func<TokenInstance, TToken, bool> canEnd, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
        {
            if (_endsCurrentContexts == null) _endsCurrentContexts = new DelegateSetCollection<EndsCurrentContextPredicate<TokenDefinition>, TokenDefinition>(_provider);

            if (shouldHandle != null)
            {
                _endsCurrentContexts.AddHandler<TToken>(canEnd, token => shouldHandle((TToken)token));
            }
            else
            {
                _endsCurrentContexts.AddHandler<TToken>(canEnd);
            }

            return this;
        }

        public IComposedTokenParseContext AddIsValidInContext<TToken>(Func<TokenInstance, TToken, bool> isValid, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
        {
            if (_isValidInContexts == null) _isValidInContexts = new DelegateSetCollection<IsValidInContextPredicate<TokenDefinition>, TokenDefinition>(_provider);

            if (shouldHandle != null)
            {
                _isValidInContexts.AddHandler<TToken>(isValid, token => shouldHandle((TToken)token));
            }
            else
            {
                _isValidInContexts.AddHandler<TToken>(isValid);
            }

            return this;
        }

        protected override TokenParseContext CreateNewContext(TokenInstance startToken)
        {
            if (_parseContextFactories != null)
            {
                var dele = _parseContextFactories.GetFirstDelegate(startToken.TokenDefinition);
                if (dele != null) return dele(startToken, startToken.TokenDefinition);
            }

            return base.CreateNewContext(startToken);
        }

        protected override bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (_endsCurrentContexts != null)
            {
                var dele = _endsCurrentContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return base.EndsCurrentContext(tokenInstance);
        }

        protected override bool IsValidInContext(TokenInstance token)
        {
            if (_isValidInContexts != null)
            {
                var dele = _isValidInContexts.GetFirstDelegate(token.TokenDefinition);
                if (dele != null) return dele(token, token.TokenDefinition);
            }

            return base.IsValidInContext(token);
        }

        protected override bool StartsNewContext(TokenInstance tokenInstance)
        {
            if (_canStartNewContexts != null)
            {
                var dele = _canStartNewContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return base.StartsNewContext(tokenInstance);
        }
    }
}
