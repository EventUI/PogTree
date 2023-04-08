/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree
{
    /// <summary>
    /// Represents the registry of tokens that are available to be used by the TokenParser.
    /// </summary>
    public static class TokenCollection
    {
        private static readonly ConcurrentDictionary<Type, TokenDefinition> _tokens = new ConcurrentDictionary<Type, TokenDefinition>();
        
        /// <summary>
        /// A read-only collection of all the tokens that have been added to the TokenCollection so far.
        /// </summary>
        public static ReadOnlyCollection<TokenDefinition> Tokens { get { return (ReadOnlyCollection<TokenDefinition>)_tokens.Values; } }

        /// <summary>
        /// Adds a token to the token collection. Must have a parameterless constructor.
        /// </summary>
        /// <typeparam name="TToken"></typeparam>
        /// <returns></returns>
        public static TokenDefinition AddToken<TToken>() where TToken : TokenDefinition, new()
        {
            Type genericTypeParam = typeof(TToken);
            if (_tokens.TryGetValue(genericTypeParam, out TokenDefinition tokenDefinition) == false)
            {
                TokenDefinition def = new TToken();
                return GetOrAddToken(genericTypeParam, def);
            }
            else
            {
                return tokenDefinition;
            }
        }

        /// <summary>
        /// Gets or adds a token to the token collection based on the result of a factory function.
        /// </summary>
        /// <typeparam name="TToken"></typeparam>
        /// <param name="factory">A function that will return a token of type TToken.</param>
        /// <returns></returns>
        public static TokenDefinition AddToken<TToken>(Func<TToken> factory) where TToken : TokenDefinition
        {
            Type genericTypeParam = typeof(TToken);
            if (_tokens.TryGetValue(genericTypeParam, out TokenDefinition tokenDefinition) == false)
            {
                TokenDefinition def = factory();
                return GetOrAddToken(genericTypeParam, def);
            }
            else
            {
                return tokenDefinition;
            }
        }

        /// <summary>
        /// Gets or adds a token based on a Type object. The Type object must represent a type that derives from TokenDefinition and has a parameterless constructor.
        /// </summary>
        /// <param name="tokenType">The type of token to add.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static TokenDefinition AddToken(Type tokenType)
        {
            if (tokenType == null) throw new ArgumentNullException(nameof(tokenType));
            if (tokenType.IsSubclassOf(typeof(TokenDefinition)) == false)
            {
                throw new ArgumentException(nameof(tokenType) + " must derive from TokenDefinition.");
            }

             if (_tokens.TryGetValue(tokenType, out TokenDefinition tokenDefinition) == false)
            {
                TokenDefinition def = (TokenDefinition)Activator.CreateInstance(tokenType);
                return GetOrAddToken(tokenType, def);
            }
            else
            {             
                return tokenDefinition;
            }
        }

        /// <summary>
        /// Either adds the token to the internal tokens list or returns the token with the matching type key.
        /// </summary>
        /// <param name="genericTypeParam"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        private static TokenDefinition GetOrAddToken(Type genericTypeParam, TokenDefinition def)
        {
            var added = _tokens.TryAdd(genericTypeParam, def);
            if (added == true)
            {
                return def;
            }
            else
            {
                _tokens.TryGetValue(genericTypeParam, out TokenDefinition tokenDefinition);
                {
                    return tokenDefinition;
                }
            }
        }

        /// <summary>
        /// Gets a token from the TokenCollection based on its Type.
        /// </summary>
        /// <typeparam name="TToken"></typeparam>
        /// <returns></returns>
        public static TokenDefinition GetToken<TToken>() where TToken : TokenDefinition 
        {
            return GetToken(typeof(TToken));
        }

        /// <summary>
        /// Gets a token from the TokenCollection based on its type.
        /// </summary>
        /// <param name="tokenType">The type of token definition to get.</param>
        /// <returns></returns>
        public static TokenDefinition GetToken(Type tokenType)
        {
            if (_tokens.TryGetValue(tokenType, out TokenDefinition token) == true)
            {
                return token;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Internal-only method for removing tokens from the collection.
        /// </summary>
        /// <typeparam name="TToken"></typeparam>
        /// <returns></returns>
        internal static bool RemoveToken<TToken>()
        {
            var tokenType = typeof(TToken);
            return _tokens.TryRemove(tokenType, out _);
        }
    }
}
