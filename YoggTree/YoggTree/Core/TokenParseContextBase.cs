﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core
{
    /// <summary>
    /// Represents a section or sub-section of a file being parsed for tokens.
    /// </summary>
    public abstract class TokenParseContextBase
    {
        private Guid _id = Guid.NewGuid();
        private int _absoluteOffset = 0; //the offset that this context starts at relative to the ParseSession
        private IReadOnlyList<TokenParseContextBase> _roChildContexts = null; //read-only wrapper to expose as the child contexts list.
        private Dictionary<Guid, int> _lastTokenIndexes = null; //internal dictionary of the last index in the ParseSession's global token list that each type of token was accessed at.
        private IReadOnlyList<TokenInstance> _tokensRO = null; //read-only wrapper to expose as the tokens list.

        protected int _currentIndex = 0;
        protected List<TokenInstance> _tokens = new List<TokenInstance>();
        protected List<TokenParseContextBase> _childContexts = new List<TokenParseContextBase>();

        /// <summary>
        /// The unique ID of this Context.
        /// </summary>
        public Guid ID { get { return _id; } }

        /// <summary>
        /// The human-readable name describing this context.
        /// </summary>
        public string Name { get; } = null;

        /// <summary>
        /// The ParseSession this context is a child of.
        /// </summary>
        public TokenParseSession ParseSession { get; } = null;

        /// <summary>
        /// The immediate parent context of this context.
        /// </summary>
        public TokenParseContextBase Parent { get; } = null;

        /// <summary>
        /// The index in the parent context where this context starts.
        /// </summary>
        public int StartIndex { get; } = -1;

        /// <summary>
        /// The index in the parent context where this context ends.
        /// </summary>
        public int EndIndex { get; protected set; } = -1;

        /// <summary>
        /// The current index in the parse operation of this context.
        /// </summary>
        internal int CurrentIndex { get { return _currentIndex; } }

        /// <summary>
        /// The offset from the root ParseSession's Context to the beginning of this one in the source Content.
        /// </summary>
        public int AbsoluteOffset { get { return _absoluteOffset; } }

        /// <summary>
        /// How many hierarchical levels deep the Context is from the root.
        /// </summary>
        public int Depth { get; } = 0;

        /// <summary>
        /// The contents of this context.
        /// </summary>
        public ReadOnlyMemory<char> Contents { get; protected set; } = null;

        /// <summary>
        /// The token that signaled the beginning of this context.
        /// </summary>
        public TokenInstance StartToken { get; } = null;

        /// <summary>
        /// The token that signaled the end of this context.
        /// </summary>
        public TokenInstance EndToken { get; protected set; } = null;

        /// <summary>
        /// Every instance of every TokenDefinitionBase found in this context.
        /// </summary>
        public IReadOnlyList<TokenInstance> Tokens { get { return _tokensRO; } }

        /// <summary>
        /// All of the child contexts that belong to this context.
        /// </summary>
        public IReadOnlyList<TokenParseContextBase> ChildContexts { get { return _roChildContexts; } }

        /// <summary>
        /// Creates a new "root" level TokenParseContextBase that sits immediately beneath the ParseSession.
        /// </summary>
        /// <param name="session">The parent parse session of this context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenParseContextBase(TokenParseSession session, string name = null)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            ParseSession = session;
            StartIndex = 0;
            EndIndex = session.Contents.Length;
            Contents = session.Contents;
            Parent = null;
            Name = name;

            _roChildContexts = _childContexts.AsReadOnly();
            _lastTokenIndexes = new Dictionary<Guid, int>();
        }

        /// <summary>
        /// Creates a new sub-context that is the child of another context.
        /// </summary>
        /// <param name="parent">The context that is the parent of this one.</param>
        /// <param name="start">The token instance that signaled the beginning of this context.</param>
        /// <param name="end">The token instance that signaled the end of this context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenParseContextBase(TokenParseContextBase parent, TokenInstance start, string name = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (start == null) throw new ArgumentException(nameof(start));

            ParseSession = parent.ParseSession;
            Parent = parent;
            StartIndex = start.TokenStartIndex;
            StartToken = start;
            Contents = parent.Contents.Slice(StartIndex);
            Depth = parent.Depth + 1;
            Name = name;

            _lastTokenIndexes = parent._lastTokenIndexes;
            _roChildContexts = _childContexts.AsReadOnly();
            _absoluteOffset = StartIndex + parent._absoluteOffset;
        }

        /// <summary>
        /// Creates a new sub-context that is the child of another context.
        /// </summary>
        /// <param name="parent">The context that is the parent of this one.</param>
        /// <param name="start">The token instance that signaled the beginning of this context.</param>
        /// <param name="end">The token instance that signaled the end of this context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenParseContextBase(TokenParseContextBase parent, TokenInstance start, TokenInstance end, string name = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (start == null) throw new ArgumentException(nameof(start));
            if (end == null) throw new ArgumentException(nameof(end));

            ParseSession = parent.ParseSession;
            Parent = parent;
            StartIndex = start.TokenStartIndex;
            StartToken = start;
            EndIndex = end.TokenEndIndex;
            Contents = parent.Contents.Slice(StartIndex, EndIndex - StartIndex);
            Depth = parent.Depth + 1;
            Name = name;

            _lastTokenIndexes = new Dictionary<Guid, int>(parent._lastTokenIndexes);
            _roChildContexts = _childContexts.AsReadOnly();
            _absoluteOffset = StartIndex + parent._absoluteOffset;
        }        

        /// <summary>
        /// Walks the Content of this Context, identifying tokens contained within and recursively spawning child contexts when such a token is encountered.
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void WalkContent()
        {
            TokenInstance _previousToken = null;

            while (_currentIndex < Contents.Length)
            {
                TokenInstance nextToken = GetNextToken();
                if (nextToken == null) break;

                //slot for enforcing token syntax rules - if one token cannot come before or after another token without being invalid, this will stop the whole parse operation
                if (_previousToken != null)
                {
                    if (nextToken.TokenDefinition.CanComeAfter(_previousToken, this) == false) throw new Exception($"{nextToken.ToString()} cannot come after {_previousToken.ToString()}");
                    if (_previousToken.TokenDefinition.CanComeBefore(nextToken, this) == false) throw new Exception($"{_previousToken.ToString()} cannot come before {nextToken.ToString()}");

                    _previousToken = nextToken;
                }

                //check to see if both the context allows this token to be valid, and that the token detects itself to be valid in this context. Skip if either is false.
                if (nextToken.TokenDefinition.IsValidInstance(nextToken, this) == false || IsValidInContext(nextToken) == false)
                {
                    _currentIndex = nextToken.TokenEndIndex;
                    continue;
                }

                _tokens.Add(nextToken);
                _currentIndex = nextToken.TokenEndIndex;

                //if this token meets the context's criteria for starting a new sub-context, make the context and walk its contents starting at the start token.
                //The new sub-context will eventually hit the block below to end itself and then we return to this context.
                if (StartsNewContext(nextToken) == true)
                {
                    TokenParseContextBase childContext = CreateNewContext(nextToken);
                    if (childContext != null) 
                    {
                        _childContexts.Add(childContext);
                        childContext.WalkContent();                      

                        _currentIndex = _currentIndex + childContext.Contents.Length;
                        if (childContext.EndToken != null) _tokens.Add(childContext.EndToken);
                        SeekForward(_currentIndex + _absoluteOffset);
                        continue;
                    }                    
                }

                //if this token ends this context, snip the contents down to only what was inside the context and return to the parent context.
                if (EndsCurrentContext(nextToken) == true)
                {
                    _currentIndex = nextToken.TokenEndIndex;

                    Contents = Contents.Slice(0, _currentIndex);
                    EndIndex = StartIndex + _currentIndex;
                    EndToken = nextToken;

                    break;
                }
            }
        }

        /// <summary>
        /// Gets the next token in this context.
        /// </summary>
        /// <returns></returns>
        protected TokenInstance GetNextToken()
        {
            (TokenInstance, int) lowestIndex = default;

            //we walk our list of defined tokens looking for the token instance (found in the Session's global token list) that comes next inside this context.
            foreach (var tokenType in ParseSession.DefinedTokens)
            {
                //get the last index that was used to look up a token from the previous token lookup so we don't have to loop through the whole token list starting at 0 every time (which would be redundant)
                _lastTokenIndexes.TryGetValue(tokenType.ID, out int startingIndex);
                if (startingIndex < 0) startingIndex = 0;

                //because our token list is the global list from the session, we need to search for the next token using the absolute index in the root context, not the index in this local context
                var instance = tokenType.GetNextToken(_currentIndex + _absoluteOffset, this, startingIndex);
                if (instance.Instance == null) continue;

                if (lowestIndex.Item1 == null || instance.Instance.TokenStartIndex < lowestIndex.Item1.TokenStartIndex) //nothing there yet or this came before the earliest token, so this token becomes the next new token
                {
                    lowestIndex = instance;
                }
                else if (instance.Instance.TokenStartIndex == lowestIndex.Item1.TokenStartIndex) //in the case we have a token index collision, we take the longer token as it is more "specific" than the shorter token and likely contains the shorter token.
                {
                    if (instance.Instance.Value.Length > lowestIndex.Item1.Value.Length)
                    {
                        lowestIndex = instance;
                    }
                }
            }

            //didn't find anything, return nothing.
            if (lowestIndex.Item1 == null) return null;

            //set the last index we iterated to in the global tokens list for the token of the type that was found to serve as the starting point for the next token search.
            if (_lastTokenIndexes.TryGetValue(lowestIndex.Item1.TokenDefinition.ID, out int index) == false)
            {
                _lastTokenIndexes[lowestIndex.Item1.TokenDefinition.ID] = lowestIndex.Item2;
            }
            else
            {
                if (index < lowestIndex.Item2)
                {
                    _lastTokenIndexes[lowestIndex.Item1.TokenDefinition.ID] = lowestIndex.Item2;
                }
            }

            //make a new token relative to this context.
            return lowestIndex.Item1 with 
            { 
                Context = this, 
                TokenStartIndex = lowestIndex.Item1.TokenStartIndex - _absoluteOffset, 
                TokenEndIndex = lowestIndex.Item1.TokenEndIndex - _absoluteOffset 
            };
        }

        /// <summary>
        /// Fast-forwards through the token list without processing the tokens. Used to seek over child context contents once the parent context has completed its child.
        /// </summary>
        /// <param name="index">The absolute index (relative to the ParseSession) to seek to.</param>
        protected void SeekForward(int index)
        {
            foreach (var tokenType in ParseSession.DefinedTokens)
            {
                _lastTokenIndexes.TryGetValue(tokenType.ID, out int startingIndex);
                if (startingIndex < 0) startingIndex = 0;

                var nextToken = tokenType.GetNextToken(index, this, startingIndex);

                if (nextToken.Instance == null)
                {
                    _lastTokenIndexes[tokenType.ID] = nextToken.Index;
                }
            }
        }

        /// <summary>
        /// Overridable filter function to tell the parser to ignore certain tokens. By default, no tokens are ignored.
        /// </summary>
        /// <param name="token">The token to possibly ignore.</param>
        /// <returns></returns>
        protected virtual bool IsValidInContext(TokenInstance token)
        {
            return true;
        }

        /// <summary>
        /// Determines if a token starts a new context. By default, only IContextStarter-implementing TokenDefinitions can start new contexts.
        /// </summary>
        /// <param name="tokenInstance">The token instance that could start a new context.</param>
        /// <returns></returns>
        protected virtual bool StartsNewContext(TokenInstance tokenInstance)
        {
            if (tokenInstance.TokenDefinition is IContextStarter starter == false) return false;
            return true;
        }

        /// <summary>
        /// Determines if a token ends the current context. By default, only IContextEnder-implementing TokenDefifitions that matches the StartToken can end the current context.
        /// </summary>
        /// <param name="tokenInstance">The token that could end the current context.</param>
        /// <returns></returns>
        protected virtual bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (StartToken == null || tokenInstance.TokenDefinition is IContextEnder ender == false) return false;
            return (StartToken.TokenDefinition as IContextStarter)?.ContextStartKey == ender.ContextEndKey;
        }

        /// <summary>
        /// Creates a new context based on the type of token being passed in.
        /// </summary>
        /// <param name="startToken">The token that will be the StartToken of the new </param>
        /// <returns></returns>
        protected abstract TokenParseContextBase CreateNewContext(TokenInstance startToken);

        /// <summary>
        /// Gets a string formatted in such a way that it shows the name of the context and its current depth as well as the names of the contexts that sit above it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
           
            string contextName = (String.IsNullOrWhiteSpace(Name) == true) ? GetType().Name : Name;
            string name = contextName;

            var parent = Parent;
            while (parent != null)
            {
                string parentName = parent.Name;
                if (String.IsNullOrWhiteSpace(parentName) == true) parentName = GetType().Name;

                name = parentName + ">" + name;
                parent = parent.Parent;
            }

            return $"({contextName}[{Depth}]) " + name;
        }
    }
}
