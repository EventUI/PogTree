/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.DelegateSet
{
    internal class DelegateSetCollection<T, TPredicate> where T : Delegate
    {
        private DelegateItemOridinalProvider _ordinal = null;
        private SortedList<int, DelegateSetItem<T, TPredicate>> _delegateItems = new SortedList<int, DelegateSetItem<T, TPredicate>>();
        private Dictionary<Type, TypeMatchResultCacheItem> _resultsCache = new Dictionary<Type, TypeMatchResultCacheItem>();
        private HashSet<Type> _misses = new HashSet<Type>();    

        internal DelegateSetCollection(DelegateItemOridinalProvider ordinal)
        {
            if (ordinal == null) throw new ArgumentNullException(nameof(ordinal));
            _ordinal = ordinal;
        }

        public void AddHandler<TConstraint>(Delegate handler, Func<TPredicate, bool> predicate = null)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var delegateItem = new DelegateSetItem<T, TPredicate>()
            {
                Ordinal = _ordinal.GetNextOrdinal(),
                Delegate = (T)handler,
                Predicate = predicate,
                TypeConstraint = typeof(T)
            };

            _delegateItems.Add(delegateItem.Ordinal, delegateItem);

            ResetCache();
        }

        public T GetFirstDelegate(TPredicate target)
        {
            if (target == null) return null; 

            Type targetType = target.GetType();
            if (_misses.Contains(targetType)) return null;

            TypeMatchResultCacheItem cacheCollection = null;
            if (_resultsCache.TryGetValue(targetType, out cacheCollection) == true)
            {
                foreach (var value in cacheCollection.Results)
                {
                    if (value.Value.Predicate != null)
                    {
                        if (value.Value.Predicate(target) == true)
                        {
                            return value.Value.Delegate;
                        }
                    }
                    else
                    {
                        return value.Value.Delegate;
                    }
                }

                if (cacheCollection.ScannedWholeList == true) return null;
            }

            foreach (var delegateItem in _delegateItems.Values) 
            {
                bool match = false;

                if (delegateItem.TypeConstraint == targetType)
                {
                    match = true;
                }
                else if (delegateItem.TypeConstraint.IsClass || delegateItem.TypeConstraint.IsValueType)
                {
                    if (targetType.IsSubclassOf(delegateItem.TypeConstraint) == true)
                    {
                        match = true;
                    }                   
                }
                else if (delegateItem.TypeConstraint.IsInterface)
                {
                    if (targetType.GetInterface(delegateItem.TypeConstraint.FullName) != null)
                    {
                        match = true;
                    }
                }

                if (match == true)
                {
                    if (cacheCollection == null)
                    {
                        cacheCollection = new TypeMatchResultCacheItem()
                        { 
                            MatchedType = targetType                            
                        };

                        cacheCollection.Results.Add(delegateItem.Ordinal, delegateItem);
                        _resultsCache.Add(targetType, cacheCollection);
                    }

                    if (delegateItem.Predicate != null)
                    {
                        if (delegateItem.Predicate(target) == true)
                        {
                            return delegateItem.Delegate;
                        }
                    }
                    else
                    {
                        return delegateItem.Delegate;
                    }
                }
            }

            if (cacheCollection != null)
            {
                cacheCollection.ScannedWholeList = true;
            }
            else
            {
                _misses.Add(targetType);
            }

            return null;
        }

        private void ResetCache()
        {
            if (_misses.Count > 0) _misses.Clear();

            foreach (var cachedResultSet in _resultsCache)
            {
                cachedResultSet.Value.ScannedWholeList = false;
            }
        }

        private class TypeMatchResultCacheItem
        {
            public bool ScannedWholeList { get; set; } = false;

            public Type MatchedType { get; set; }

            public SortedList<int, DelegateSetItem<T, TPredicate>> Results { get; } = new SortedList<int, DelegateSetItem<T, TPredicate>>();
        }
    }
}
