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

namespace PogTree.Core.Enumerators
{
    /// <summary>
    /// IEnumerable for iterating over a collection of TokenContextInstances belonging to a parent TokenContextInstance.
    /// </summary>
    public sealed class TokenContextInstanceEnumerable : IEnumerable<TokenContextInstance>
    {
        /*A note about this enumerator: in recursive mode it returns both the parent context (first) then it's children recursively. To make this all sync up properly the Position of the _currentLocation
        always starts at -2, this is because a Position of -1 says "return yourself" and we always increment the Position of the _currentLocation BEFORE getting a context so that the index lines up with
        the token that was just returned.*/


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
                Position = -2,
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
        private TokenContextInstance GetNextContext()
        {
            _currentLocation.Position++; //always increment counter first so that the Position is always the index of the CurrentContext. 
            if (_currentLocation.Position == -1) //-1 is the magic number for "return yourself" and this is why we start at -2.
            {
                return _currentLocation.ContextInstance;
            }

 
            if (_currentLocation.ContextInstance.ChildContexts.Count > _currentLocation.Position) //we have more contexts to iterate over
            {
                var contextInstance = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
                if (contextInstance.ChildContexts.Count > 0 && _recursive == true) //we're in recursive mode and it has children, dig into it and have it return itself next.
                {
                    _depthStack.Push(_currentLocation);
                    _currentLocation = new TokenContextInstanceLocation()
                    {
                        ContextInstance = contextInstance,
                        Depth = _currentLocation.Depth + 1,
                        Position = -2
                    };

                    return GetNextContext();
                }
                else //not recursive, just return what's at the index
                {
                    return contextInstance;
                }
            }
            else //out of tokens in this context
            {
                if (_recursive == false || _currentLocation.Depth == 0) //we're at the top, or not recursively walking the hierarchy, so return null to signal "all done"
                {
                    return null;
                }

                //otherwise, we walk back up in the hierarchy and perform the same logic again.
                _currentLocation = _depthStack.Pop();
                return GetNextContext();
            }
        }

        /// <summary>
        /// Gets the previous token (in the backwards direction). Moving backwards never returns the parent itself.
        /// </summary>
        /// <returns></returns>
        private TokenContextInstance GetPreviousContext()
        {
            if (_currentLocation.Position < 0) //we're either returning the _currentLocation.ContextInstance or walking back up the hierarchy
            {
                if (_currentLocation.Position == -1) //-1 is the magic number for "return yourself"
                {
                    _currentLocation.Position--;
                    return _currentLocation.ContextInstance;
                }
                else //if the position is less than -1 and we're either at the top or not recursively walking up, return null to signal "all done"
                {
                    if (_recursive == false || _currentLocation.Depth == 0) return null;
                }

                //otherwise pop back up in the hierarchy and perform the same logic again.
                _currentLocation = _depthStack.Pop();
                return GetPreviousContext();
            }

            _currentLocation.Position--;
            if (_currentLocation.Position == -1) return GetPreviousContext();
            if (_currentLocation.ContextInstance.ChildContexts.Count == 0 || _recursive == false)
            {
                _currentLocation.Position--; //this should leave us at -1. Call ourselves again and it will return the correct thing.
                return GetPreviousContext();
            }

            var previousContext = _currentLocation.ContextInstance.ChildContexts[_currentLocation.Position];
            if (previousContext.ChildContexts.Count > 0 && _recursive == true) //we have children to do into recursively
            {
                _depthStack.Push(_currentLocation);
                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = previousContext,
                    Depth = _currentLocation.Depth + 1,
                    Position = previousContext.ChildContexts.Count
                };

                return GetPreviousContext();
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
                        _currentLocation.Position = _currentLocation.ContextInstance.Tokens.Count;
                    }
                }
            }
        }

        /// <summary>
        /// Seeks to the StartToken of the context instance by walking backwards up the hierarchy from the context parameter to the root.
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
                        Position = -2
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

        /// <summary>
        /// Seeks the enumerable to the last context with optional recursion.
        /// </summary>
        /// <param name="recursive">Whether or not to get the very last child context in the entire hierarchy or just the last one in the current context.</param>
        internal void SeekToEnd(bool recursive)
        {
            if (recursive == false)
            {
                _currentLocation.Position = (_currentLocation.ContextInstance.ChildContexts.Count == 0) ? -2 : _currentLocation.ContextInstance.ChildContexts.Count;
            }
            else
            {

                if (_rootContext.ChildContexts.Count == 0) //we have no hierarchy to traverse, so just return the _rootContext the next time a context is requested.
                {
                    _depthStack.Clear();
                    _currentLocation = new TokenContextInstanceLocation()
                    {
                        ContextInstance = _rootContext,
                        Depth = 0,
                        Position = -2
                    };

                    _depthStack.Push(_currentLocation);
                }
                else //the root context has children, we walk through them backwards recursively, starting at the END of the context's ChildContexts array, to find the deepest child context to return.
                {
                    TokenContextInstance parentContext = _rootContext;
                    TokenContextInstance lastContext = _rootContext.ChildContexts[_rootContext.ChildContexts.Count - 1];
                    
                    while (lastContext != null)
                    {
                        bool foundToken = false;
                        bool foundChildContext = false;

                        if (lastContext.ChildContexts.Count == 0) //no more children, we're as deep as we can go.
                        {
                            break;
                        }

                        for (int x = parentContext.ChildContexts.Count - 1; x >= 0; x--)
                        {
                            lastContext = parentContext.ChildContexts[x];
                            if (lastContext.ChildContexts.Count > 0) //walk down into the child context
                            {
                                parentContext = lastContext;
                                foundChildContext = true;
                                break;
                            }
                            else //found someone with no contexts, we're as deep as we can go.
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

                    //if we found a context, seek to it (this is slightly redundant/inefficient, but it builds the _depthStack needed for walking the contexts in either direction and we don't want to duplicate that logic)
                    if (lastContext != null)
                    {
                        Seek(lastContext);
                        _currentLocation.Position++; //so getting the previous context returns the right thing.
                    }
                }
            }
        }

        /// <summary>
        /// Seeks to the beginning of the current context or, if recursive, to the beginning of the _rootContext.
        /// </summary>
        /// <param name="recursive">Whether or not to stay in the current context or move to the root.</param>
        internal void SeekToBeginning(bool recursive)
        {
            if (_recursive == false)
            {
                _currentLocation.Position = -2;
            }
            else
            {
                _depthStack.Clear();
                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = _rootContext,
                    Depth = 0,
                    Position = -2
                };

                _depthStack.Push(_currentLocation);
            }
        }
    }
}

