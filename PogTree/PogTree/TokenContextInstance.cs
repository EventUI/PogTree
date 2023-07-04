/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Data;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using PogTree.Core.Enumerators;
using PogTree.Core.Exceptions;
using PogTree.Core.Spools;

namespace PogTree
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
        /// <param name="contextDefinition">The definition of the rules for this context instance.</param>
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
            int absoluteStart = 0;

            if (nextToken != null) //we already discovered the next token in the content string
            {
                //if we have a token that has a different context, is not included in a context, and is a regular regex result, we may have a different token be the "true" next token and the next token we already have points to a location past the "true" next token, so we go back and look for it
                if (nextToken.Context != this || (nextToken.TokenInstanceType == TokenInstanceType.RegexResult && ContextDefinition.HasToken(nextToken.TokenDefinition.GetType()) == false))
                {
                    absoluteStart = nextToken.StartIndex + nextToken.Context.AbsoluteOffset;
                    if (absoluteStart < 0) absoluteStart = 0;

                    bool foundEarlierToken = false;
                    var potentialNextToken = GetNextToken(previousToken.EndIndex + previousToken.Context.AbsoluteOffset);
                    if (potentialNextToken != null)
                    {
                        int nextTokenAbsoluteStart = potentialNextToken.StartIndex + AbsoluteOffset;
                        if (nextTokenAbsoluteStart < absoluteStart || (nextTokenAbsoluteStart == absoluteStart && potentialNextToken.Contents.Length > nextToken.Contents.Length)) //if this token starts before the contextually adjusted start index of the next token AND doesn't overlap that token, its the true next token
                        {
                            potentialNextToken.NextToken = nextToken;
                            nextToken = potentialNextToken;

                            absoluteStart = nextToken.StartIndex + nextToken.Context.AbsoluteOffset;
                            foundEarlierToken = true;
                        }
                    }
                    else if (nextToken.TokenInstanceType != TokenInstanceType.RegexResult) //if we didn't find another token in this context and the previous token was some kind of placeholder, we don't have a real token to continue with
                    {
                        nextToken = null;
                        foundEarlierToken = true;
                    }

                    if (nextToken?.TokenInstanceType == TokenInstanceType.RegexResult && ContextDefinition.HasToken(nextToken) == false) //we found a token in the new context, but it came after the current "next" token - however, it's not in this context so we use the other token instead.
                    {
                        nextToken = potentialNextToken;
                        absoluteStart = nextToken.StartIndex + nextToken.Context.AbsoluteOffset;
                        foundEarlierToken = true;
                    }

                    if (setContext == true && foundEarlierToken == false) //if setting the context, we need to shift the start index of the token to be relative to its current context
                    {
                        if (Tokens.Count > 0)
                        {
                            absoluteStart = Tokens[Tokens.Count - 1].EndIndex;
                        }
                        else
                        {
                            absoluteStart = StartToken.EndIndex;
                        }

                        nextToken.Context = this;
                        nextToken.StartIndex = absoluteStart;
                        nextToken.EndIndex = absoluteStart + nextToken.Contents.Length;

                        return nextToken;
                    }
                }
                else
                {
                    if (setContext == true) //if setting the context, we need to shift the start index of the token to be relative to its current context.
                    {
                        if (nextToken.Context != this)
                        {
                            if (Tokens.Count > 0)
                            {
                                absoluteStart = Tokens[Tokens.Count - 1].EndIndex;
                            }
                            else
                            {
                                absoluteStart = StartToken.EndIndex;
                            }

                            nextToken.Context = this;
                            nextToken.StartIndex = absoluteStart;
                            nextToken.EndIndex = absoluteStart + nextToken.Contents.Length;
                        }
                    }

                    return nextToken;
                }
            }
            else //need to get another token from the regex spools
            {
                nextToken = GetNextToken(previousToken == null ? 0 : previousToken.EndIndex + previousToken.Context.AbsoluteOffset);
                if (nextToken != null && nextToken.StartIndex == previousToken?.StartIndex && nextToken.Contents.Length == previousToken?.Contents.Length)
                {
                    nextToken = null;
                }

                absoluteStart = (nextToken == null) ? 0 : nextToken.StartIndex + nextToken.Context.AbsoluteOffset;
            }

            //get the start and end of the last two tokens to see if there is a gap between them. If there is, we need to make a TextContentToken token to fill in the gap.
            int textContentEndIndex = (nextToken == null) ? Contents.Length + AbsoluteOffset : absoluteStart;
            int textContentStartIndex = (previousToken == null) ? 0 : previousToken.EndIndex + previousToken.Context.AbsoluteOffset;

            if (textContentStartIndex < textContentEndIndex) //there's a gap - make a text placeholder token
            {
                var textContentToken = new TextPlaceholderTokenInstance(this, textContentStartIndex - AbsoluteOffset, ParseSession.Contents.Slice(textContentStartIndex, textContentEndIndex - textContentStartIndex));

                if (previousToken != null) previousToken.NextToken = textContentToken;
                textContentToken.PreviousToken = previousToken;
                textContentToken.NextToken = nextToken;

                return textContentToken;
            }
            else //no gap, regex result token is fine as-is
            {
                if (nextToken == null) return null; //no subsequent token, all done

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
                    if (nextToken.TokenDefinition.CanComeAfter(previousToken, nextToken) == false)
                    {
                        var lineAndCol = nextToken.GetLineAndColumn();
                        throw new TokenSyntaxErrorException($"{nextToken} cannot come after {previousToken}.\nLine: {lineAndCol.LineNumber} Column: {lineAndCol.ColumnNumber}", lineAndCol.LineNumber, lineAndCol.ColumnNumber);                       
                    }

                    if (previousToken.TokenDefinition.CanComeBefore(nextToken, previousToken) == false) 
                    {
                        var lineAndCol = nextToken.GetLineAndColumn();
                        throw new TokenSyntaxErrorException($"{previousToken} cannot come before {nextToken}.\nLine: {lineAndCol.LineNumber} Column: {lineAndCol.ColumnNumber}", lineAndCol.LineNumber, lineAndCol.ColumnNumber);
                    }
                }

                //check to see if the token definition allows this token to be valid in its current context, fail if it is not
                if (nextToken.TokenDefinition.IsValidInstance(nextToken) == false)
                {
                    var lineAndCol = nextToken.GetLineAndColumn();
                    throw new TokenSyntaxErrorException($"{nextToken} is not valid in context {this}.\nLine: {lineAndCol.LineNumber} Column: {lineAndCol.ColumnNumber}", lineAndCol.LineNumber, lineAndCol.ColumnNumber);
                }

                //if this token meets the context's criteria for starting a new sub-context, make the context and walk its contents starting at the start token.
                //The new sub-context will eventually hit the block below to end itself and then we return to this context.
                if (ContextDefinition.StartsNewContext(nextToken) == true)
                {
                    TokenContextInstance childContext = ContextDefinition.CreateNewContext(nextToken);
                    if (childContext != null)
                    {
                        _childContexts.Add(childContext);

                        //hook up the context's linked lists
                        childContext.PreviousContext = previousContext;
                        if (previousContext != null) previousContext.NextContext = childContext;

                        //advance the "previous" context to be the current one
                        previousContext = childContext;

                        //recursively discover or re-assign tokens to the new sub-context
                        childContext.WalkContent();

                        //once the child context is done being processed, make a placeholder to put into this context's list of tokens.
                        var contextPlaceholder = new ContextPlaceholderTokenInstance(this, childContext.StartIndex, childContext.Contents, childContext);

                        //insert the context placeholder between the previous and "next" tokens in the token list
                        contextPlaceholder.PreviousToken = previousToken;

                        //the logical "next" token is actually whatever follows the end token of the child context because we don't know how many tokens forward the child context (or its contexts) have brought us
                        contextPlaceholder.NextToken = childContext.EndToken?.NextToken;

                        //if we had a previous token, it's new "next" token is the placeholder
                        if (previousToken != null) previousToken.NextToken = contextPlaceholder;

                        //previousToken.NextToken = contextPlaceholder;
                        _tokens.Add(contextPlaceholder);

                        _currentIndex = contextPlaceholder.EndIndex;
                        previousToken = contextPlaceholder;

                        continue;
                    }
                }

                _tokens.Add(nextToken);

                //if this token ends this context, snip the contents down to only what was inside the context and return to the parent context.
                if (ContextDefinition.EndsCurrentContext(nextToken) == true)
                {
                    Contents = Contents.Slice(0, nextToken.EndIndex);

                    _currentIndex = nextToken.EndIndex;
                    EndIndex = StartIndex + _currentIndex;
                    EndToken = nextToken;

                    nextToken.PreviousToken = previousToken;
                    if (previousToken != null) previousToken.NextToken = nextToken;

                    break;
                }

                _currentIndex = nextToken.EndIndex;

                nextToken.PreviousToken = previousToken;
                if (previousToken != null) previousToken.NextToken = nextToken;

                previousToken = nextToken;
            }

            if (EndToken == null)
            {
                if (ContextDefinition.Flags.HasFlag(ContextDefinitionFlags.Unbounded) || Parent == null) return;
                throw new UnexpectedEndOfContentException($"Context {this} had no end token and was not flagged as Unbounded or was not the root of the ParseSession.");
            }
        }

        /// <summary>
        /// Gets the next token in this context. Note that this assumes a forward-only sweep of the content and can't be rewound. 
        /// </summary>
        /// <returns></returns>
        private TokenInstance GetNextToken(int startingIndex)
        {
            (SpooledResult SpooledResult, int Index) firstResult = default;
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
                (SpooledResult SpooledResult, int Index) nextInstance = spool.GetNextResult(startingIndex, ParseSession.Contents);
                if (nextInstance.SpooledResult.IsEmpty() == true || nextInstance.SpooledResult.StartIndex < startingIndex) continue;

                if (firstResult.SpooledResult == null) //first thing we found, set it as the earliest result
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (nextInstance.SpooledResult.StartIndex < firstResult.SpooledResult.StartIndex) //a subsequent result began earlier than the current earliest match. Use it instead.
                {
                    firstResult = nextInstance;
                    firstSpool = spool;
                }
                else if (firstResult.SpooledResult.StartIndex == nextInstance.SpooledResult.StartIndex) //two tokens found at the same index - take whichever is longer as that is a more "specific" match and that it also contains the shorter token. NOTE: may want to add a hook to resolve this ambiguity here if we run into problems with this rule
                {
                    if (nextInstance.SpooledResult.Length > firstResult.SpooledResult.Length)
                    {
                        firstResult = nextInstance;
                        firstSpool = spool;
                    }
                }
            }
         
            if (firstResult.SpooledResult.IsEmpty() == true) return null; //no more results. All done.

            //a result was found - advance the index of the spool so it doesn't redundantly search itself for the next result. Note this will always be incremented by one as the loop above only takes the first token from each spool to find the earliest one.
            firstSpool.CurrentSpoolIndex = firstResult.Index;
            firstSpool.CurrentContentIndex = firstResult.SpooledResult.StartIndex + firstResult.SpooledResult.Length;

            return new TokenInstance(firstSpool.Token, this, firstResult.SpooledResult.StartIndex - _absoluteOffset, ParseSession.Contents.Slice(firstResult.SpooledResult.StartIndex, firstResult.SpooledResult.Length), TokenInstanceType.RegexResult);
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

        /// <summary>
        /// Gets all the text between two tokens using the start token's root context.
        /// </summary>
        /// <param name="startToken">The token at which to start getting content.</param>
        /// <param name="endToken">Optional. The token to stop getting content at. A null value gets the rest of the context's string.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetText(TokenInstance startToken, TokenInstance endToken = null)
        {
            if (startToken == null) throw new ArgumentNullException(nameof(startToken));

            if (endToken != null && endToken.Context.ParseSession.RootContext != startToken.Context.ParseSession.RootContext)
            {
                throw new Exception("Start and end tokens do not belong to the same root context.");
            }

            int contentStart = startToken.GetContextualStartIndex(startToken.Context.ParseSession.RootContext);
            int contentEnd = (endToken == null) ? startToken.Context.ParseSession.RootContext.Contents.Length : endToken.GetContextualStartIndex(startToken.Context.ParseSession.RootContext);

            return startToken.Context.ParseSession.RootContext.Contents.Slice(contentStart, contentEnd - contentStart).ToString();
        }
    }

    /// <summary>
    /// Extension methods for TokenContextInstances.
    /// </summary>
    public static class TokenContextInstanceExtensions
    {
        /// <summary>
        /// Gets the next peer context to the current context.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenContextInstance PeekNextContext(this TokenContextInstance instance)
        {
            if (instance == null) return null;
            return instance.NextContext;
        }

        /// <summary>
        /// Gets the previous peer context to the current context.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenContextInstance PeekPreviousContext(this TokenContextInstance instance)
        {
            if (instance == null) return null;  
            return instance.PreviousContext;
        }

        /// <summary>
        /// Determines if the TokenContextDefinition satisfies the "is" operator for the given type.
        /// </summary>
        /// <typeparam name="T">The type of TokenContextDefinition to check against.</typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool Is<T>(this TokenContextInstance instance) where T : ITokenContextDefinition
        {
            return instance?.ContextDefinition is T;
        }

        /// <summary>
        /// Determines whether or not a context instance is inside of a context instance of the given definition.
        /// </summary>
        /// <typeparam name="T">The context definition to check against.</typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsIn<T>(this TokenContextInstance instance) where T : ITokenContextDefinition
        {
            if (instance == null) return false;
            if (instance.Parent.ContextDefinition is T) return true;

            return false;
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

        /// <summary>
        /// Gets a TokenContextReader that can be used to iterate through the tokens in this context.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenReader GetReader(this TokenContextInstance instance)
        {
            if (instance == null) return null;

            return new TokenReader(instance);
        }
    }
}
