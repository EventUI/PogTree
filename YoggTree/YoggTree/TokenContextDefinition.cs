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
    /// <summary>
    /// Represents a definition of the rules that apply to tokens found within a segment of a string (a "context").
    /// </summary>
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

        /// <summary>
        /// Creates a new TokenContextDefinition that is initialized with a set of tokens to look for.
        /// </summary>
        /// <param name="name">A name to give the context.</param>
        /// <param name="validTokens">A set of TokenDefinitions to look for in this context.</param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name, IEnumerable<TokenDefinition> validTokens)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");
            Name = name;
            
            _validTokensRO = _validTokens.AsReadOnly();

            if (validTokens != null)
            {
                AddTokens(validTokens);
            }
        }

        /// <summary>
        /// Creates a new TokenContextDefinition that is blank once initialized.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
            _validTokensRO = _validTokens.AsReadOnly();
        }

        /// <summary>
        /// Adds a token to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <param name="token">The token definition to add.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Adds a set of tokens to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <param name="tokens">The token definitions to add.</param>
        /// <exception cref="ArgumentException"></exception>
        protected void AddTokens(IEnumerable<TokenDefinition> tokens)
        {
            Dictionary<string, TokenDefinition> allRegexes = new Dictionary<string, TokenDefinition>();
            foreach (var token in ValidTokens)
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
                allRegexes.Add(token.Token.ToString(), token);
            }
        }

        /// <summary>
        /// Removes a token definition from this context.
        /// </summary>
        /// <param name="token">The token definition to remove.</param>
        protected void RemoveToken(TokenDefinition token)
        {
            if (_validTokens.Contains(token) == true)
            {
                _validTokens.Remove(token);
            }
        }

        /// <summary>
        /// Removes a token definition from this context based on its ID.
        /// </summary>
        /// <param name="tokenID">The ID of the token definition to remove.</param>
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

        /// <summary>
        /// Removes a token definition from this context based on the matching of the token definition's Regex pattern and flags.
        /// </summary>
        /// <param name="regex">The regext to find and remove from this context.</param>
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

        /// <summary>
        /// Creates a new (child) context of the TokenContextInstance that is using this definition as its rule set.
        /// </summary>
        /// <param name="startToken">The token that is triggering the creation of a new TokenContextInstance.</param>
        /// <returns></returns>
        public TokenContextInstance CreateNewContext(TokenInstance startToken)
        {
            if (_parseContextFactories != null)
            {
                var dele = _parseContextFactories.GetFirstDelegate(startToken.TokenDefinition);
                if (dele != null) return dele(startToken, startToken.TokenDefinition);
            }

            return HandleCreateNewContext(startToken);
        }

        /// <summary>
        /// Determines whether or not a token instance should start a new TokenContextInstance.
        /// </summary>
        /// <param name="tokenInstance">The token instance to check.</param>
        /// <returns></returns>
        public bool StartsNewContext(TokenInstance tokenInstance)
        {
            if (_canStartNewContexts != null)
            {
                var dele = _canStartNewContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return HandleStartsNewContext(tokenInstance);
        }

        /// <summary>
        /// Determines whether or not a token instance marks the end of the current TokenContextInstance.
        /// </summary>
        /// <param name="tokenInstance">The token instance to check.</param>
        /// <returns></returns>
        public bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (_endsCurrentContexts != null)
            {
                var dele = _endsCurrentContexts.GetFirstDelegate(tokenInstance.TokenDefinition);
                if (dele != null) return dele(tokenInstance, tokenInstance.TokenDefinition);
            }

            return HandleEndsCurrentContext(tokenInstance);
        }

        /// <summary>
        /// Determines whether or not a token is valid in this context at all. Invalid tokens are ignored.
        /// </summary>
        /// <param name="token">The token instance to check.</param>
        /// <returns></returns>
        public bool IsValidInContext(TokenInstance token)
        {
            if (_isValidInContexts != null)
            {
                var dele = _isValidInContexts.GetFirstDelegate(token.TokenDefinition);
                if (dele != null) return dele(token, token.TokenDefinition);
            }

            return HandleIsValidInContext(token);
        }

        /// <summary>
        /// Adds a handler for a specific type of token definition to determine if it should start a new context or not. Handlers are fired in order of addition, with the first match being the only one executed.
        /// </summary>
        /// <typeparam name="TToken">The type derived from TokenDefinition</typeparam>
        /// <param name="canStart">A predicate function that takes the current token instance and the type definition of the token and returns a boolean.</param>
        /// <param name="shouldHandle">A predicate function that decides whether or not this handler applies to the given token instance at all.</param>
        protected void AddCanStartContext<TToken>(Func<TokenInstance, TToken, bool> canStart, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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

        protected void AddCreateParseContextFactory<TToken>(Func<TokenInstance, TToken, TokenContextInstance> contextFactory, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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

        protected void AddEndsCurrentContext<TToken>(Func<TokenInstance, TToken, bool> canEnd, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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

        protected void AddIsValidInContext<TToken>(Func<TokenInstance, TToken, bool> isValid, Func<TToken, bool> shouldHandle = null) where TToken : TokenDefinition
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
            if (tokenInstance.Context.StartToken == null) return false;

            if (tokenInstance.TokenDefinition.Flags.HasFlag(TokenTypeFlags.ContextEnder) == true)
            {
                if (tokenInstance.TokenDefinition.ContextKey == tokenInstance.Context.StartToken?.TokenDefinition.ContextKey)
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

        protected virtual TokenContextInstance HandleCreateNewContext(TokenInstance tokenInstance)
        {
            var tokenContext = tokenInstance.TokenDefinition.CreateContext(tokenInstance);
            if (tokenContext != null) return tokenContext;

            return new TokenContextInstance(tokenInstance.Context.TokenContextDefinition, tokenInstance.Context, tokenInstance);
        }
    }
}
