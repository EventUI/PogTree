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
        private TokenContextInstance _rootContext = null;
        private bool _recursive = false;
        private TokenContextInstanceLocation _currentLocation = null;
        private SeekDirection _direction = SeekDirection.Forwards;
        private readonly Stack<TokenContextInstanceLocation> _depthStack = new Stack<TokenContextInstanceLocation>();

        /// <summary>
        /// The current location of the Enumerable in the context hierarchy.
        /// </summary>
        internal TokenContextInstanceLocation CurrentLocation { get { return _currentLocation; } }

        /// <summary>
        /// Whether or not the iterator will recursively dig into or climb out of token context instances as they are encountered or end.
        /// </summary>
        internal bool Recursive { get { return _recursive; } set { _recursive = value; } }

        /// <summary>
        /// The direction in which the iterator is iterating: forwards towards the end of the context hierarchy, or backwards towards the beginning of the context hierarchy.
        /// </summary>
        internal SeekDirection Direction { get { return _direction; } set { _direction = value; } }

        /// <summary>
        /// Creates a new TokenInstanceEnumerable based on the given root context.
        /// </summary>
        /// <param name="rootContext">The context with tokens to iterate through.</param>
        /// <param name="recursive">Whether or not to recursively walk the context hierarchy.</param>
        /// <exception cref="ArgumentNullException"></exception>
        internal TokenContextInstanceEnumerable(TokenContextInstance rootContext, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));

            _rootContext = rootContext;
            _recursive = recursive;
            _currentLocation = new TokenContextInstanceLocation()
            {
                ContextInstance = rootContext,
                Depth = 0,
                Position = 0,
            };

            _depthStack.Push(_currentLocation);
        }

        /// <summary>
        /// Creates a new TokenInstanceEnumerable based on the given root context and has been advanced to the given position in the context instance.
        /// </summary>
        /// <param name="rootContext">The context with tokens to iterate through.</param>
        /// <param name="position">The position to start at in the context.</param>
        /// <param name="recursive">Whether or not to recursively walk the context hierarchy.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal TokenContextInstanceEnumerable(TokenContextInstance rootContext, int position, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (position > rootContext.ChildContexts.Count) throw new ArgumentOutOfRangeException(nameof(position));

            _rootContext = rootContext;
            _recursive = recursive;
            _currentLocation = new TokenContextInstanceLocation()
            {
                ContextInstance = rootContext,
                Depth = 0,
                Position = position,
            };

            _depthStack.Push(_currentLocation);
        }

        public IEnumerator<TokenContextInstance> GetEnumerator()
        {
            TokenInstance nextToken = (Direction == SeekDirection.Forwards) ? GetNextContext() : GetPreviousToken();
            while (nextToken != null)
            {
                yield return nextToken;
                nextToken = (Direction == SeekDirection.Forwards) ? GetNextContext() : GetPreviousToken();
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the next token (in the forwards direction).
        /// </summary>
        /// <returns></returns>
        private TokenContextInstance GetNextContext()
        {
            if (_currentLocation.Position >= _currentLocation.ContextInstance.ChildContexts.Count) //reached the end of a context, "pop" back up to where we were in the parent stream to keep going.
            {
                if (_currentLocation.Depth == 0) //we're at the top of the hierarchy and have reached the end of the context. All done.
                {
                    return null;
                }

                _currentLocation = _depthStack.Pop();
                return GetNextContext();
            }

            if (_currentLocation.Position < 0)
            {
                _currentLocation.Position = 0; //seeking backwards can put the Position to -1, so we "advance" implicitly to zero.               
            }
            else
            {
                _currentLocation.Position++;
            }

            TokenInstance nextToken = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];

            if (_recursive == true && nextToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder) //recursive means we skip placeholders, so the "next" token is either the first child of the context, or, if the context is null, the next token in current context
            {
                var childContext = nextToken.GetChildContext();
                if (childContext == null) //no child context, advance to the next token
                {
                    _currentLocation.Position++;
                    return GetNextContext();
                }

                _depthStack.Push(_currentLocation);

                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = childContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = 0
                };

                return GetNextContext();
            }

            return nextToken;
        }

        /// <summary>
        /// Gets the previous token (In the backwards direction).
        /// </summary>
        /// <returns></returns>
        private TokenContextInstance GetPreviousToken()
        {
            if (_currentLocation.Position <= 0)
            {
                if (_currentLocation.Depth == 0) return null;
                if (_recursive == false)
                {
                    return null;
                }
                else
                {
                    _currentLocation = _depthStack.Pop();
                    return GetPreviousToken();
                }
            }

            var previousToken = _currentLocation.ContextInstance.Tokens[_currentLocation.Position - 1];
            _currentLocation.Position--;

            if (_recursive == true && previousToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
            {
                return GetPreviousToken();
            }

            return previousToken;
        }

        /// <summary>
        /// Seeks the enumerable to the given location in the context being iterated over (NOT recursive).
        /// </summary>
        /// <param name="offset">The amount of move the position by.</param>
        /// <param name="origin">The relative point from where to seek the enumerable.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void Seek(int offset, SeekOrigin origin)
        {
            if (offset < 0)
            {
                if (origin == SeekOrigin.Begin) //falls off the start of the context
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                else if (origin == SeekOrigin.End)
                {
                    if (_currentLocation.ContextInstance.ChildContexts.Count + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset)); //moving backwards to a token that falls off the start of the context.
                    _currentLocation.Position = _currentLocation.ContextInstance.ChildContexts.Count + offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    if (_currentLocation.Position + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset)); //also moving backwards to a token that falls off the start of the context.
                    _currentLocation.Position = _currentLocation.Position + offset;
                }
            }
            else if (offset > 0)
            {
                if (origin == SeekOrigin.End) //falls off the end of the context.
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                else if (origin == SeekOrigin.Begin)
                {
                    if (_currentLocation.ContextInstance.ChildContexts.Count < offset) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position = offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    if (_currentLocation.ContextInstance.ChildContexts.Count < offset + _currentLocation.Position) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position += offset;
                }
            }
        }

        internal void Seek(TokenInstance instance)
        {
            var contextQueue = new Queue<TokenContextInstanceLocation>();

            TokenContextInstance parent = instance.Context;
            var initialLocation = new TokenContextInstanceLocation();
            initialLocation.ContextInstance = parent;
            initialLocation.Position = GetTokenIndex(instance);

            contextQueue.Enqueue(initialLocation);
            bool isUnderRoot = false;

            while (parent != null)
            {
                if (parent == _rootContext)
                {
                    isUnderRoot = true;
                }

                if (parent.Parent != null)
                {
                    var location = new TokenContextInstanceLocation();
                    location.ContextInstance = parent.Parent;
                    location.Position = GetTokenIndex(parent);

                    contextQueue.Enqueue(location);
                    parent = parent.Parent;
                }
                else
                {
                    break;
                }

                if (isUnderRoot == true) break;
            }

            if (isUnderRoot == false) throw new Exception("TokenInstance not contained by RootContext of " + nameof(TokenInstanceEnumerable));

            _depthStack.Clear();

            int depthCounter = contextQueue.Count;
            while (depthCounter >= 0)
            {
                var nextLocation = contextQueue.Dequeue();
                nextLocation.Depth = depthCounter;

                depthCounter--;

                _depthStack.Push(nextLocation);
            }

            _currentLocation = initialLocation;
        }
        internal void Seek(TokenContextInstance context)
        {
            if (context.StartToken == null)
            {
                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = context,
                    Position = 0,
                    Depth = 0
                };

                _depthStack.Clear();
            }
            else
            {
                Seek(context.StartToken);
            }
        }

        internal static int GetTokenIndex(TokenInstance instance)
        {
            if (instance == null || instance.Context == null) return -1;
            for (int x = 0; x < instance.Context.ChildContexts.Count; x++)
            {
                if (instance.Context.Tokens[x] == instance) return x;
            }

            return -1;
        }

        internal static int GetTokenIndex(TokenContextInstance instance)
        {
            if (instance == null || instance.Parent == null) return -1;
            for (int x = 0; x < instance.Parent.ChildContexts.Count; x++)
            {
                var curToken = instance.Parent.Tokens[x];
                if (curToken.TokenInstanceType != TokenInstanceType.ContextPlaceholder) continue;
                if (curToken.GetChildContext() == instance) return x;
            }

            return -1;
        }
    }
}

