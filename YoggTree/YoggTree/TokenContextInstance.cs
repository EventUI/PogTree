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

        internal TokenContextInstance PreviousContext = null;
        internal TokenContextInstance NextContext = null;

        /// <summary>
        /// The ID of this context instance.
        /// </summary>
        public Guid ID
        {
            get { return _id; }
        }

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
        public TokenInstance StartToken { get; private set; } = null;

        /// <summary>
        /// The token that signaled the end of this context.
        /// </summary>
        public TokenInstance EndToken { get; private set; } = null;

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
           _absoluteOffset = parent._absoluteOffset + start.StartIndex;

            ContextDefinition = contextDefinition;
            ParseSession = parent.ParseSession;
            Parent = parent;
            StartIndex = start.StartIndex;
            StartToken = start;
            StartToken.Context = this;
            StartToken.StartIndex = 0;
            StartToken.EndIndex = start.Contents.Length;  // with { Context = this, StartIndex = 0, EndIndex = start.Contents.Length };
            Contents = parent.Contents.Slice(StartIndex);
            Depth = parent.Depth + 1;

 
            _childContextsRO = _childContexts.AsReadOnly();
            _tokensRO = _tokens.AsReadOnly();
            _tokens.Add(StartToken);
        }

        /// <summary>
        /// Gets the next TokenInstance. If used while the string is being parsed, this will continue to get tokens until the end of the string. If used after the string has been parsed, this will only return the tokens that were found within the bounds of the context.
        /// </summary>
        /// <param name="previousToken">The reference point to get the token after.</param>
        /// <param name="setContext">Whether or not, upon finding a new token, to set its context to be the current context. This is used in situations where the stream was advanced beyond the ending of the context from which the seeking was performed to ensure that the token is assigned to the correct containing context.</param>
        /// <returns></returns>
        internal TokenInstance GetNextTokenInstance(TokenInstance previousToken, bool setContext)
        {
            TokenInstance nextToken = previousToken == null ? null : previousToken.NextToken;

            if (nextToken != null)
            {
                if (setContext == true && nextToken.Context != this)
                {
                    var newStart = nextToken.GetContextualStartIndex(this);
                    nextToken.Context = this;
                    nextToken.StartIndex = newStart;
                    nextToken.EndIndex = newStart + nextToken.Contents.Length;
                }

                //if (nextToken.NextToken != null) return nextToken;
                //if (nextToken.TokenInstanceType != TokenInstanceType.RegexResult) return nextToken;

                return nextToken;
            }
            else
            {
                nextToken = GetNextToken(previousToken == null ? 0 : previousToken.EndIndex);
                if (nextToken != null && nextToken.StartIndex == previousToken?.StartIndex && nextToken.Contents.Length == previousToken?.Contents.Length)
                {
                    nextToken = null;
                }
            }

            //get the start and end of the last two tokens to see if there is a gap between them. If there is, we need to make a TextContentToken token to fill in the gap.
            int textContentEndIndex = (nextToken == null) ? Contents.Length : nextToken.StartIndex;
            int textContentStartIndex = (previousToken == null) ? 0 : previousToken.EndIndex;

            if (textContentStartIndex < textContentEndIndex)
            {
                var textContentToken = new TextPlacehodlerTokenInstance(this, textContentStartIndex, Contents.Slice(textContentStartIndex, textContentEndIndex - textContentStartIndex));

                if (previousToken != null) previousToken.NextToken = textContentToken;
                textContentToken.PreviousToken = previousToken;
                textContentToken.NextToken = nextToken;

                return textContentToken;
            }
            else
            {
                if (nextToken == null) return null;
                nextToken.PreviousToken = previousToken;
                if (previousToken != null) previousToken.NextToken = nextToken;
            }

            return nextToken;
        }

        /// <summary>
        /// Walks the content string looking for Regex token matches and recursively drilling down into new contexts as they are discovered.
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void WalkContent()
        {
            TokenInstance previousToken = StartToken;
            TokenContextInstance previousContext = PreviousContext;

            while (_currentIndex < Contents.Length)
            {
                TokenInstance nextToken = GetNextTokenInstance(previousToken, true);
                if (nextToken == null) break;

                //slot for enforcing token syntax rules - if one token cannot come before or after another token without being invalid, this will stop the whole parse operation
                if (previousToken != null)
                {
                    if (nextToken.TokenDefinition.CanComeAfter(previousToken) == false) throw new Exception($"{nextToken.ToString()} cannot come after {previousToken.ToString()}");
                    if (previousToken.TokenDefinition.CanComeBefore(nextToken) == false) throw new Exception($"{previousToken.ToString()} cannot come before {nextToken.ToString()}");
                }

                //check to see if the context allows this token to be valid, if not skip it
                if (nextToken.TokenDefinition.IsValidInstance(nextToken) == false)
                {
                    _currentIndex = nextToken.EndIndex;
                    previousToken = nextToken;

                    continue;
                }

                bool wasPlaceholder = false;

                //if this token meets the context's criteria for starting a new sub-context, make the context and walk its contents starting at the start token.
                //The new sub-context will eventually hit the block below to end itself and then we return to this context.
                if (ContextDefinition.StartsNewContext(nextToken) == true)
                {
                    TokenContextInstance childContext = ContextDefinition.CreateNewContext(nextToken);
                    if (childContext != null)
                    {
                        _childContexts.Add(childContext);

                        childContext.PreviousContext = previousContext;
                        if (previousContext != null) previousContext.NextContext = childContext;
                        previousContext = childContext;

                        childContext.WalkContent();

                        var contextPlaceholder = new ChildContextTokenInstance(this, childContext.StartIndex, childContext.Contents, childContext);
                        if (previousToken == null)
                        {
                            if (childContext.EndToken != null) previousToken = childContext.EndToken;
                        }

                        contextPlaceholder.NextToken = childContext.EndToken?.NextToken;
                        previousToken.NextToken = contextPlaceholder;
                        wasPlaceholder = true;

                        nextToken = contextPlaceholder;
                    }
                }

                _tokens.Add(nextToken);

                //if this token ends this context, snip the contents down to only what was inside the context and return to the parent context.
                if (wasPlaceholder == false && ContextDefinition.EndsCurrentContext(nextToken) == true)
                {
                    Contents = Contents.Slice(0, nextToken.EndIndex);

                    _currentIndex = nextToken.EndIndex;
                    EndIndex = StartIndex + _currentIndex;
                    EndToken = nextToken;

                    nextToken.PreviousToken = previousToken;
                    previousToken.NextToken = nextToken;

                    break;
                }

                _currentIndex = nextToken.EndIndex;
                nextToken.PreviousToken = previousToken;
                previousToken = nextToken;
            }
        }

        /// <summary>
        /// Gets the next token in this context. Note that this assumes a forward-only sweep of the content and can't be rewound. 
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetNextToken(int startingIndex)
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
                var nextInstance = spool.GetNextResult(startingIndex + _absoluteOffset, ParseSession.Contents);
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

            return new TokenInstance(firstSpool.Token, this, firstResult.StartIndex - _absoluteOffset, ParseSession.Contents.Slice(firstResult.StartIndex, firstResult.Length), TokenInstanceType.RegexResult);
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

    public static class TokenContextInstanceExtensions
    {
        /// <summary>
        /// Gets the next peer context to the current context.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenContextInstance GetNextContext(this TokenContextInstance instance)
        {
            if (instance == null) return null;
            return instance.NextContext;
        }

        /// <summary>
        /// Gets the previous peer context to the current context.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenContextInstance GetPreviousContext(this TokenContextInstance instance)
        {
            if (instance == null) return null;  
            return instance.PreviousContext;
        }

        /// <summary>
        /// Determines if the TokenContextDefintion satisfies the "is" operator for the given type.
        /// </summary>
        /// <typeparam name="T">The type of TokenContextDefintion to check against.</typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool Is<T>(this TokenContextInstance instance) where T : TokenContextDefinition
        {
            return instance?.ContextDefinition is T;
        }

        /// <summary>
        /// Gets the full text contents of the TokenContextInstance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string GetText(this TokenContextInstance instance)
        {
            return instance?.Contents.ToString();
        }
    }
}
