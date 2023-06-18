/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Enumerators
{
    public sealed class TokenContextInstanceEnumerable : IEnumerable<TokenContextInstance>
    {
        private readonly IEnumerator<TokenInstance> _enumerator = null;

        internal TokenInstanceEnumerable TokenEnumerable { get; } = null;

        internal TokenContextInstanceEnumerable(TokenInstanceEnumerable instanceEnumerator)
        {
            TokenEnumerable = instanceEnumerator;
            _enumerator = TokenEnumerable.GetEnumerator();
        }

        internal TokenContextInstanceEnumerable(TokenContextInstance rootContext, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));

            TokenEnumerable = new TokenInstanceEnumerable(rootContext, recursive);
            _enumerator = TokenEnumerable.GetEnumerator();
        }

        internal TokenContextInstanceEnumerable(TokenContextInstance rootContext, int position, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (position > rootContext.Tokens.Count) throw new ArgumentOutOfRangeException(nameof(position));


            TokenEnumerable = new TokenInstanceEnumerable(rootContext, recursive);
            _enumerator = TokenEnumerable.GetEnumerator();
        }

        public IEnumerator<TokenContextInstance> GetEnumerator()
        {
            TokenContextInstance nextContext = GetNextContext();
            if (nextContext == null)
            {
                yield break;
            }
            else
            {
                yield return nextContext;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private TokenContextInstance GetNextContext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current == null) return null;
                if (_enumerator.Current.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
                {
                    var childContext = _enumerator.Current.GetChildContext();
                    if (childContext != null) return childContext;
                }
            }

            return null;
        }
    }
}

