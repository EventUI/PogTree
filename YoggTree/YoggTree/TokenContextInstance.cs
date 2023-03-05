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
using YoggTree.Core.Spools;
using static System.Collections.Specialized.BitVector32;

namespace YoggTree
{
    /// <summary>
    /// Represents a section or sub-section of a file being parsed for tokens.
    /// </summary>
    public sealed class TokenContextInstance : IContentSpan
    {
        private Guid _id = Guid.NewGuid();
        private int _absoluteOffset = 0; //the offset that this context starts at relative to the ParseSession
        private int _currentIndex = 0;

        private List<TokenContextInstance> _childContexts = new List<TokenContextInstance>();
        private IReadOnlyList<TokenContextInstance> _childContextsRO = null; //read-only wrapper to expose as the child contexts list.

        private List<TokenInstance> _tokens = new List<TokenInstance>();
        private IReadOnlyList<TokenInstance> _tokensRO = null; //read-only wrapper to expose as the tokens list.


        /// <summary>
        /// The unique ID of this Context.
        /// </summary>
        public TokenContextDefinition TokenContextDefinition { get; } = null;

        /// <summary>
        /// The ParseSession this context is a child of.
        /// </summary>
        public TokenParseSession ParseSession { get; set; } = null;

        /// <summary>
        /// The immediate parent context of this context.
        /// </summary>
        public TokenContextInstance Parent { get; } = null;

        /// <summary>
        /// The index in the parent context where this context starts.
        /// </summary>
        public int StartIndex { get; } = -1;

        /// <summary>
        /// The index in the parent context where this context ends.
        /// </summary>
        public int EndIndex { get; private set; } = -1;

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
        public ReadOnlyMemory<char> Contents { get; private set; } = null;

        /// <summary>
        /// The token that signaled the beginning of this context.
        /// </summary>
        public TokenInstance StartToken { get; } = null;

        /// <summary>
        /// The token that signaled the end of this context.
        /// </summary>
        public TokenInstance EndToken { get; private set; } = null;

        /// <summary>
        /// The token currently being processed by the TokenContextInstance.
        /// </summary>
        public TokenInstance CurrentToken { get; private set; } = null;

        /// <summary>
        /// Every instance of every TokenDefinition found in this context.
        /// </summary>
        public IReadOnlyList<TokenInstance> Tokens { get { return _tokensRO; } }

        /// <summary>
        /// All of the child contexts that belong to this context.
        /// </summary>
        public IReadOnlyList<TokenContextInstance> ChildContexts { get { return _childContextsRO; } }

        internal TokenContextInstance(TokenContextDefinition contextDefinition, string contents)
        {
            Contents = new ReadOnlyMemory<char>(contents.ToCharArray());
            StartIndex = 0;
            EndIndex = Contents.Length;
            Parent = null;
            TokenContextDefinition = contextDefinition;

            _childContextsRO = _childContexts.AsReadOnly();
            _tokensRO = _tokens.AsReadOnly();
        }

