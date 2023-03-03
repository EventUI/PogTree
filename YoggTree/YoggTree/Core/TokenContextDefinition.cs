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
using YoggTree.Core.Interfaces;
using YoggTree.Core.Tokens;

namespace YoggTree.Core
{
    public class TokenContextDefinition
    {
        internal bool InUse { get; set; } = false;

        private Guid _id = Guid.NewGuid();
        private IReadOnlyList<TokenDefinition> _validTokensRO = null;
        private List<TokenDefinition> _validTokens = new List<TokenDefinition>();

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

        public TokenContextDefinition(string name, TokenListBuilder validTokensBuilder)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");
            if (validTokensBuilder != null)
            {
                AddTokens(validTokensBuilder.GetTokens());
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

        /// <summary>
        /// Creates a new context based on the type of token being passed in.
        /// </summary>
        /// <param name="startToken">The token that will be the StartToken of the new </param>
        /// <returns></returns>
        public virtual TokenContextInstance CreateNewContext(TokenInstance startToken)
        {
            return new TokenContextInstance(this, startToken.Context, startToken);
        }


        /// <summary>
        /// Overridable filter function to tell the parser to ignore certain tokens. By default, no tokens are ignored.
        /// </summary>
        /// <param name="token">The token to possibly ignore.</param>
        /// <returns></returns>
        public virtual bool IsValidInContext(TokenInstance token)
        {
            return true;
        }

        /// <summary>
        /// Determines if a token starts a new context. By default, only IContextStarter-implementing TokenDefinitions can start new contexts.
        /// </summary>
        /// <param name="tokenInstance">The token instance that could start a new context.</param>
        /// <returns></returns>
        public virtual bool StartsNewContext(TokenInstance tokenInstance)
        {
            return false;
        }

        /// <summary>
        /// Determines if a token ends the current context. By default, only IContextEnder-implementing TokenDefifitions that matches the StartToken can end the current context.
        /// </summary>
        /// <param name="tokenInstance">The token that could end the current context.</param>
        /// <returns></returns>
        public virtual bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            return false;
        }
    }
}
