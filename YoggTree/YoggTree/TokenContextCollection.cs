/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System.Collections.Concurrent;

namespace PogTree
{
    /// <summary>
    /// A container for a list of TokenContextDefinitions that can be used to swap out one TokenContextDefinition for another at runtime automatically.
    /// </summary>
    public sealed class TokenContextCollection
    {
        private ConcurrentDictionary<Type, TokenContextDefinition> _contexts = new ConcurrentDictionary<Type, TokenContextDefinition>();

        /// <summary>
        /// Whether or not the registry has any contents.
        /// </summary>
        public bool IsEmpty { get { return _contexts.IsEmpty; } }

        /// <summary>
        /// Returns if the inner dictionary contains a given type as a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key to find.</typeparam>
        /// <returns></returns>
        public bool ContainsKey<TKey>()
        {
            return ContainsKey(typeof(TKey));
        }

        /// <summary>
        /// Returns if the inner dictionary contains a given type as a key.
        /// </summary>
        /// <param name="key">The type of the key to find.</param>
        /// <returns></returns>
        public bool ContainsKey(Type key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _contexts.ContainsKey(key);
        }

        /// <summary>
        /// Adds a context to the internal dictionary of contexts with the given key. The key must be a derived type of TokenContextDefinition, and when a TokenContextDefinition is used that matches the key, the definition passed into this method will be returned instead.
        /// </summary>
        /// <typeparam name="TKey">The type of TokenContextDefinition to replace.</typeparam>
        /// <param name="contextDefinition">The TokenContextDefinition to replace the matching context with.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool AddContext<TKey>(TokenContextDefinition contextDefinition) where TKey : TokenContextDefinition
        {
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));
            return _contexts.TryAdd(typeof(TKey), contextDefinition);
        }

        /// <summary>
        /// Removes a TokenContextDefinition with the given key from the Registry.
        /// </summary>
        /// <typeparam name="TKey">The type key of the context to remove.</typeparam>
        /// <returns></returns>
        public bool RemoveContext<TKey>() where TKey : TokenContextDefinition
        {
            return _contexts.TryRemove(typeof(TKey), out _);
        }

        /// <summary>
        /// Removes a TokenContextDefinition with the given key from the Registry.
        /// </summary>
        /// <param name="key">The type key of the context to remove.</param>
        /// <returns></returns>
        public bool RemoveContext(Type key)
        {
            return _contexts.TryRemove(key, out _);
        }

        /// <summary>
        /// Gets a context definition based on a type key.
        /// </summary>
        /// <typeparam name="TKey">The type of context to get the mapped definition for.</typeparam>
        /// <returns></returns>
        public TKey GetContext<TKey>() where TKey : TokenContextDefinition
        {
            if (_contexts.TryGetValue(typeof(TKey), out var definition) == false)
            {
                return null;
            }

            return (TKey)definition;
        }

        /// <summary>
        /// Gets a context definition based on a type key.
        /// </summary>
        /// <param name="contextType">The type of context to get the mapped definition for.</param>
        /// <returns></returns>
        public TokenContextDefinition GetContext(Type contextType)
        {
            if (contextType == null) throw new ArgumentNullException(nameof(contextType));
            if (_contexts.TryGetValue(contextType, out var definition) == false)
            {
                return null;
            }

            return definition;
        }

        /// <summary>
        /// Gets the first context definition that satisfies the predicate function.
        /// </summary>
        /// <param name="predicate">A function used to select a context definition.</param>
        /// <returns></returns>
        public TokenContextDefinition GetContext(Predicate<TokenContextDefinition> predicate)
        {
            foreach (var def in _contexts.Values)
            {
                if (predicate(def) == true) return def;
            }

            return null;
        }

        /// <summary>
        /// Gets all context definitions that satisfy the predicate function.
        /// </summary>
        /// <param name="predicate">A function used to select a context definition.</param>
        /// <returns></returns>
        public List<TokenContextDefinition> GetContexts(Predicate<TokenContextDefinition> predicate)
        {
            var results = new List<TokenContextDefinition>();

            foreach (var def in _contexts.Values)
            {
                if (predicate(def) == true) results.Add(def);
            }

            return results;
        }

        /// <summary>
        /// Replaces the value associated with a key in the registry.
        /// </summary>
        /// <typeparam name="TKey">The key to replace the value of.</typeparam>
        /// <param name="other">The implementation to replace the implementation with the given key at.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ReplaceContext<TKey>(TokenContextDefinition other) where TKey : TokenContextDefinition
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            TokenContextDefinition existing = null;
            if (_contexts.TryGetValue(typeof(TKey), out existing) == false) return false;

            return _contexts.TryUpdate(typeof(TKey), existing, other);            
        }

        /// <summary>
        /// Replaces the value associated with a key in the registry.
        /// </summary>
        /// <param name="key">The key to replace the value of.</param>
        /// <param name="other">The implementation to replace the implementation with the given key at.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ReplaceContext(Type key, TokenContextDefinition other)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (other == null) throw new ArgumentNullException(nameof(other));

            TokenContextDefinition existing = null;
            if (_contexts.TryGetValue(key, out existing) == false) return false;

            return _contexts.TryUpdate(key, existing, other);
        }

        /// <summary>
        /// Creates a new TokenContextCollection.
        /// </summary>
        public TokenContextCollection()
        {

        }

        /// <summary>
        /// Creates a clone of an existing TokenContextCollection.
        /// </summary>
        /// <param name="other">The TokenContextCollection to make a clone of.</param>
        public TokenContextCollection(TokenContextCollection other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            _contexts = new ConcurrentDictionary<Type, TokenContextDefinition>(other._contexts);
        }
    }
}
