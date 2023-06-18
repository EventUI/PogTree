/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System.Collections;

namespace YoggTree.Core.Enumerators
{
    public sealed class TokenInstanceEnumerable : IEnumerable<TokenInstance>
    {
        private TokenContextInstance _rootContext = null;
        private bool _recursive = false;
        private TokenContextInstanceLocation _currentLocation = null;
        private SeekDirection _direction = SeekDirection.Forwards;
        private readonly Stack<TokenContextInstanceLocation> _depthStack = new Stack<TokenContextInstanceLocation>();
          
        internal TokenContextInstanceLocation CurrentLocation { get { return _currentLocation; } }

        internal bool Recursive { get { return _recursive; } set { _recursive = value; } }

        internal SeekDirection Direction { get { return _direction; } set { _direction = value; } }

        internal TokenInstanceEnumerable(TokenContextInstance rootContext, bool recursive)
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

        private TokenInstance GetNextToken()
        {
            TokenInstance instance = null;

            if (_currentLocation.ContextInstance.Tokens.Count == _currentLocation.Position) //reached the end of a context, "pop" back up to where we were in the parent stream to keep going.
            {
                if (_currentLocation.Depth == 0)
                {
                    _currentLocation = null;
                    return null;
                }

                _currentLocation = _depthStack.Pop();

                return GetNextToken();
            }

            if (_currentLocation.Position < 0) _currentLocation.Position = 0;
            instance = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
            _currentLocation.Position++;

            if (_recursive == true && instance.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
            {
                var childContext = instance.GetChildContext();
                if (childContext == null) return instance;

                _depthStack.Push(_currentLocation);

                _currentLocation = new TokenContextInstanceLocation()
                {
                    ContextInstance = instance.GetChildContext(),
                    Depth = _currentLocation.Depth + 1,
                    Position = 0
                };
            }

            return instance;
        }

        private TokenInstance GetPreviousToken()
        {
            if ((_currentLocation.Depth == 0 || Recursive == false) && _currentLocation.Position < 0) return null;

            TokenInstance instance = null;
            if (_currentLocation.Position <= 0)
            {
                if (_currentLocation.Position == 0)
                {
                    instance = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
                    if (_currentLocation.Depth > 0 && Recursive == true)
                    {
                        _currentLocation = _depthStack.Pop();
                        return instance;
                    }
                    else
                    {
                        _currentLocation.Position--;
                        return instance;
                    }
                }
                else
                {
                    if (_currentLocation.Depth > 0 && Recursive == true)
                    {
                        _currentLocation = _depthStack.Pop();
                        return _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                instance = _currentLocation.ContextInstance.Tokens[_currentLocation.Position];
            }
            
            _currentLocation.Position--;
            return instance;
        }

        internal void Seek(int offset, SeekOrigin origin)
        {
            if (offset < 0)
            {
                if (origin == SeekOrigin.Begin)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                else if (origin == SeekOrigin.End)
                {
                    if (_currentLocation.ContextInstance.Tokens.Count + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position = _currentLocation.ContextInstance.Tokens.Count + offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    if (_currentLocation.Position + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                    _currentLocation.Position = _currentLocation.Position + offset;
                }
            }
            else if (offset > 0)
            {
                if (origin == SeekOrigin.End)
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
    }
}
