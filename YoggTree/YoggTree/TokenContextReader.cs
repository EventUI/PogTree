/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoggTree.Core.Enumerators;

namespace YoggTree
{
    public sealed class TokenContextReader
    {
        private TokenContextInstance _rootContext = null;
        private IEnumerator<TokenInstance> _instanceEnumerator = null;
        private TokenInstanceEnumerable _instanceEnumerable = null;

        public int Position
        {
            get 
            { 
                int position = _instanceEnumerable.CurrentLocation.Position; 
                if (position < 0) { return 0; };

                return position;
            }
        }

        public int Length
        {
            get { return _instanceEnumerable.CurrentLocation.ContextInstance.Tokens.Count; }
        }
        
        public TokenContextInstance RootContext
        {
            get { return _rootContext; }
        }

        public TokenContextInstance CurrentContext
        {
            get { return _instanceEnumerable.CurrentLocation.ContextInstance; }
        }

        public int Depth
        {
            get { return _instanceEnumerable.CurrentLocation.Depth; }
        }

        public TokenInstance CurrentToken
        {
            get { return CurrentContext.Tokens[Position]; }
        }

        public TokenContextReader(TokenContextInstance context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _rootContext = context;
            _instanceEnumerable = new TokenInstanceEnumerable(context, false);
            _instanceEnumerator = _instanceEnumerable.GetEnumerator();

            _contextEnumerable = new TokenContextInstanceEnumerable(_instanceEnumerable);
            _contextEnumerator = _contextEnumerable.GetEnumerator();
        }

        public TokenContextReader(TokenInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.Context == null) throw new ArgumentNullException("Instance's Context cannot be null.");

            int tokenIndex = TokenInstanceEnumerable.GetTokenIndex(instance);
            if (tokenIndex < 0) throw new ArgumentException("TokenInstance is not contained by its Context.");           

            _rootContext = instance.Context;
            _instanceEnumerable = new TokenInstanceEnumerable(instance.Context, tokenIndex, false);
            _instanceEnumerator = _instanceEnumerable.GetEnumerator();

            _contextEnumerable = new TokenContextInstanceEnumerable(_instanceEnumerable);
            _contextEnumerator = _contextEnumerable.GetEnumerator();
        }

        public TokenInstance GetNextToken(bool recursive = false)
        {
            _instanceEnumerable.Direction = SeekDirection.Forwards;
            _instanceEnumerable.Recursive = recursive;

            if (_instanceEnumerator.MoveNext() == false)
            {
                _instanceEnumerator.Reset();
                return null;
            }
            return _instanceEnumerator.Current;
        }

        public TokenInstance GetNextToken<T>(bool recursive = false) where T : TokenDefinition
        {
            TokenInstance nextToken = GetNextToken(recursive);
            while (nextToken != null && nextToken.Is<T>() == false)
            {
                nextToken = GetNextToken(recursive);
            }

            return nextToken;
        }

        public TokenInstance GetPreviousToken(bool recursive = false)
        {
            _instanceEnumerable.Direction = SeekDirection.Backwards;
            _instanceEnumerable.Recursive = recursive;

            if (_instanceEnumerator.MoveNext() == false)
            {
                _instanceEnumerator.Reset();
                return null;
            }

            return _instanceEnumerator.Current;
        }

        public TokenInstance GetPreviousToken<T>(bool recursive = false) where T : TokenDefinition
        {
            TokenInstance previousToken = GetPreviousToken(recursive);
            while (previousToken != null && previousToken.Is<T>() == false)
            {
                previousToken = GetPreviousToken(recursive);
            }

            return previousToken;
        }

        public IEnumerable<TokenInstance> GetAllTokens(bool recursive = false)
        {
            return new TokenInstanceEnumerable(_rootContext, recursive);
        }

        public IEnumerable<TokenInstance> GetRemainingTokens(bool recursive = false)
        {
            var iterator = new TokenInstanceEnumerable(_rootContext, recursive);
            iterator.Seek(CurrentToken);

            return iterator;
        }

        public IEnumerable<TokenContextInstance> GetAllTokenContexts(bool recursive = false)
        {
            return new TokenContextInstanceEnumerable(_rootContext, recursive);
        }

        public IEnumerable<TokenContextInstance> GetRemainingTokenContexts(bool recursive = false)
        {
            var iterator = new TokenInstanceEnumerable(_rootContext, recursive);
            iterator.Seek(CurrentToken);

            return new TokenContextInstanceEnumerable(iterator);
        }

        public IEnumerable<TokenInstance> Search(Func<TokenInstance, bool> predicate, bool recursive = false)
        {
            return new TokenInstanceEnumerable(CurrentContext, Position, recursive).Where(predicate);
        }

        public IEnumerable<TokenContextInstance> Search(Func<TokenContextInstance, bool> predicate, bool recursive = false)
        {
            return new TokenContextInstanceEnumerable(CurrentContext, Position, recursive).Where(predicate);
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            _instanceEnumerable.Seek(offset, origin);
        }

        public void Seek(TokenInstance instance)
        {
            _instanceEnumerable.Seek(instance);
        }

        public void Seek(TokenContextInstance context)
        {
            _instanceEnumerable.Seek(context);
        }

        public TokenContextInstance GetNextContext(bool recursive = false)
        {
            
        }

        public TokenContextInstance GetNextContext<T>(bool recursive = false) where T : TokenContextDefinition
        {
            TokenContextInstance nextContext = GetNextContext(recursive);
            while (nextContext != null && nextContext.Is<T>() == false)
            {
                nextContext = GetNextContext(recursive);
            }

            return nextContext;
        }

        public TokenContextInstance GetPreviousContext(bool recursive = false)
        {
            
        }

        public TokenContextInstance GetPreviousContext<T>(bool recursive = false) where T : TokenContextDefinition
        {
            TokenContextInstance previousInstance = GetPreviousContext(recursive);
            while (previousInstance != null && previousInstance.Is<T>() == false)
            {
                previousInstance = GetPreviousContext(recursive);
            }

            return previousInstance;
        }
    }
}
