/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

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
        private ContextDefinitionFlags _flags = ContextDefinitionFlags.None;

        /// <summary>
        /// The unique ID of this Context.
        /// </summary>
        public Guid ID { get { return _id; } }

        /// <summary>
        /// The human-readable name describing this context.
        /// </summary>
        public string Name { get; } = null;

        /// <summary>
        /// Flags indicating special behavior to be taken when encountering this context definition.
        /// </summary>
        public ContextDefinitionFlags Flags { get { return _flags; } }

        /// <summary>
        /// All of the token definitions that should be processed in this context.
        /// </summary>
        public IReadOnlyList<TokenDefinition> ValidTokens { get { return _validTokensRO; } }

        /// <summary>
        /// Creates a new ContextDefinition that is blank once initialized.
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
        /// Creates a new ContextDefinition that is blank once initialized.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name, ContextDefinitionFlags flags)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
            _validTokensRO = _validTokens.AsReadOnly();
            _flags = flags;
        }


        /// <summary>
        /// Creates a new ContextDefinition that is initialized with a set of tokens to look for.
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
        /// Creates a new ContextDefinition that is initialized with a set of tokens to look for.
        /// </summary>
        /// <param name="name">A name to give the context.</param>
        /// <param name="validTokens">A set of TokenDefinitions to look for in this context.</param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name, ContextDefinitionFlags flags, IEnumerable<TokenDefinition> validTokens)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");
            Name = name;

            _validTokensRO = _validTokens.AsReadOnly();
            _flags = flags;

            if (validTokens != null)
            {
                AddTokens(validTokens);
            }
        }

        /// <summary>
        /// Adds a token to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <param name="token">The token definition to add.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddToken(TokenDefinition token)
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
        public void AddTokens(IEnumerable<TokenDefinition> tokens)
        {
            Dictionary<string, TokenDefinition> allRegexes = new Dictionary<string, TokenDefinition>();
            foreach (var token in ValidTokens)
            {
                allRegexes.Add($"\"{token.Token.ToString()}\"::\"{(int)token.Token.Options}", token);
            }

            foreach (var token in tokens)
            {
                if (token == null) continue;
                string tokenKey = $"\"{token.Token.ToString()}\"::\"{(int)token.Token.Options}";

                if (allRegexes.TryGetValue(tokenKey, out TokenDefinition match) == true)
                {
                    if (match.Token.Options == token.Token.Options)
                    {
                        throw new ArgumentException("A token with the Regex of " + token.Token.ToString() + " already exists in this context.");
                    }
                }

                _validTokens.Add(token);
                allRegexes.Add(tokenKey, token);
            }
        }

        /// <summary>
        /// Removes a token definition from this context.
        /// </summary>
        /// <param name="token">The token definition to remove.</param>
        public void RemoveToken(TokenDefinition token)
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
        public void RemoveToken(Guid tokenID)
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
        /// <param name="regex">The Regex to find and remove from this context.</param>
        public void RemoveToken(Regex regex)
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
        internal TokenContextInstance CreateNewContext(TokenInstance startToken)
        {
            //first see if the token gives us a definition or not when its flagged itself as starting a new context
            var newContext = startToken.TokenDefinition.GetNewContextDefinition(startToken);
            if (newContext == null) throw new Exception($"Token {startToken.ToString()} failed to return a TokenContextDefinition.");

            //if the registry's been populated, see if we have a replacement context type to use instead of the one returned by the token.
            if (startToken.Context.ParseSession.ContextRegistry.IsEmpty == false)
            {
                Type newContextType = newContext.GetType();

                var replacementContext = startToken.Context.ParseSession.ContextRegistry.GetContext(newContextType);
                if (replacementContext != null && replacementContext.GetType() != newContextType) //if the replacement was found and its of a different type, use the different type of context. Otherwise use the one the token provided.
                {
                    newContext = replacementContext;
                }
            }

            return new TokenContextInstance(newContext, startToken.Context, startToken);
        }

        /// <summary>
        /// Determines whether or not a token instance should start a new TokenContextInstance.
        /// </summary>
        /// <param name="tokenInstance">The token instance to check.</param>
        /// <returns></returns>
        public virtual bool StartsNewContext(TokenInstance tokenInstance)
        {
            if (tokenInstance.TokenDefinition.Flags.HasFlag(TokenDefinitionFlags.ContextStarter)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether or not a token instance marks the end of the current TokenContextInstance.
        /// </summary>
        /// <param name="tokenInstance">The token instance to check.</param>
        /// <returns></returns>
        public virtual bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (tokenInstance.StartIndex == 0) return false;
            if (tokenInstance.Context.StartToken == null) return false;

            if (tokenInstance.TokenDefinition.Flags.HasFlag(TokenDefinitionFlags.ContextEnder) == true)
            {
                if (tokenInstance.TokenDefinition.ContextKey == tokenInstance.Context.StartToken?.TokenDefinition.ContextKey)
                {
                    return true;
                }
            }

            return false;
        }       
    }
}