        /// <summary>
        /// Creates a new sub-context that is the child of another context.
        /// </summary>
        /// <param name="parent">The context that is the parent of this one.</param>
        /// <param name="start">The token instance that signaled the beginning of this context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenContextInstance(TokenContextDefinition contextDefinition, TokenContextInstance parent, TokenInstance start)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (start == null) throw new ArgumentException(nameof(start));
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));

            TokenContextDefinition = contextDefinition;
            ParseSession = parent.ParseSession;
            Parent = parent;
            StartIndex = start.StartIndex;
            StartToken = start;
            Contents = parent.Contents.Slice(StartIndex);
            Depth = parent.Depth + 1;

            _absoluteOffset = parent._absoluteOffset + start.StartIndex;
            _childContextsRO = _childContexts.AsReadOnly();
            _tokensRO = _tokens.AsReadOnly();

            _tokens.Add(start with { Context = this, StartIndex = start.StartIndex - _absoluteOffset, EndIndex = start.EndIndex - _absoluteOffset });
        }

        /// <summary>
        /// Walks the Content of this Context, identifying tokens contained within and recursively spawning child contexts when such a token is encountered.
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void WalkContent()
        {
            TokenInstance _previousToken = (_tokens.Count > 0) ? _tokens[0] : null;

            while (_currentIndex < Contents.Length)
            {
                TokenInstance nextToken = GetNextToken();
                CurrentToken = nextToken;

                if (nextToken == null) break;

                //slot for enforcing token syntax rules - if one token cannot come before or after another token without being invalid, this will stop the whole parse operation
                if (_previousToken != null)
                {
                    if (nextToken.TokenDefinition.CanComeAfter(_previousToken) == false) throw new Exception($"{nextToken.ToString()} cannot come after {_previousToken.ToString()}");
                    if (_previousToken.TokenDefinition.CanComeBefore(nextToken) == false) throw new Exception($"{_previousToken.ToString()} cannot come before {nextToken.ToString()}");

                    _previousToken = nextToken;
                }

                //check to see if both the context allows this token to be valid, and that the token detects itself to be valid in this context. Skip if either is false.
                if (nextToken.TokenDefinition.IsValidInstance(nextToken) == false || TokenContextDefinition.IsValidInContext(nextToken) == false)
                {
                    _currentIndex = nextToken.EndIndex;
                    continue;
                }

                _tokens.Add(nextToken);

                //if this token meets the context's criteria for starting a new sub-context, make the context and walk its contents starting at the start token.
                //The new sub-context will eventually hit the block below to end itself and then we return to this context.
                if (TokenContextDefinition.StartsNewContext(nextToken) == true)
                {
                    TokenContextInstance childContext = TokenContextDefinition.CreateNewContext(nextToken);
                    if (childContext != null)
                    {
                        _childContexts.Add(childContext);
                        childContext.WalkContent();

                        _currentIndex = childContext.EndIndex;
                        if (childContext.EndToken != null) _tokens.Add(childContext.EndToken);
                        continue;
                    }
                }

                //if this token ends this context, snip the contents down to only what was inside the context and return to the parent context.
                if (TokenContextDefinition.EndsCurrentContext(nextToken) == true)
                {
                    Contents = Contents.Slice(0, nextToken.EndIndex);

                    _currentIndex = nextToken.EndIndex;
                    EndIndex = StartIndex + _currentIndex;
                    EndToken = nextToken;

                    break;
                }
                
                _currentIndex = nextToken.EndIndex;
            }

            if (EndToken == null && Depth > 0) throw new Exception("Failed to find an EndToken for context: " + ToString());
        }

        /// <summary>
        /// Gets the next token in this context.
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetNextToken()
        {
            SpooledResult firstResult = null;
            TokenSpool firstSpool = null;

            foreach (var tokenDefinition in TokenContextDefinition.ValidTokens)
            {
                if (ParseSession.TokenSpools.TryGetValue(tokenDefinition.ID, out TokenSpool spool) == false)
                {
                    spool = new TokenSpool(tokenDefinition, 10);
                    ParseSession.TokenSpools.Add(tokenDefinition.ID, spool);
                }

                var nextInstance = spool.GetNextResult(_currentIndex + _absoluteOffset, ParseSession.Contents);
                if (nextInstance.IsEmpty() == true) continue;

                if (firstResult == null)
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (nextInstance.StartIndex < firstResult.StartIndex)
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (firstResult.StartIndex == nextInstance.StartIndex)
                {
                    if (nextInstance.Length > firstResult.Length)
                    {
                        firstResult = nextInstance;
                        firstSpool = spool;
                    }
                }
            }

            if (firstResult.IsEmpty() == true) return null;
            firstSpool.CurrentIndex++;

            return new TokenInstance(firstSpool.Token, this, firstResult.StartIndex - _absoluteOffset, ParseSession.Contents.Slice(firstResult.StartIndex, firstResult.Length));
        }

        public int GetContextualIndex(TokenContextInstance targetContext)
        {
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (StartIndex < 0) throw new Exception("StartIndex must be a positive number.");

            bool parentFound = false;

            TokenContextInstance parent = Parent.Parent;

            while (parent != null)
            {
                if (parent == targetContext)
                {
                    parentFound = true;
                    break;
                }

                parent = parent.Parent;
            }

            if (parentFound != true) throw new Exception("TokenContextInstance is not contained by the target targetContext.");

            return StartIndex + targetContext.AbsoluteOffset;
        }

        public int GetAbsoluteIndex()
        {
            if (Parent?.ParseSession == null) throw new Exception("TokenContextInstance does not belong to a parse session, cannot calculate absolute index.");
            return GetContextualIndex(Parent.ParseSession.RootContext);
        }

        public ReadOnlyMemory<char> GetContents(IContentSpan start, IContentSpan end = null)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            return GetContents(start.GetAbsoluteIndex(), end?.GetAbsoluteIndex() + (end?.Contents.Length));
        }

        public ReadOnlyMemory<char> GetContents(IContentSpan start, int? end = null)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            return GetContents(start.GetAbsoluteIndex(), end + _absoluteOffset);
        }

        public ReadOnlyMemory<char> GetContents(int start, int? end = null)
        {
            if (start < 0 || start > Contents.Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (end.HasValue && end.Value > Contents.Length) throw new ArgumentOutOfRangeException(nameof(end));

            if (end.HasValue == true)
            {
                return Contents.Slice(start, end.Value - start);
            }
            else
            {
                return Contents.Slice(start);
            }
        }

        /// <summary>
        /// Gets a string formatted in such a way that it shows the name of the context and its current depth as well as the names of the contexts that sit above it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            string contextName = TokenContextDefinition.Name;
            string name = contextName;

            var parent = Parent;
            while (parent != null)
            {
                string parentName = parent.TokenContextDefinition.Name;
                if (string.IsNullOrWhiteSpace(parentName) == true) parentName = GetType().Name;

                name = parentName + ">" + name;
                parent = parent.Parent;
            }

            return $"({contextName}[{Depth}]) " + name;
        }

        public TokenContextInstance GetContext()
        {
            return Parent;
        }
    }
}
