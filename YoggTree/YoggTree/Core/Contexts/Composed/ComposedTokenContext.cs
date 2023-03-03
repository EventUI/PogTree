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
using YoggTree.Core.Tokens;
using YoggTree.Core.Tokens.Strings;

namespace YoggTree.Core.Contexts.Composed
{
    public class ComposedTokenContext : TokenContextDefinition
    {
        private DelegateItemOridinalProvider _provider = new DelegateItemOridinalProvider();
        private DelegateSetCollection<CanStartNewContextPredicate<TokenDefinition>, TokenDefinition> _canStartNewContexts = null;
        private DelegateSetCollection<EndsCurrentContextPredicate<TokenDefinition>, TokenDefinition> _endsCurrentContexts = null;
        private DelegateSetCollection<IsValidInContextPredicate<TokenDefinition>, TokenDefinition> _isValidInContexts = null;
        private DelegateSetCollection<CreateParseContextFactory<TokenDefinition>, TokenDefinition> _parseContextFactories = null;

        public ComposedTokenContext(string name, IEnumerable<TokenDefinition> validTokens) 
            : base(name, validTokens)
        {
        }

        public ComposedTokenContext(string name, TokenListBuilder validTokenBuilder) 
            : base(name, validTokenBuilder)
        {
        }

        protected internal void AddCanStartContext<TToken>(Func<TokenInstance, TToken, bool> canStart, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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
        }

        protected internal void AddCreateParseContextFactory<TToken>(Func<TokenInstance, TToken, TokenContextInstance> contextFactory, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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
        }

        protected internal void AddEndsCurrentContext<TToken>(Func<TokenInstance, TToken, bool> canEnd, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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
        }

        protected internal void AddIsValidInContext<TToken>(Func<TokenInstance, TToken, bool> isValid, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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
        }

        public override TokenContextInstance CreateNewContext(TokenInstance startToken)
        {
            if (_parseContextFactories != null)
            {
                var dele = _parseContextFactories.GetFirstDelegate(startToken.TokenDefinition);
                if (dele != null) return dele(startToken, startToken.TokenDefinition);
            }

            return base.CreateNewContext(startToken);
        }

        public override bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (_endsCurrentContexts != null)
            {
                var dele = _endsCurrentContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return base.EndsCurrentContext(tokenInstance);
        }

        public override bool IsValidInContext(TokenInstance token)
        {
            if (_isValidInContexts != null)
            {
                var dele = _isValidInContexts.GetFirstDelegate(token.TokenDefinition);
                if (dele != null) return dele(token, token.TokenDefinition);
            }

            return base.IsValidInContext(token);
        }

        public override bool StartsNewContext(TokenInstance tokenInstance)
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
