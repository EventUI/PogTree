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
    public static class TokenCollection
    {
        private static readonly ConcurrentDictionary<Type, TokenDefinition> _tokens = new ConcurrentDictionary<Type, TokenDefinition>();
        
        public static ReadOnlyCollection<TokenDefinition> Tokens { get { return (ReadOnlyCollection<TokenDefinition>)_tokens.Values; } }

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

        public static TokenDefinition GetToken<TToken>() where TToken : TokenDefinition 
        {
            return GetToken(typeof(TToken));
        }

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

        internal static bool RemoveToken<TToken>()
        {
            var tokenType = typeof(TToken);
            return _tokens.TryRemove(tokenType, out _);
        }
    }
}
