/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System.Collections;

namespace YoggTree.Core.Enumerators
{
    /// <summary>
    /// An IEnumerable of TokenInstance that iterates through a TokenContextInstance's Tokens with options for recursive seeking and token iteration in both backwards and forward directions.
    /// </summary>
    public sealed class TokenInstanceEnumerable : IEnumerable<TokenInstance>
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
        internal TokenInstanceEnumerable(TokenContextInstance rootContext, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));

            _rootContext = rootContext;
            _recursive = recursive;
            _currentLocation = new TokenContextInstanceLocation()
            {
                ContextInstance = rootContext,
                Depth = 0,
                Position = -1,
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
        internal TokenInstanceEnumerable(TokenContextInstance rootContext, int position, bool recursive)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (position > rootContext.Tokens.Count) throw new ArgumentOutOfRangeException(nameof(position));

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

        /// <summary>
        /// Gets the Enumerator for the Tokens list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TokenInstance> GetEnumerator()
        {
            TokenInstance nextToken = (Direction == SeekDirection.Forwards) ? GetNextToken() : GetPreviousToken();
            while (nextToken != null)
            {
                yield return nextToken;
                nextToken = (Direction == SeekDirection.Forwards) ? GetNextToken() : GetPreviousToken();
            }            

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the next token (in the forwards direction). Token placeholders are skipped over and dug into instead of being returned.
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetNextToken()
        {
            if (_currentLocation.Position + 1 >= _currentLocation.ContextInstance.Tokens.Count) //reached the end of a context, "pop" back up to where we were in the parent stream to keep going.
            {
                if (_currentLocation.Depth == 0) //we're at the top of the hierarchy and have reached the end of the context. All done.
                {
                    return null;
                }

                _currentLocation = _depthStack.Pop();
                return GetNextToken();
            }

             _currentLocation.Position++;
            TokenInstance nextToken = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];

            if (_recursive == true && nextToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder) //recursive means we skip placeholders, so the "next" token is either the first child of the context, or, if the context is null, the next token in current context
            {
                var childContext = nextToken.GetChildContext();
                if (childContext == null || childContext.Tokens.Count == 0) //no child context, advance to the next token
                {
                    _currentLocation.Position++;
                    return GetNextToken();
                }

                _depthStack.Push(_currentLocation);

                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = childContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = -1
                };

                return GetNextToken();
            }

            return nextToken;
        }

        /// <summary>
        /// Gets the previous token (In the backwards direction).
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetPreviousToken()
        {
            return GetPreviousToken2();

            TokenInstance previousToken = null;

            if (_currentLocation.Position <= 0)
            {
                if (_currentLocation.Position == 0)
                {
                    if (_currentLocation.ContextInstance.Tokens.Count > 0)
                    {
                        previousToken = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
                        _currentLocation.Position--;
                    }
                }
                else if (_currentLocation.Position < 0)
                {

                }

                if (previousToken == null && _recursive == true && _currentLocation.Depth != 0)
                {
                    _currentLocation = _depthStack.Pop();
                    return GetPreviousToken();
                }
            }
            else
            {
                _currentLocation.Position--;
                if (_currentLocation.Position < _currentLocation.ContextInstance.Tokens.Count)
                {
                    if (_currentLocation.Position == -1 && _currentLocation.Depth > 0)
                    {
                        if (_recursive == true)
                        {
                            _currentLocation = _depthStack.Pop();
                            return GetPreviousToken();
                        }

                        return null;
                    }
                    else
                    {
                        previousToken =_currentLocation.ContextInstance.Tokens[_currentLocation.Position];
                    }
                }
                else
                {
                    if (_recursive == true && _currentLocation.Depth != 0)
                    {
                        _currentLocation = _depthStack.Pop();
                        return GetPreviousToken();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (previousToken != null && previousToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder && _recursive == true)
            {
                _currentLocation.Position--;
                _currentLocation = _depthStack.Pop();
                return GetPreviousToken();
            }
            else
            {
                return previousToken;
            }
        }

        private TokenInstance GetPreviousToken2()
        {
            if (_currentLocation.Position == -1)
            {
                if (_currentLocation.Depth == 0 || _recursive == false) return null;
                _currentLocation = _depthStack.Pop();

                return GetPreviousToken2();
            }

            _currentLocation.Position--;
            if (_currentLocation.Position == -1) return GetPreviousToken2();

            TokenInstance previousToken = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
            if (_recursive == true && previousToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
            {
                var childContext = previousToken.GetChildContext();
                if (childContext == null || childContext.Tokens.Count == 0)
                {
                    _currentLocation.Position--;
                    return GetPreviousToken2();
                }

                _depthStack.Push(_currentLocation);

                 _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = childContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = childContext.Tokens.Count
                };

                return GetPreviousToken2();
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
                    if (_currentLocation.ContextInstance.Tokens.Count + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset)); //moving backwards to a token that falls off the start of the context.
                    _currentLocation.Position = _currentLocation.ContextInstance.Tokens.Count + offset;
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
                    if (_currentLocation.ContextInstance.Tokens.Count < offset) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position = offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    if (_currentLocation.ContextInstance.Tokens.Count < offset + _currentLocation.Position) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position += offset;
                }
            }
            else
            {
                if (origin == SeekOrigin.End)
                {
                    if (_currentLocation.ContextInstance.Tokens.Count == 0)
                    {
                        _currentLocation.Position = -1;
                    }
                    else
                    {
                        _currentLocation.Position = _currentLocation.ContextInstance.Tokens.Count - 1;
                    }  
                }
            }
        }

        /// <summary>
        /// Seeks the IEnumerable to the position of the given token.
        /// </summary>
        /// <param name="instance">The token to advance to.</param>
        /// <exception cref="Exception"></exception>
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
            while (depthCounter > 0)
            {
                var nextLocation = contextQueue.Dequeue();
                nextLocation.Depth = depthCounter - 1;

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
                    Position = -1,
                    Depth = 0
                };

                _depthStack.Clear();
                _depthStack.Push(_currentLocation);
            }
            else
            {
                Seek(context.StartToken);
            }
        }
        
        internal static int GetTokenIndex(TokenInstance instance)
        {
            if (instance == null || instance.Context == null) return -1;
            for (int x = 0; x < instance.Context.Tokens.Count; x++)
            {
                if (instance.Context.Tokens[x] == instance) return x;
            }

            return -1;            
        }

        internal static int GetTokenIndex(TokenContextInstance instance)
        {
            if (instance == null || instance.Parent == null) return -1;
            for (int x = 0; x < instance.Parent.Tokens.Count; x++)
            {
                var curToken = instance.Parent.Tokens[x];
                if (curToken.TokenInstanceType != TokenInstanceType.ContextPlaceholder) continue;
                if (curToken.GetChildContext() == instance) return x;   
            }

            return -1;
        }

        internal void SeekToEnd(bool recursive)
        {
            if (recursive == false)
            {
                _currentLocation.Position = (_currentLocation.ContextInstance.Tokens.Count == 0) ? 0 : _currentLocation.ContextInstance.Tokens.Count - 1;
            }
            else
            {

                if (_rootContext.Tokens.Count == 0)
                {
                    _depthStack.Clear();
                    _currentLocation = new TokenContextInstanceLocation()
                    {
                        ContextInstance = _rootContext,
                        Depth = 0,
                        Position = 0
                    };

                    _depthStack.Push(_currentLocation);
                }
                else
                {                        
                    TokenContextInstance parentContext = _rootContext;
                    TokenInstance lastToken = _rootContext.Tokens[_rootContext.Tokens.Count - 1];
                    while (lastToken != null && lastToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
                    {
                        bool foundToken = false;
                        bool foundChildContext = false;

                        if (parentContext.Tokens.Count == 0)
                        {
                            break;
                        }

                        for (int x = parentContext.Tokens.Count - 1; x >= 0; x--)
                        {
                            lastToken = parentContext.Tokens[x];
                            if (lastToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
                            {
                                parentContext = lastToken.GetChildContext();
                                foundChildContext = true;
                                break;
                            }
                            else
                            {
                                foundToken = true;
                                break;
                            }
                        }

                        if (foundToken == true)
                        {                            
                            break;
                        }
                        else if (foundChildContext == true)
                        {
                            continue;
                        }
                    }

                    if (lastToken != null)
                    {
                        Seek(lastToken);
                    }
                }
            }
        }

        internal void SeekToBeginning(bool recursive)
        {
            if (recursive == false)
            {
                _currentLocation.Position = -1;
            }
            else
            {
                _depthStack.Clear();
                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = _rootContext,
                    Depth = 0,
                    Position = -1
                };

                _depthStack.Push(_currentLocation);
            }
        }
    }
}
