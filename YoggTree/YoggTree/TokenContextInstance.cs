/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using YoggTree.Core.Spools;

namespace YoggTree
{
    /// <summary>
    /// Represents a section or sub-section of a file being parsed for tokens.
    /// </summary>
    public sealed class TokenContextInstance
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
        public TokenContextDefinition ContextDefinition { get; } = null;

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

        /// <summary>
        /// Internal only constructor that creates the root level context for a ParseSession.
        /// </summary>
        /// <param name="contextDefinition">The definition used for the root context of the ParseSession.</param>
        /// <param name="contents">The entire content string being parsed in the ParseSession.</param>
        internal TokenContextInstance(TokenContextDefinition contextDefinition, string contents)
        {
            Contents = new ReadOnlyMemory<char>(contents.ToCharArray());
            StartIndex = 0;
            EndIndex = Contents.Length;
            Parent = null;
            ContextDefinition = contextDefinition;

            _childContextsRO = _childContexts.AsReadOnly();
            _tokensRO = _tokens.AsReadOnly();
        }

        /// <summary>
        /// Internal only constructor that creates a new sub-context that is the child of another context.
        /// </summary>
        /// <param name="parent">The context that is the parent of this one.</param>
        /// <param name="start">The token instance that signaled the beginning of this context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal TokenContextInstance(TokenContextDefinition contextDefinition, TokenContextInstance parent, TokenInstance start)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (start == null) throw new ArgumentException(nameof(start));
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));

            ContextDefinition = contextDefinition;
            ParseSession = parent.ParseSession;
            Parent = parent;
            StartIndex = start.StartIndex;
            StartToken = start with { Context = this, StartIndex = 0, EndIndex = start.Contents.Length };
            Contents = parent.Contents.Slice(StartIndex);
            Depth = parent.Depth + 1;

            _absoluteOffset = parent._absoluteOffset + start.StartIndex;
            _childContextsRO = _childContexts.AsReadOnly();
            _tokensRO = _tokens.AsReadOnly();
            _tokens.Add(StartToken);
        }

        /// <summary>
        /// Walks the Content of this Context, identifying tokens contained within and recursively spawning child contexts when such a token is encountered.
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void WalkContent()
        {
            TokenInstance previousToken = (_tokens.Count > 0) ? _tokens[0] : null;
            TokenInstance textContentToken = null;
            TokenInstance currentToken = null;
            while (_currentIndex < Contents.Length)
            {
                TokenInstance nextToken = null;
                
                if (textContentToken != null)
                {
                    nextToken = currentToken;

                    currentToken = null;
                    textContentToken = null;
                }
                else
                {
                    nextToken = GetNextToken();
                }

                //get the start and end of the last two tokens to see if there is a gap between them. If there is, we need to make a TextContent token to fill in the gap.
                int textContentEndIndex = (nextToken == null) ? Contents.Length : nextToken.StartIndex;
                int textContentStartIndex = (previousToken == null || _currentIndex > previousToken?.EndIndex) ? _currentIndex : previousToken.EndIndex;

                if (textContentStartIndex < textContentEndIndex)
                {
                    currentToken = nextToken;
                    textContentToken = new TokenInstance(StandardTokens.TextContent, this, textContentStartIndex, Contents.Slice(textContentStartIndex, textContentEndIndex - textContentStartIndex));

                    nextToken = textContentToken;
                }

                CurrentToken = nextToken;

                if (nextToken == null) break;

                //slot for enforcing token syntax rules - if one token cannot come before or after another token without being invalid, this will stop the whole parse operation
                if (previousToken != null)
                {
                    if (nextToken.TokenDefinition.CanComeAfter(previousToken) == false) throw new Exception($"{nextToken.ToString()} cannot come after {previousToken.ToString()}");
                    if (previousToken.TokenDefinition.CanComeBefore(nextToken) == false) throw new Exception($"{previousToken.ToString()} cannot come before {nextToken.ToString()}");
                }

                previousToken = nextToken;

                //check to see if both the context allows this token to be valid, and that the token detects itself to be valid in this context. Skip if either is false.
                if (nextToken.TokenDefinition.IsValidInstance(nextToken) == false)
                {
                    _currentIndex = nextToken.EndIndex;
                    continue;
                }

                _tokens.Add(nextToken);

                //if this token meets the context's criteria for starting a new sub-context, make the context and walk its contents starting at the start token.
                //The new sub-context will eventually hit the block below to end itself and then we return to this context.
                if (ContextDefinition.StartsNewContext(nextToken) == true)
                {
                    TokenContextInstance childContext = ContextDefinition.CreateNewContext(nextToken);
                    if (childContext != null)
                    {
                        //replace the token in the array with one that is flagged as starting a context instance.
                        _tokens[_tokens.Count -1] = nextToken with { StartedContextInstance = childContext };

                        _childContexts.Add(childContext);
                        childContext.WalkContent();

                        _currentIndex = childContext.EndIndex;

                        if (childContext.EndToken != null)
                        {
                            int delta = childContext.AbsoluteOffset - AbsoluteOffset;
                            previousToken = childContext.EndToken with { Context = this, StartIndex = childContext.EndToken.StartIndex + delta, EndIndex = childContext.EndToken.EndIndex + delta };
                            
                            _tokens.Add(previousToken);
                        }

                        continue;
                    }
                }

                //if this token ends this context, snip the contents down to only what was inside the context and return to the parent context.
                if (ContextDefinition.EndsCurrentContext(nextToken) == true)
                {
                    Contents = Contents.Slice(0, nextToken.EndIndex);

                    _currentIndex = nextToken.EndIndex;
                    EndIndex = StartIndex + _currentIndex;
                    EndToken = nextToken;

                    break;
                }
                
                _currentIndex = nextToken.EndIndex;
            }
        }

        /// <summary>
        /// Gets the next token in this context. Note that this assumes a forward-only sweep of the content and can't be rewound. 
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetNextToken()
        {
            SpooledResult firstResult = null;
            TokenSpool firstSpool = null;

            //walk the list of every token definition that was included to the context definition and find the one that occurs next in the content string.
            foreach (var tokenDefinition in ContextDefinition.ValidTokens)
            {
                //see if we have a spool that's already been made for this type of token. If not, make and add it.
                if (ParseSession.TokenSpools.TryGetValue(tokenDefinition.ID, out TokenSpool spool) == false)
                {
                    spool = new TokenSpool(tokenDefinition, tokenDefinition.SpoolSize);
                    ParseSession.TokenSpools.Add(tokenDefinition.ID, spool);
                }

                //get the next instance of the token in the content string for the entire parse session.
                var nextInstance = spool.GetNextResult(_currentIndex + _absoluteOffset, ParseSession.Contents);
                if (nextInstance.IsEmpty() == true) continue;

                if (firstResult == null) //first thing we found, set it as the earliest result
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (nextInstance.StartIndex < firstResult.StartIndex) //a subsequent result began earlier than the current earliest match. Use it instead.
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (firstResult.StartIndex == nextInstance.StartIndex) //two tokens found at the same index - take whichever is longer as that is a more "specific" match and that it also contains the shorter token. NOTE: may want to add a hook to resolve this ambiguity here if we run into problems with this rule
                {
                    if (nextInstance.Length > firstResult.Length)
                    {
                        firstResult = nextInstance;
                        firstSpool = spool;
                    }
                }
            }
         
            if (firstResult.IsEmpty() == true) return null; //no more results. All done.

            //a result was found - advance the index of the spool so it doesn't redundantly search itself for the next result. Note this will always be incremented by one as the loop above only takes the first token from each spool to find the earliest one.
            firstSpool.CurrentIndex++;

            return new TokenInstance(firstSpool.Token, this, firstResult.StartIndex - _absoluteOffset, ParseSession.Contents.Slice(firstResult.StartIndex, firstResult.Length));
        }

        /// <summary>
        /// Gets a slice of this context instance's contents.
        /// </summary>
        /// <param name="start">The starting index.</param>
        /// <param name="end">The ending index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ReadOnlyMemory<char> Slice(int start, int? end = null)
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

            string contextName = ContextDefinition.Name;
            string name = contextName;

            var parent = Parent;
            while (parent != null)
            {
                string parentName = parent.ContextDefinition.Name;
                if (string.IsNullOrWhiteSpace(parentName) == true) parentName = GetType().Name;

                name = parentName + ">" + name;
                parent = parent.Parent;
            }

            return $"({contextName}[{Depth}]) " + name;
        }
    }
}
