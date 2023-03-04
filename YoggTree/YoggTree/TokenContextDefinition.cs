/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Contexts;
using YoggTree.Core.DelegateSet;
using YoggTree.Core.Interfaces;
using YoggTree.Core.Tokens;

namespace YoggTree
{
    public abstract class TokenContextDefinition
    {
        private Guid _id = Guid.NewGuid();
        private IReadOnlyList<TokenDefinition> _validTokensRO = null;
        private List<TokenDefinition> _validTokens = new List<TokenDefinition>();

        private DelegateItemOridinalProvider _provider = new DelegateItemOridinalProvider();
        private DelegateSetCollection<CanStartNewContextPredicate<TokenDefinition>, TokenDefinition> _canStartNewContexts = null;
        private DelegateSetCollection<EndsCurrentContextPredicate<TokenDefinition>, TokenDefinition> _endsCurrentContexts = null;
        private DelegateSetCollection<IsValidInContextPredicate<TokenDefinition>, TokenDefinition> _isValidInContexts = null;
        private DelegateSetCollection<CreateParseContextFactory<TokenDefinition>, TokenDefinition> _parseContextFactories = null;

        /// <summary>
        /// The unique ID of this Context.
        /// </summary>
        public Guid ID { get { return _id; } }

        /// <summary>
        /// The human-readable name describing this context.
        /// </summary>
        public string Name { get; } = null;

        /// <summary>
        /// All of the token definitions that should be processed in this context.
        /// </summary>
        public IReadOnlyList<TokenDefinition> ValidTokens { get { return _validTokensRO; } }

        public TokenContextDefinition(string name, IEnumerable<TokenDefinition> validTokens)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");
            if (validTokens != null)
            {
                AddTokens(validTokens);
            }

            Name = name;
            _validTokensRO = _validTokens.AsReadOnly();
        }

        public TokenContextDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
        }

        protected void AddToken(TokenDefinition token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            foreach (var tokenDefinition in _validTokens)
            {
                if (tokenDefinition.Token.ToString() == token.Token.ToString() && tokenDefinition.Token.Options == token.Token.Options)
                {
                    throw new ArgumentException("A token with the Regex of " + tokenDefinition.Token.ToString() + " already exists in this context.");
                }
            }

            _validTokens.Add(token);
        }

        protected void AddTokens(IEnumerable<TokenDefinition> tokens)
        {
            Dictionary<string, TokenDefinition> allRegexes = new Dictionary<string, TokenDefinition>();
            foreach (var token in tokens)
            {
                allRegexes.Add(token.Token.ToString(), token);
            }

            foreach (var token in tokens)
            {
                if (token == null) continue;
                if (allRegexes.TryGetValue(token.Token.ToString(), out TokenDefinition match) == true)
                {
                    if (match.Token.Options == token.Token.Options)
                    {
                        throw new ArgumentException("A token with the Regex of " + token.Token.ToString() + " already exists in this context.");
                    }
                }

                _validTokens.Add(token);
            }
        }

        protected void RemoveToken(TokenDefinition token)
        {
            if (_validTokens.Contains(token) == true)
            {
                _validTokens.Remove(token);
            }
        }

        protected void RemoveToken(Guid tokenID)
        {
            for (int x = 0; x < _validTokens.Count; x++)
            {
                var curToken = _validTokens[x];
                if (curToken.ID == tokenID)
                {
                    _validTokens.RemoveAt(x);
                    break;
                }
            }
        }

        protected void RemoveToken(Regex regex)
        {
            for (int x = 0; x < _validTokens.Count; x++)
            {
                var curToken = _validTokens[x];
                if (curToken.Token.ToString() == regex.ToString() && curToken.Token.Options == regex.Options)
                {
                    _validTokens.RemoveAt(x);
                    break;
                }
            }
        }

        public TokenContextInstance CreateNewContext(TokenInstance startToken)
        {
            if (_parseContextFactories != null)
            {
                var dele = _parseContextFactories.GetFirstDelegate(startToken.TokenDefinition);
                if (dele != null) return dele(startToken, startToken.TokenDefinition);
            }

            return HandleCreateNewContext(startToken);
        }

        public bool StartsNewContext(TokenInstance tokenInstance)
        {
            if (_canStartNewContexts != null)
            {
                var dele = _canStartNewContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return HandleStartsNewContext(tokenInstance);
        }

        public bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (_endsCurrentContexts != null)
            {
                var dele = _endsCurrentContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return HandleEndsCurrentContext(tokenInstance);
        }

        public bool IsValidInContext(TokenInstance token)
        {
            if (_isValidInContexts != null)
            {
                var dele = _isValidInContexts.GetFirstDelegate(token.TokenDefinition);
                if (dele != null) return dele(token, token.TokenDefinition);
            }

            return HandleIsValidInContext(token);
        }

        public TokenContextInstance HandleCreateNewContext(TokenInstance tokenInstance)
        {
            var tokenContext = tokenInstance.TokenDefinition.CreateContext(tokenInstance);
            if (tokenContext != null) return tokenContext;

            return new TokenContextInstance(tokenInstance.Context.TokenContextDefinition, tokenInstance.Context, tokenInstance);
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

        protected virtual bool HandleEndsCurrentContext(TokenInstance tokenInstance)
        {
            if (tokenInstance.StartIndex == 0) return false;
            if (tokenInstance.TokenDefinition.Flags.HasFlag(TokenTypeFlags.ContextEnder) == true)
            {
                if (tokenInstance.TokenDefinition.ContextKey == tokenInstance.Context.StartToken.TokenDefinition.ContextKey)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool HandleIsValidInContext(TokenInstance token)
        {
            return true;
        }

        protected virtual bool HandleStartsNewContext(TokenInstance tokenInstance)
        {
            if (tokenInstance.TokenDefinition.Flags.HasFlag(TokenTypeFlags.ContextStarter)) return true;
            return false;
        }
    }
}
