

using System.Data.Common;
using YoggTree.Core.Tokens;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/
namespace YoggTree
{
    /// <summary>
    /// Represents an instance of a TokenDefinition that was found in a TokenContextInstance's Content.
    /// </summary>
    public record TokenInstance
    {
        /// <summary>
        /// Internal only reference to the next token in the source string. Used to create a quasi-linked list between all tokens.
        /// </summary>
        internal TokenInstance NextToken = null;

        /// <summary>
        /// Internal only reference to the previous token in the source string. Used to create a quasi-linked list between all tokens.
        /// </summary>
        internal TokenInstance PreviousToken = null;

        /// <summary>
        /// The definition of the rules for the token.
        /// </summary>
        public TokenDefinition TokenDefinition { get; } = null;

        /// <summary>
        /// The targetContext in which the token was found.
        /// </summary>
        public TokenContextInstance Context { get; internal set; } = null;

        /// <summary>
        /// The index at which the token begins.
        /// </summary>
        public int StartIndex { get; internal set; } = -1;

        /// <summary>
        /// The value of the token that was found.
        /// </summary>
        public ReadOnlyMemory<char> Contents { get; } = null;

        /// <summary>
        /// The ending index of the token.
        /// </summary>
        public int EndIndex { get; internal set; } = -1;

        /// <summary>
        /// The type of instance this token represents.
        /// </summary>
        public TokenInstanceType TokenInstanceType { get; } = TokenInstanceType.None;

        /// <summary>
        /// Internal constructor used to make a new TokenInstance.
        /// </summary>
        /// <param name="tokenDefinition">The definition of the token that was found.</param>
        /// <param name="context">The context in whichi the token was found.</param>
        /// <param name="tokenStartIndex">The start index of the token in its context.</param>
        /// <param name="value">The value of the token.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal TokenInstance(TokenDefinition tokenDefinition, TokenContextInstance context, int tokenStartIndex, ReadOnlyMemory<char> value, TokenInstanceType instanceType)
        {
            if (tokenDefinition == null) throw new ArgumentNullException("token");
            if (context == null) throw new ArgumentNullException("targetContext");
            if (tokenStartIndex < 0) throw new ArgumentOutOfRangeException(nameof(tokenStartIndex));
            if (value.IsEmpty == true) throw new ArgumentException("value cannot be empty.");

            TokenDefinition = tokenDefinition;
            Context = context;
            StartIndex = tokenStartIndex;
            Contents = value;
            EndIndex = StartIndex + value.Length;
            TokenInstanceType = instanceType;
        }        

        public override string ToString()
        {
            return $"{TokenDefinition.GetType().Name} @:{StartIndex}=\"{Contents}\"";
        }
    }

    /// <summary>
    /// Represents an instance of a token that wraps a child TokenContextInstance.
    /// </summary>
    public record ChildContextTokenInstance : TokenInstance
    {
        public TokenContextInstance ChildContext { get; internal init; } = null;

        internal ChildContextTokenInstance(TokenContextInstance context, int tokenStartIndex, ReadOnlyMemory<char> value, TokenContextInstance childContext)
            : base(StandardTokens.Empty, context, tokenStartIndex, value, TokenInstanceType.ContextPlaceholder)
        {
            ChildContext = childContext;
        }

        public override string ToString()
        {
            return $"<context placeholder> @:{StartIndex}=\"{Contents}\"";
        }
    }

    /// <summary>
    /// Represents an instance of a token that is a run of text between other tokens.
    /// </summary>
    public record TextPlacehodlerTokenInstance : TokenInstance
    { 
        internal TextPlacehodlerTokenInstance(TokenContextInstance context, int tokenStartIndex, ReadOnlyMemory<char> value)
            :base (StandardTokens.TextContent, context, tokenStartIndex, value, TokenInstanceType.TextPlaceholder)
        {

        }

        public override string ToString()
        {
            return $"<text placeholder> @:{StartIndex}=\"{Contents}\"";
        }
    }

