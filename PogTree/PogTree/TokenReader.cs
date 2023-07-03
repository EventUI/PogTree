/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PogTree.Core.Enumerators;

namespace PogTree
{
    /// <summary>
    /// Utility class for searching through a TokenContextInstance hierarchy. It is intended to be used on a TokenContextInstance that has been completed, using it on an unfinished parse operation could result in odd behavior.
    /// </summary>
    public sealed class TokenReader
    {
        /*Note: This class is intended to seek backwards and forwards in both the Tokens and ChildContexts arrays of a single TokenContextInstance, but each required its own IEnumerable implementation.
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

                //doing a bit of hackery to make some of the exception cases from the enumerable not case an out of range exception.
                int position = _tokens.CurrentLocation.Position; 
                if (position < 0) position = 0;
                if (position != 0 && position >= _tokens.CurrentLocation.ContextInstance.Tokens.Count) position = _tokens.CurrentLocation.ContextInstance.Tokens.Count - 1;

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

                //doing a bit of hackery to make some of the exception cases from the enumerable not case an out of range exception.
                int position = _contexts.CurrentLocation.Position;
                if (position < 0) position = -1;
                if (position != 0 && position >= _contexts.CurrentLocation.ContextInstance.ChildContexts.Count) position = _contexts.CurrentLocation.ContextInstance.ChildContexts.Count - 1;

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
                return (_contexts.CurrentLocation.Position < 0 ? _contexts.CurrentLocation.ContextInstance : _contexts.CurrentLocation.ContextInstance.ChildContexts[ContextPosition]);
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
                return _tokens.CurrentLocation.ContextInstance.Tokens[TokenPosition]; 
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
        /// Gets the next token in the current context, including placeholders, that have the given TokenDefinition. If recursive is true, placeholder tokens are dug into and have their first matching token returned instead.
        /// </summary>
        /// <typeparam name="T">The type of token to find.</typeparam>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
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

        /// <summary>
        /// Gets the previous token in the current context, including placeholders. If recursive is true, placeholder tokens are dug into and have their last token returned instead.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the previous token in the current context, including placeholders, that have the given TokenDefinition. If recursive is true, placeholder tokens are dug into and have their last matching token returned instead.
        /// </summary>
        /// <typeparam name="T">The type of token to find.</typeparam>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public TokenInstance GetPreviousToken<T>(bool recursive = false) where T : ITokenDefinition
        {
            TokenInstance previousToken = GetPreviousToken(recursive);
            while (previousToken != null && previousToken.Is<T>() == false)
            {
                previousToken = GetPreviousToken(recursive);
            }

            return previousToken;
        }

        /// <summary>
        /// Gets an IEnumerable for all the TokenInstances contained in this reader, starting at the root context. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenInstance> GetAllTokens(bool recursive = false)
        {
            return new TokenInstanceEnumerable(_rootContext, recursive);
        }

        /// <summary>
        /// Gets an IEnumerable for all the TokenInstances that come after the current token. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenInstance> GetRemainingTokens(bool recursive = false)
        {
            var iterator = new TokenInstanceEnumerable(CurrentContext, recursive);
            iterator.Seek(CurrentToken);

            return iterator;
        }

        /// <summary>
        /// Gets an IEnumerable for all the TokenContextInstances contained in this reader, starting at the root context.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenContextInstance> GetAllTokenContexts(bool recursive = false)
        {
            return new TokenContextInstanceEnumerable(_rootContext, recursive);
        }

        /// <summary>
        /// Gets an IEnumerable for all the TokenContextInstances that come after the current context.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenContextInstance> GetRemainingTokenContexts(bool recursive = false)
        {

            var iterator = new TokenContextInstanceEnumerable(CurrentContext, recursive);
            iterator.Seek(CurrentContext);

            return iterator;
        }

        /// <summary>
        /// Gets an IEnumerable of all TokenInstances (starting at the RootContext) that is filtered by the predicate. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="predicate">A filtering function used to select TokenInstances.</param>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenInstance> SearchAll(Func<TokenInstance, bool> predicate, bool recursive = false)
        {
            return GetAllTokens(recursive).Where(predicate);
        }

        /// <summary>
        /// Gets an IEnumerable of TokenContextInstances (starting at the RootContext) that is filtered by the predicate. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="predicate">A filtering function used to select TokenContextInstances.</param>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenContextInstance> SearchAll(Func<TokenContextInstance, bool> predicate, bool recursive = false)
        {
            return GetAllTokenContexts(recursive).Where(predicate);
        }

        /// <summary>
        /// Gets an IEnumerable of all TokenInstances (starting at the CurrentToken) that is filtered by the predicate. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="predicate">A filtering function used to select TokenInstances.</param>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenInstance> Search(Func<TokenInstance, bool> predicate, bool recursive = false)
        {
            return GetRemainingTokens(recursive).Where(predicate);
        }

        /// <summary>
        /// Gets an IEnumerable of TokenContextInstances (starting at the CurrentContext) that is filtered by the predicate. If recursive is set to true this will skip placeholder tokens and recursively follow the hierarchy of contexts.
        /// </summary>
        /// <param name="predicate">A filtering function used to select TokenContextInstances.</param>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public IEnumerable<TokenContextInstance> Search(Func<TokenContextInstance, bool> predicate, bool recursive = false)
        {
            return GetRemainingTokenContexts(recursive).Where(predicate);
        }

        /// <summary>
        /// Seeks the TokenReader to a new position within the CurrentContext.
        /// </summary>
        /// <param name="offset">The offset from the SeekOrigin.</param>
        /// <param name="origin">The point of reference to Seek from.</param>
        public void SeekTokens(int offset, SeekOrigin origin)
        {
            _tokens.Seek(offset, origin);
            _enumeratedTokenLast = true;                    
        }

        /// <summary>
        /// Seeks the TokenReader to a new position within the CurrentContext.
        /// </summary>
        /// <param name="offset">The offset from the SeekOrigin.</param>
        /// <param name="origin">The point of reference to Seek from.</param>
        public void SeekContexts(int offset, SeekOrigin origin)
        {
            _contexts.Seek(offset, origin);
            _enumeratedContextLast = true;
        }

        /// <summary>
        /// Seeks the TokenReader to a new position within the CurrentContext or the hierarchy of tokens and contexts under the RootContext if performing a recursive operation.
        /// </summary>
        /// <param name="location">The location to seek to in the TokenReader.</param>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
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

        /// <summary>
        /// Seeks the TokenReader to the given token instance.
        /// </summary>
        /// <param name="instance"></param>
        public void Seek(TokenInstance instance)
        {
            _tokens.Seek(instance);
            _enumeratedTokenLast = true;
        }

        /// <summary>
        /// Seeks the TokenReader to the FirstToken given context instance.
        /// </summary>
        /// <param name="context"></param>
        public void Seek(TokenContextInstance context)
        {
            _contexts.Seek(context);
            _enumeratedContextLast = true;
        }

        /// <summary>
        /// Gets the next context in the CurrentContext's ChildContexts list. Returns the parent context before its children if recursion is enabled.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the next context in the CurrentContext's ChildContexts list that has the given TokenContextDefinition. Returns the parent context before its children if recursion is enabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public TokenContextInstance GetNextContext<T>(bool recursive = false) where T : ITokenContextDefinition
        {
            TokenContextInstance nextContext = GetNextContext(recursive);
            while (nextContext != null && nextContext.Is<T>() == false)
            {
                nextContext = GetNextContext(recursive);
            }

            return nextContext;
        }

        /// <summary>
        /// Gets the previous context in the CurrentContext's ChildContexts list. Returns the parent context after its children if recursion is enabled.
        /// </summary>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the previous context in the CurrentContext's ChildContexts list that has the given TokenContextDefinition. Returns the parent context before its children if recursion is enabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recursive">Whether or not to perform a recursive operation or stay in the same context. False by default.</param>
        /// <returns></returns>
        public TokenContextInstance GetPreviousContext<T>(bool recursive = false) where T : ITokenContextDefinition
        {
            TokenContextInstance previousInstance = GetPreviousContext(recursive);
            while (previousInstance != null && previousInstance.Is<T>() == false)
            {
                previousInstance = GetPreviousContext(recursive);
            }

            return previousInstance;
        }

        /// <summary>
        /// One of two annoying little syncing functions that need to be called when the user switches from iterating TokenInstances to something requiring the current position of the CurrentContext.
        /// We need to do this because we have two separate Enumerators - one for Tokens, one for Contexts, and advancing one does not advance the other, so they need to be synced, but done in a "lazy" way
        /// so we're not syncing after every operation.
        /// </summary>

        private void UpdateTokenInstancePosition()
        {
            if (_enumeratedTokenLast == true) return;
            if (_enumeratedContextLast == false) return;

            TokenInstance referenceToken = null;
            TokenContextInstance lastContext = _contextEnumerator?.Current; //
            
            //if (lastContext == null) lastContext = _contexts.CurrentLocation.Position < 0 ? _contexts.CurrentLocation.ContextInstance : _contexts.CurrentLocation.ContextInstance.ChildContexts[_contexts.CurrentLocation.Position];

            if (lastContext == null)
            {
                if (_contexts.CurrentLocation.Position < 0)
                {
                    lastContext = _contexts.CurrentLocation.ContextInstance;
                }
                else
                {
                    if (_contexts.CurrentLocation.Position > _contexts.CurrentLocation.ContextInstance.ChildContexts.Count)
                    {
                        lastContext = _contexts.CurrentLocation.ContextInstance.ChildContexts[_contexts.CurrentLocation.ContextInstance.ChildContexts.Count - 1];
                    }
                    else
                    {
                        lastContext = _contexts.CurrentLocation.ContextInstance.ChildContexts[_contexts.CurrentLocation.Position];
                    }
                }
            }


            if (_tokens.CurrentLocation.ContextInstance == lastContext)
            {
                _enumeratedContextLast = false;
                return;
            }

            referenceToken = lastContext.StartToken;

            _tokens.Seek(referenceToken);
            _enumeratedContextLast = false;
        }
        /// <summary>
        /// One of two annoying little syncing functions that need to be called when the user switches from iterating TokenContextInstances to something requiring the current position of the CurrentToken.
        /// We need to do this because we have two separate Enumerators - one for Tokens, one for Contexts, and advancing one does not advance the other, so they need to be synced, but done in a "lazy" way
        /// so we're not syncing after every operation.
        /// </summary>
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
                TokenInstance lastToken = null;

                if (position >= _tokens.CurrentLocation.ContextInstance.Tokens.Count)
                {
                    lastToken = _tokens.CurrentLocation.ContextInstance.Tokens[_tokens.CurrentLocation.ContextInstance.Tokens.Count - 1];
                }
                else
                {
                    lastToken = _tokens.CurrentLocation.ContextInstance.Tokens[position];
                }

                _contexts.Seek(lastToken.Context);
            }

            _enumeratedTokenLast = false;
        }
    }
}
