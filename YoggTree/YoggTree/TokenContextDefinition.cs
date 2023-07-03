/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/




using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace PogTree
{
    /// <summary>
    /// Represents a definition of the rules that apply to tokens found within a segment of a string (a "context").
    /// </summary>
    public abstract class TokenContextDefinition : ITokenContextDefinition
    {
        private Guid _id = Guid.NewGuid();
        private ConcurrentDictionary<Type, TokenDefinition> _validTokens = new ConcurrentDictionary<Type, TokenDefinition>();
        private ContextDefinitionFlags _flags = ContextDefinitionFlags.None;

        /// <summary>
        /// The unique ID of this ContextDefinition.
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
        public IReadOnlyCollection<TokenDefinition> ValidTokens
        {
            get
            {
                return (IReadOnlyCollection<TokenDefinition>)_validTokens.Values;
            }
        }

        /// <summary>
        /// Creates a new ContextDefinition that is blank once initialized.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
        }

        /// <summary>
        /// Creates a new ContextDefinition that is blank once initialized.
        /// </summary>
        /// <param name="name">A name to give the context.</param>
        /// <param name="flags">Flags to indicate special behavior for this context.</param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name, ContextDefinitionFlags flags)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
            _flags = flags;
        }

        /// <summary>
        /// Creates a new ContextDefinition that is blank once initialized.
        /// </summary>
        /// <param name="name">A name to give the context.</param>
        /// <param name="flags">Flags to indicate special behavior for this context.</param>
        /// <param name="tokenTypes">An IEnumerable of types representing a set of TokenDefinitions to initialize the context with.</param>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextDefinition(string name, ContextDefinitionFlags flags, IEnumerable<Type> tokenTypes)
        {
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name) + " cannot be null or whitespace.");

            Name = name;
            _flags = flags;

            AddTokens(tokenTypes);
        }

        /// <summary>
        /// Adds a token to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddToken<TToken>() where TToken : TokenDefinition, new()
        {
            var token = TokenCollection.AddToken<TToken>();
            if (token == null) throw new Exception($"Failed to get or add Token ({typeof(TToken).Name})");

            foreach (var tokenDefinition in _validTokens)
            {
                if (tokenDefinition.Value.Token.ToString() == token.Token.ToString() && tokenDefinition.Value.Token.Options == token.Token.Options)
                {
                    throw new ArgumentException("A token with the Regex of " + tokenDefinition.Value.Token.ToString() + " already exists in this context.");
                }
            }

            _validTokens.TryAdd(typeof(TToken), token);
        }

        /// <summary>
        /// Adds a token to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <param name="tokenType"></param>
        /// <exception cref="Exception"></exception>
        public void AddToken(Type tokenType)
        {
            var addedToken = TokenCollection.AddToken(tokenType);
            if (tokenType == null) throw new Exception($"Failed to get or add Token ({tokenType.Name})");

            ValidateTokenNotDuplicate(addedToken);

            _validTokens.TryAdd(tokenType, addedToken);
        }

        /// <summary>
        /// Adds an IEnumerable of Types as Tokens. Each Type must be of an object derived from TokenDefinition.
        /// </summary>
        /// <param name="tokenTypes">A list of token types to add.</param>
        public void AddTokens(IEnumerable<Type> tokenTypes)
        {
            foreach (var token in tokenTypes)
            {
                AddToken(token);
            }
        }

        /// <summary>
        /// Determines whether or not this context uses the given token definition.
        /// </summary>
        /// <typeparam name="TToken"></typeparam>
        /// <returns></returns>
        public bool HasToken<TToken>() where TToken : TokenDefinition
        {
            return HasToken(typeof(TToken));
        }

        /// <summary>
        /// Determines whether or not this context uses the given token definition.
        /// </summary>
        /// <param name="tokenDefinitionType">The type of a TokenDefinition.</param>
        /// <returns></returns>
        public bool HasToken(Type tokenDefinitionType)
        {
            return _validTokens.ContainsKey(tokenDefinitionType);
        }

        /// <summary>
        /// Determines whether or not the definition of the token instance is included in this context.
        /// </summary>
        /// <param name="token">A token instance to check HasToken with it's definition type.</param>
        /// <returns></returns>
        public bool HasToken(TokenInstance token)
        {
            if (token == null) return false;

            return HasToken(token.TokenDefinition.GetType());
        }

        /// <summary>
        /// Adds a token to this context's list of tokens to look for. Duplicate tokens will cause an exception to be thrown (pattern AND flags must match for it to be considered a duplicate).
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddToken<TToken>(Func<TToken> factory) where TToken : TokenDefinition
        {
            var token = TokenCollection.AddToken<TToken>(factory);
            if (token == null) throw new Exception($"Failed to get or add Token ({typeof(TToken).Name})");

            ValidateTokenNotDuplicate(token);

            _validTokens.TryAdd(typeof(TToken), token);
        }

        /// <summary>
        /// Removes a token definition from this context.
        /// </summary>
        public void RemoveToken<TToken>() where TToken : TokenDefinition
        {
            _validTokens.TryRemove(typeof(TToken), out _);
        }

        /// <summary>
        /// Removes a token definition from this context.
        /// </summary>
        ///<param name="tokenType">The type of token to remove.</param>
        public void RemoveToken(Type tokenType)
        {
            if (tokenType == null) throw new ArgumentNullException(nameof(tokenType));

            _validTokens.TryRemove(tokenType, out _);
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

        /// <summary>
        /// Validates that a token that is going to be added to the context is not a duplicate in terms of its Regex.
        /// </summary>
        /// <param name="token">The token to check for uniqueness</param>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateTokenNotDuplicate(TokenDefinition token)
        {
            foreach (var tokenDefinition in _validTokens)
            {
                if (tokenDefinition.Value.Token.ToString() == token.Token.ToString() && tokenDefinition.Value.Token.Options == token.Token.Options)
                {
                    throw new ArgumentException("A token with the Regex of " + tokenDefinition.Value.Token.ToString() + " already exists in this context.");
                }
            }
        }
    }
}
