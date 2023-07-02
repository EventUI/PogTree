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
    /// <summary>
    /// Utility class for searching through a TokenContextInstance hierarchy. It is intended to be used on a TokenContextInstance that has been completed, using it on an unfinished parse operation could result in odd behavior.
    /// </summary>
    public sealed class TokenReader
    {
        /*Note: This class is intended to seek backwards and forwards in both the Tokens and ChildContexts arrays of a single TokenContextInstance, but each required it's own IEnumerable implementation.
        So we sync them lazily when the consumer of the API needs a value that came from whatever source they did not use last. For example, if the tokens list is advanced,
        and the user uses the CurrentContext property, we need to advance the context position to point at whatever context the token currently resides in. This could cause performance problems and 
        might merit a refactoring to keep both in sync in a more performant fashion.
        
        It is also important to note that the recursive behavior of contexts and token is different: 

        Tokens which are Placeholders are not included in recursive walking of the hierarchy for tokens - they are drilled into instead. The result of reading the tokens backwards is exactly the reverse of reading them forwards.
            This is because token placeholders are not "real" tokens that have actual content associated with them, but instead they are wrappers for other tokens - recursion does NOT include the wrapper in this case.
        
        Contexts follow a different pattern in recursive mode where the parent context comes first, then its children. This is also true for reverse recursive walking of the hierarchy, so the result of reading the contexts backwards is NOT the same as reading forwards.
            This is because token instances are all containers for other instances, so in order to walk the hierarchy without missing a context, they must all be returned in order of occurrence.*/


        private TokenContextInstance _rootContext = null;

        private IEnumerator<TokenInstance> _instanceEnumerator = null;
        private TokenInstanceEnumerable _tokens = null;

        private IEnumerator<TokenContextInstance> _contextEnumerator = null;
        private TokenContextInstanceEnumerable _contexts = null; 
        
        private bool _enumeratedTokenLast = false;
        private bool _enumeratedContextLast = false;

        /// <summary>
        /// The current index of the CurrentContext in its Tokens list.
        /// </summary>
        public int TokenPosition
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                int position = _tokens.CurrentLocation.Position; 
                if (position < 0) position = 0;

                return position;
            }
        }

        /// <summary>
        /// The total number of tokens in the CurrentContext in its Tokens list.
        /// </summary>
        public int TokenLength
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return _tokens.CurrentLocation.ContextInstance.Tokens.Count; 
            }
        }

        /// <summary>
        /// The current index of the CurrentContext in its ChildContexts list.
        /// </summary>
        public int ContextPosition
        {
            get
            {
                if (_enumeratedTokenLast == true) UpdateTokenContextInstancePosition();
                int position = _contexts.CurrentLocation.Position;
                if (position < 0) position = 0;

                return position;
            }
        }

        /// <summary>
        /// The total number of TokenContextInstances in the CurrentContext in its Tokens list.
        /// </summary>
        public int ContextLength
        {
            get
            {
                if (_enumeratedTokenLast == true) UpdateTokenContextInstancePosition();
                return _contexts.CurrentLocation.ContextInstance.ChildContexts.Count;
            }
        }

        /// <summary>
        /// The TokenContextInstance 
        /// </summary>
        public TokenContextInstance RootContext
        {
            get { return _rootContext; }
        }

        /// <summary>
        /// The context that the current token is contained by.
        /// </summary>
        public TokenContextInstance CurrentContext
        {
            get 
            {
                if (_enumeratedTokenLast == true) UpdateTokenContextInstancePosition();
                return (_contexts.CurrentLocation.Position < 0 ? _contexts.CurrentLocation.ContextInstance : _contexts.CurrentLocation.ContextInstance.ChildContexts[_contexts.CurrentLocation.Position]);
            }
        }

        /// <summary>
        /// The number of contexts deep the TokenReader is relative to the RootContext of the TokenReader.
        /// </summary>
        public int Depth
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return _tokens.CurrentLocation.Depth; 
            }
        }

        /// <summary>
        /// The current token in the list of tokens in the CurrentContext.
        /// </summary>
        public TokenInstance CurrentToken
        {
            get 
            {
                if (_enumeratedContextLast == true) UpdateTokenInstancePosition();
                return CurrentContext.Tokens[TokenPosition]; 
            }
        }

        /// <summary>
        /// Creates a new instance of the TokenReader class with the provided TokenContextInstance as the RootContext.
        /// </summary>
        /// <param name="context">THe root context of the reader.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenReader(TokenContextInstance context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _rootContext = context;
            _tokens = new TokenInstanceEnumerable(context, false);
            _instanceEnumerator = _tokens.GetEnumerator();

            _contexts = new TokenContextInstanceEnumerable(context, false);
            _contextEnumerator = _contexts.GetEnumerator();
        }

        /// <summary>
        /// Creates a new instance of the TokenReader class with the provided TokenInstance's Context as the RootContext.
        /// </summary>
        /// <param name="instance">The token with the root context of the reader</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenReader(TokenInstance instance)
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

        /// <summary>
        /// Gets the next token in the current context, including placeholders. If recursive is true, placeholder tokens are dug into and have their first token returned instead.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public TokenInstance GetNextToken(bool recursive = false)
        {
            UpdateTokenInstancePosition();
            _enumeratedTokenLast = true;

            _tokens.Direction = SeekDirection.Forwards;
            _tokens.Recursive = recursive;

            if (_instanceEnumerator == null) _instanceEnumerator = _tokens.GetEnumerator();
            if (_instanceEnumerator.MoveNext() == false)
            {
                _instanceEnumerator = null;
                return null;
            }

            return _instanceEnumerator.Current;
        }

        /// <summary>
        /// Gets the next token in the current context, including placeholders, that have the given context definition. If recursive is true, placeholder tokens are dug into and have their first matching token returned instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recursive"></param>
        /// <returns></returns>
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