    public static class TokenInstanceExtensions
    {
        /// <summary>
        /// If the TokenInstance is a ChildContextTokenInstance, this will return the TokenContextInstance being wrapped.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenContextInstance GetChildContext(this TokenInstance instance)
        {
            return (instance as ChildContextTokenInstance)?.ChildContext;
        }

        /// <summary>
        /// Gets the full text content of the TokenInstance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string GetText(this TokenInstance instance)
        {
            return (instance == null) ? null : instance.Contents.ToString();
        }

        /// <summary>
        /// Gets a clone of this token that is relative to a different (parent) context.
        /// </summary>
        /// <param name="targetContext"></param>
        /// <returns></returns>
        public static TokenInstance GetContextualInstance(this TokenInstance instance, TokenContextInstance targetContext)
        {
            int contextualStartIndex = instance.GetContextualStartIndex(targetContext);
            int delta = contextualStartIndex - instance.StartIndex;

            return instance with { Context = targetContext, EndIndex = instance.EndIndex + delta, StartIndex = instance.StartIndex + delta };
        }

        /// <summary>
        /// Gets a clone of this token that is relative to the entire parse session.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TokenInstance GetAbsoluteInstance(this TokenInstance instance)
        {
            if (instance?.Context?.ParseSession == null) throw new Exception("Token does not belong to a parse session, cannot get absolute instance.");
            return instance.GetContextualInstance(instance?.Context?.ParseSession?.RootContext);
        }

        /// <summary>
        /// Gets the token that follows this one.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenInstance GetNextToken(this TokenInstance instance)
        {
            if (instance == null) return null;
            return instance.Context.GetNextTokenInstance(instance, false);
        }

        /// <summary>
        /// Gets the token that came before this one.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static TokenInstance GetPreviousToken(this TokenInstance instance)
        {
            if (instance == null) return null;
            return instance.PreviousToken;
        }

        /// <summary>
        /// Determines if the TokenDefintion of this instance satisfy's the "is" operator as a type check.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool Is<T>(this TokenInstance instance) where T : TokenDefinition
        {
            return instance?.TokenDefinition is T;
        }

        /// <summary>
        /// Gets the start index of this token in another context.
        /// </summary>
        /// <param name="targetContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        internal static int GetContextualStartIndex(this TokenInstance instance, TokenContextInstance targetContext)
        {
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));

            if (instance == null) return -1;
            if (instance.StartIndex < 0) throw new Exception("StartIndex must be a positive number.");

            return instance.StartIndex + (instance.Context.AbsoluteOffset - targetContext.AbsoluteOffset);

            TokenContextInstance currentContext = instance.Context;
            if (currentContext.Depth == targetContext.Depth)
            {
                if (currentContext == targetContext)
                {
                    return instance.StartIndex;
                }
                else
                {
                    var previousContext = targetContext.PreviousContext;
                    if (previousContext != null)
                    {
                        return previousContext.EndIndex + instance.StartIndex;
                    }
                }
            }


            int delta = currentContext.AbsoluteOffset- targetContext.AbsoluteOffset;
            return instance.StartIndex + delta;
        }

        /// <summary>
        /// Gets the line number and column number of the start of the token relative to the entire string being parsed.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static (int LineNumber, int ColumnNumber) GetLineAndColumn(this TokenInstance instance)
        {
            if (instance == null) return (-1, -1);
            
            int column = -1;
            int line = 0;

            foreach (var result in TokenRegexStore.Whitespace_Vertical.EnumerateMatches(instance.Context.ParseSession.Contents.Span))
            {
                if (result.Index < instance.StartIndex)
                {
                    line++;
                }
                else
                {
                    column = instance.StartIndex - (result.Index + result.Length);
                    break;
                }
            }
            
            if (line == 0)
            {
                column = instance.StartIndex;
            }

            return (line, column);
        }
    }
}
