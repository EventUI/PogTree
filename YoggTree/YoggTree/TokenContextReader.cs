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
        private TokenInstanceEnumerable _tokens = null;

        private IEnumerator<TokenContextInstance> _contextEnumerator = null;
        private TokenContextInstanceEnumerable _contexts = null; 
        
        private bool _enumeratedTokenLast = false;
        private bool _enumeratedContextLast = false;

        public int Position
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                int position = _tokens.CurrentLocation.Position; 
                if (position < 0) position = 0;

                return position;
            }
        }

        public int Length
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return _tokens.CurrentLocation.ContextInstance.Tokens.Count; 
            }
        }
        
        public TokenContextInstance RootContext
        {
            get { return _rootContext; }
        }

        public TokenContextInstance CurrentContext
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return _tokens.CurrentLocation.ContextInstance; 
            }
        }

        public int Depth
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return _tokens.CurrentLocation.Depth; 
            }
        }

        public TokenInstance CurrentToken
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return CurrentContext.Tokens[Position]; 
            }
        }

        public TokenContextReader(TokenContextInstance context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _rootContext = context;
            _tokens = new TokenInstanceEnumerable(context, false);
            _instanceEnumerator = _tokens.GetEnumerator();

            _contexts = new TokenContextInstanceEnumerable(context, false);
            _contextEnumerator = _contexts.GetEnumerator();
        }

        public TokenContextReader(TokenInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.Context == null) throw new ArgumentNullException("Instance's Context cannot be null.");

            int tokenIndex = TokenInstanceEnumerable.GetTokenIndex(instance);
            if (tokenIndex < 0) throw new ArgumentException("TokenInstance is not contained by its Context.");           

            _rootContext = instance.Context;
            _tokens = new TokenInstanceEnumerable(instance.Context, tokenIndex, false);
            _instanceEnumerator = _tokens.GetEnumerator();

            _contexts = new TokenContextInstanceEnumerable(instance.Context, false);
            _contextEnumerator = _contexts.GetEnumerator();

            _contexts.Seek(instance.Context);
        }

        public TokenInstance GetNextToken(bool recursive = false)
        {
            UpdateTokenInstancePosition();
            _enumeratedTokenLast = true;

            _tokens.Direction = SeekDirection.Forwards;
            _tokens.Recursive = recursive;

            if (_instanceEnumerator == null) _instanceEnumerator = _tokens.GetEnumerator();
            if (_instanceEnumerator.MoveNext() == false)
            {
                if (_instanceEnumerator.Current == null)
                {
                    _instanceEnumerator = null;
                }

                return null;
            }

            return _instanceEnumerator.Current;
        }

        public TokenInstance GetNextToken<T>(bool recursive = false) where T : ITokenDefinition
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
            UpdateTokenInstancePosition();
            _enumeratedTokenLast = true;

            _tokens.Direction = SeekDirection.Backwards;
            _tokens.Recursive = recursive;

            if (_instanceEnumerator == null) _instanceEnumerator = _tokens.GetEnumerator();
            if (_instanceEnumerator.MoveNext() == false)
            {
                _instanceEnumerator = null;
                return null;
            }

            return _instanceEnumerator.Current;
        }

        public TokenInstance GetPreviousToken<T>(bool recursive = false) where T : ITokenDefinition
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

            var iterator = new TokenContextInstanceEnumerable(_rootContext, recursive);
            iterator.Seek(CurrentContext);

            return iterator;
        }

        public IEnumerable<TokenInstance> SearchAll(Func<TokenInstance, bool> predicate, bool recursive = false)
        {
            return GetAllTokens(recursive).Where(predicate);
        }

        public IEnumerable<TokenContextInstance> SearchAll(Func<TokenContextInstance, bool> predicate, bool recursive = false)
        {
            return GetAllTokenContexts().Where(predicate);
        }

        public IEnumerable<TokenInstance> Search(Func<TokenInstance, bool> predicate, bool recursive = false)
        {
            return GetRemainingTokens(recursive).Where(predicate);
        }

        public IEnumerable<TokenContextInstance> Search(Func<TokenContextInstance, bool> predicate, bool recursive = false)
        {
            return GetRemainingTokenContexts().Where(predicate);
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            _tokens.Seek(offset, origin);

            if (_tokens.CurrentLocation.Position < 0 || _tokens.CurrentLocation.ContextInstance.Tokens.Count == 0)
            {
                _contexts.Seek(_tokens.CurrentLocation.ContextInstance);
            }
            else
            {
                _contexts.Seek(_tokens.CurrentLocation.ContextInstance.Tokens[_tokens.CurrentLocation.Position].Context);
            }            
        }

        public void Seek(ReaderSeekLocation location, bool recursive = false)
        {
            if (location == ReaderSeekLocation.FirstToken)
            {
                UpdateTokenInstancePosition();

                _tokens.SeekToBeginning(recursive);
                _enumeratedTokenLast = true;
            }
            else if (location == ReaderSeekLocation.LastToken)
            {
                UpdateTokenInstancePosition();

                _tokens.SeekToEnd(recursive);
                _enumeratedTokenLast = true;
            }
            else if (location == ReaderSeekLocation.FirstContext)
            {
                UpdateTokenContextInstancePosition();

                _contexts.SeekToBeginning(recursive);
                _enumeratedContextLast = true;
            }
            else if (location == ReaderSeekLocation.LastContext)
            {
                UpdateTokenContextInstancePosition();

                _contexts.SeekToEnd(recursive);
                _enumeratedContextLast = true;
            }
            else
            {
                return;
            }
        }

        public void Seek(TokenInstance instance)
        {
            _tokens.Seek(instance);
            _enumeratedTokenLast = true;
        }

        public void Seek(TokenContextInstance context)
        {
            _contexts.Seek(context);
            _enumeratedContextLast = true;
        }

        public TokenContextInstance GetNextContext(bool recursive = false)
        {
            UpdateTokenContextInstancePosition();
            _enumeratedContextLast = true;

            _contexts.Direction = SeekDirection.Forwards;
            _contexts.Recursive = recursive;

            if (_contextEnumerator == null) _contextEnumerator = _contexts.GetEnumerator();
            if (_contextEnumerator.MoveNext() == false)
            {                    
                _contextEnumerator = null;
                return null;
            }

            return _contextEnumerator.Current;
        }

        public TokenContextInstance GetNextContext<T>(bool recursive = false) where T : ITokenContextDefinition
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
            UpdateTokenContextInstancePosition();
            _enumeratedContextLast = true;

            _contexts.Direction = SeekDirection.Backwards;
            _contexts.Recursive = recursive;

            if (_contextEnumerator == null) _contextEnumerator = _contexts.GetEnumerator();
            if (_contextEnumerator.MoveNext() == false)
            {
                _contextEnumerator = null;
                return null;
            }

            return _contextEnumerator.Current;
        }

        public TokenContextInstance GetPreviousContext<T>(bool recursive = false) where T : ITokenContextDefinition
        {
            TokenContextInstance previousInstance = GetPreviousContext(recursive);
            while (previousInstance != null && previousInstance.Is<T>() == false)
            {
                previousInstance = GetPreviousContext(recursive);
            }

            return previousInstance;
        }

        private void UpdateTokenInstancePosition()
        {
            if (_enumeratedTokenLast == true) return;
            if (_enumeratedContextLast == false) return;

            TokenInstance referenceToken = null;
            TokenContextInstance lastContext = _contextEnumerator?.Current; //
            if (lastContext == null) lastContext = _contexts.CurrentLocation.Position < 0 ? _contexts.CurrentLocation.ContextInstance : _contexts.CurrentLocation.ContextInstance.ChildContexts[_contexts.CurrentLocation.Position];
            
            if (_contextEnumerator.Current == null)
            {
                if (_contexts.Recursive == true)
                {
                    if (_contexts.Direction == SeekDirection.Forwards)
                    {
                        referenceToken = lastContext.Tokens[lastContext.Tokens.Count - 1];
                    }
                    else
                    {
                        referenceToken = _rootContext.Tokens[0];
                    }
                }
                else
                {
                    if (_contexts.Direction == SeekDirection.Forwards)
                    {
                        referenceToken = lastContext.Tokens[lastContext.Tokens.Count - 1];
                    }
                    else
                    {
                        referenceToken = lastContext.Tokens[0];
                    }
                }
            }
            else
            {
                referenceToken = _contexts.Direction == SeekDirection.Forwards ? lastContext.Tokens[0] : lastContext.Tokens[lastContext.Tokens.Count - 1];
            }

            _tokens.Seek(referenceToken);
            _enumeratedContextLast = false;
        }

        private void UpdateTokenContextInstancePosition()
        {
            if (_enumeratedContextLast == true) return;
            if (_enumeratedTokenLast == false) return;

            int position = _tokens.CurrentLocation.Position == -1 ? 0 : _tokens.CurrentLocation.Position;
            if (_tokens.CurrentLocation.ContextInstance.Tokens.Count == 0)
            {
                _contexts.Seek(_tokens.CurrentLocation.ContextInstance);
            }
            else
            {
                TokenInstance lastToken = _tokens.CurrentLocation.ContextInstance.Tokens[position];
                _contexts.Seek(lastToken.Context);
            }
            _enumeratedTokenLast = false;
        }
    }
}
