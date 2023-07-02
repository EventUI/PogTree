/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace YoggTree.Core.Enumerators
{
    /// <summary>
    /// IEnumerable for iterating over a collection of TokenContextInstances belonging to a parent TokenContextInstance.
    /// </summary>
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

        /// <summary>
        /// Gets an IEnumerator for the TokenContextInstance.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TokenContextInstance> GetEnumerator()
        {
            TokenContextInstance nextToken = (Direction == SeekDirection.Forwards) ? GetNextContext() : GetPreviousContext();
            while (nextToken != null)
            {
                yield return nextToken;
                nextToken = (Direction == SeekDirection.Forwards) ? GetNextContext() : GetPreviousContext();
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the next context (in the forwards direction). This loop returns the parent context and then its children recursively. 
        /// </summary>
        /// <returns></returns>
        private TokenContextInstance GetNextContext()
        {
            return GetNextContext2();

            if (_currentLocation.Position >= _currentLocation.ContextInstance.ChildContexts.Count)
            {
                if (_currentLocation.Depth == 0) //we're at the top of the hierarchy and have reached the end of the context. All done.
                {
                    return null;
                }

                //otherwise go back up the stack and pick up where we left off
                _currentLocation = _depthStack.Pop();
                return GetNextContext();
            }

            if (_currentLocation.Position < -1) _currentLocation.Position = -1;
            TokenContextInstance currentContext = null;

            //if we're at -1 with a depth of 0, we are at the root context
            if (_currentLocation.Position < 0 && _currentLocation.Depth == 0)
            {
                currentContext = _currentLocation.ContextInstance;
            }
            else //otherwise we're looking at a child context
            {
                if (_currentLocation.ContextInstance.ChildContexts.Count > 0) //we have child contexts, get the next one
                {
                    if (_currentLocation.Position == -1) _currentLocation.Position++; //sometimes the position defaults to -1, so we skip forward to 0 so we pick up the first child. Otherwise we'd return the same context twice in a row.
                    currentContext = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
                }
                else //no child contexts
                {
                    if (_currentLocation.Depth == 0) //at the top of the hierarchy, all done
                    {
                        return null;
                    }
                    else //go back up the stack and continue where we left off
                    {
                        _currentLocation = _depthStack.Pop();
                        return GetNextContext();
                    }
                }
            }

            if (currentContext.Parent == _currentLocation.ContextInstance) //if we have a child context as our current context
            {
                if (_recursive == false) //if not recursive advance the position and return the current context.
                {
                    _currentLocation.Position++;
                    return currentContext;
                }

                //move forward one slot so we pick up AFTER the current context and then dig deeper into the hierarchy.
                _currentLocation.Position++;
                _depthStack.Push(_currentLocation);

                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = currentContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = -1 //-1 is the special value that means "at the parent"
                };

                return currentContext;
            }


            if (currentContext.ChildContexts.Count == 0 || _recursive == false) //if we have no contexts or are not recursive, just go forward one slot
            {
                _currentLocation.Position++;
                return currentContext;
            }
            else
            {
                if (_recursive == false) //move forward one slot - if recursive is false then we had more than one child context, so return the next one
                {
                    _currentLocation.Position++;
                    return _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
                }

                //if we are recursive we dig in deeper and advance the current position to the next slot so when we bubble back up we're sitting at the next context and not the one about to be returned.
                _currentLocation.Position++;
                _depthStack.Push(_currentLocation);

                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = currentContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = -1
                };

                return currentContext;
            }
        }

        private TokenContextInstance GetNextContext2()
        {
            if (_currentLocation.Position == -2) _currentLocation.Position = -1;
            if (_currentLocation.Position == -1)
            {
                _currentLocation.Position++;
                return _currentLocation.ContextInstance;
            }

            if (_currentLocation.ContextInstance.ChildContexts.Count > _currentLocation.Position)
            {
                var contextInstance = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
                if (contextInstance.ChildContexts.Count > 0 && _recursive == true)
                {
                    _currentLocation.Position++;
                    _currentLocation = new TokenContextInstanceLocation()
                    {
                        ContextInstance = contextInstance,
                        Depth = _currentLocation.Depth + 1,
                        Position = -1
                    };

                    return GetNextContext2();
                }
                else
                {
                    _currentLocation.Position++;
                    return contextInstance;
                }
            }
            else
            {
                if (_recursive == false || _currentLocation.Depth == 0)
                {
                    return null;
                }

                _currentLocation = _depthStack.Pop();
                return GetNextContext2();
            }
        }

        /// <summary>
        /// Gets the previous token (In the backwards direction). Moving backwards never returns the parent itself.
        /// </summary>
        /// <returns></returns>
        private TokenContextInstance GetPreviousContext()
        {
            return GetPreviousContext2();

            if (_currentLocation.Position <= 0)
            {
                if (_currentLocation.Position == -1 && _currentLocation.Depth == 0)
                {
                    _currentLocation.Position++;
                    return _currentLocation.ContextInstance;
                }

                if (_currentLocation.ContextInstance.ChildContexts.Count == 0)
                {
                    if (_recursive == true && _currentLocation.Depth != 0)
                    {
                        _currentLocation = _depthStack.Pop();
                        return GetPreviousContext();
                    }
                    else
                    {
                        return null;
                    }
                }
            }


            if (_currentLocation.Position < _currentLocation.ContextInstance.ChildContexts.Count)
            {
                if (_currentLocation.Position == -1 || _recursive == false)
                {
                    return _currentLocation.ContextInstance;
                }
                else
                {
                    var context = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
                    _currentLocation.Position--;

                    return context;
                }
            }
            else
            {
                if (_recursive == true && _currentLocation.Depth != 0)
                {
                    _currentLocation = _depthStack.Pop();
                    return GetPreviousContext();
                }
                else
                {
                    return null;
                }
            }
        }

        private TokenContextInstance GetPreviousContext2()
        {
            if (_currentLocation.Position < 0)
            {
                if (_currentLocation.Position == -1)
                {
                    _currentLocation.Position--;
                    return _currentLocation.ContextInstance;
                }
                else
                {
                    if (_recursive == false || _currentLocation.Depth == 0) return null;
                }

                _currentLocation = _depthStack.Pop();
                return GetPreviousContext2();
            }

            _currentLocation.Position--;
            if (_currentLocation.Position == -1) return GetPreviousContext2();
            if (_currentLocation.ContextInstance.ChildContexts.Count == 0)
            {
                _currentLocation.Position--;
                return GetPreviousContext2();
            }

            var previousContext = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
            if (previousContext.ChildContexts.Count > 0 && _recursive == true)
            {
                _depthStack.Push(_currentLocation);
                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = previousContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = previousContext.ChildContexts.Count
                };

                return GetPreviousContext2();
            }

            return previousContext;
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
            else
            {
                if (origin == SeekOrigin.End)
                {
                    if (_currentLocation.ContextInstance.Tokens.Count == 0)
                    {
                        _currentLocation.Position = 0;
                    }
                    else
                    {
                        _currentLocation.Position = _currentLocation.ContextInstance.Tokens.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Seeks to the StartToken of the context instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        internal void Seek(TokenContextInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            if (instance.Parent == null) //if there's no parent this is the root context of the whole parsed graph, so just reset the location back to the start
            {
                if (instance == _rootContext)
                {
                    _depthStack.Clear();
                    _currentLocation = new TokenContextInstanceLocation()
                    {
                        ContextInstance = instance,
                        Depth = 0,
                        Position = GetContextIndex(instance)
                    };

                    _depthStack.Push(_currentLocation);

                    return;
                }
                else //if we went "above" our root context, we have made a mistake.
                {
                    throw new Exception("TokenInstance not contained by RootContext of " + nameof(TokenInstanceEnumerable));
                }
            }

            var contextStack = new Stack<TokenContextInstanceLocation>();
            TokenContextInstance parent = instance.Parent;

            var initialLocation = new TokenContextInstanceLocation();
            initialLocation.ContextInstance = parent;
            initialLocation.Position = GetContextIndex(instance);

            contextStack.Push(initialLocation);

            bool isUnderRoot = false;

            while (parent != null) //go up the hierarchy and find the position and depth of each layer.
            {
                if (parent == _rootContext)
                {
                    isUnderRoot = true;
                }

                if (parent.Parent != null)
                {
                    var location = new TokenContextInstanceLocation();
                    location.ContextInstance = parent.Parent;
                    location.Position = GetContextIndex(parent);

                    contextStack.Push(location);
                    parent = parent.Parent;
                }
                else
                {
                    break;
                }

                if (isUnderRoot == true) break;
            }

            if (isUnderRoot == false) throw new Exception("TokenContextInstance not contained by RootContext of " + nameof(TokenInstanceEnumerable));

            //clear our current location as it is no longer relevant and build a new one based on the results of the loop above. We reverse the order of the contexts by moving them from a queue to a stack.
            _depthStack.Clear();

            int depthCounter = 0;
            while (contextStack.Count > 0)
            {
                var nextLocation = contextStack.Pop();
                nextLocation.Depth = depthCounter;

                depthCounter++;

                _depthStack.Push(nextLocation);
            }

            _currentLocation = initialLocation;
        }

        /// <summary>
        /// Gets the index of a context instance in its parent.
        /// </summary>
        /// <param name="instance">The instance to find the index of in its parent context.</param>
        /// <returns></returns>
        internal static int GetContextIndex(TokenContextInstance instance)
        {
            if (instance == null || instance.Parent == null) return -1;
            for (int x = 0; x < instance.Parent.ChildContexts.Count; x++)
            {
                var curToken = instance.Parent.ChildContexts[x];
                if (curToken == instance) return x;
            }

            return -1;
        }

        internal void SeekToEnd(bool recursive)
        {
            if (recursive == false)
            {
                _currentLocation.Position = (_currentLocation.ContextInstance.ChildContexts.Count == 0) ? -1 : _currentLocation.ContextInstance.ChildContexts.Count - 1;
            }
            else
            {

                if (_rootContext.ChildContexts.Count == 0)
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
                else
                {
                    TokenContextInstance parentContext = _rootContext;
                    TokenContextInstance lastContext = _rootContext.ChildContexts[_rootContext.ChildContexts.Count - 1];
                    while (lastContext != null)
                    {
                        bool foundToken = false;
                        bool foundChildContext = false;

                        if (lastContext.ChildContexts.Count == 0)
                        {
                            break;
                        }

                        for (int x = parentContext.ChildContexts.Count - 1; x >= 0; x--)
                        {
                            lastContext = parentContext.ChildContexts[x];
                            if (lastContext.ChildContexts.Count > 0)
                            {
                                parentContext = lastContext;
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

                    if (lastContext != null)
                    {
                        Seek(lastContext);
                    }
                }
            }
        }

        internal void SeekToBeginning(bool recursive)
        {
            if (_recursive == false)
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

