using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree
{
    public class TokenContextRegistry
    {
        internal bool IsEmpty { get { return _contexts.IsEmpty; } }

        protected ConcurrentDictionary<Type, TokenContextDefinition> _contexts = new ConcurrentDictionary<Type, TokenContextDefinition>();

        public bool AddContext(TokenContextDefinition contextDefinition)
        {
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));
            return _contexts.TryAdd(contextDefinition.GetType(), contextDefinition);
        }

        public bool RemoveContext<TKey>() where TKey : TokenContextDefinition
        {
            return _contexts.TryRemove(typeof(TKey), out _);
        }

        public TKey GetContext<TKey>() where TKey : TokenContextDefinition
        {
            if (_contexts.TryGetValue(typeof(TKey), out var definition) == false)
            {
                return null;
            }

            return (TKey)definition;
        }

        public TokenContextDefinition GetContext(Predicate<TokenContextDefinition> predicate)
        {
            foreach (var def in _contexts.Values)
            {
                if (predicate(def) == true) return def;
            }

            return null;
        }

        public List<TokenContextDefinition> GetContextw(Predicate<TokenContextDefinition> predicate)
        {
            var results = new List<TokenContextDefinition>();

            foreach (var def in _contexts.Values)
            {
                if (predicate(def) == true) results.Add(def);
            }

            return results;
        }

        public bool ReplaceContext<TKey>(TokenContextDefinition contextDefinition) where TKey : TokenContextDefinition
        {
            return ReplaceContext<TKey>(contextDefinition, false);
        }

        public bool ReplaceContext<TKey>(TokenContextDefinition contextDefinition, bool ignoreInheritcanceMismatch) where TKey : TokenContextDefinition
        {
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));

            TokenContextDefinition existing = null;
            if (_contexts.TryGetValue(typeof(TKey), out existing) == false) return false;

            if (ignoreInheritcanceMismatch == true)
            {
                return _contexts.TryUpdate(typeof(TKey), existing, contextDefinition);
            }
            else
            {
                Type newDefType = contextDefinition.GetType();
                Type oldDefType = existing.GetType();

                if (newDefType != oldDefType && newDefType.IsSubclassOf(oldDefType) == false)
                {
                    throw new Exception($"Inheritance mismatch: Type {newDefType.FullName} cannot replace Type {oldDefType.FullName} because they are not part of the same inheritance hierarchy.");
                }

                return _contexts.TryUpdate(typeof(TKey), existing, contextDefinition);
            }
        }

        public TokenContextRegistry()
        {

        }

        public TokenContextRegistry(TokenContextRegistry other)
        {
            _contexts = new ConcurrentDictionary<Type, TokenContextDefinition>(_contexts);
        }
    }
}
