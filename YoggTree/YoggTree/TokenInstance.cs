using System.Runtime.CompilerServices;
using System.Xml.Linq;
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
        /// The definition of the rules for the token.
        /// </summary>
        public TokenDefinition TokenDefinition { get; } = null;

        /// <summary>
        /// The targetContext in which the token was found.
        /// </summary>
        public TokenContextInstance Context { get; internal init; } = null;

        /// <summary>
        /// The index at which the token begins.
        /// </summary>
        public int StartIndex { get; internal init; } = -1;

        /// <summary>
        /// The value of the token that was found.
        /// </summary>
        public ReadOnlyMemory<char> Contents { get; } = null;

        /// <summary>
        /// The ending index of the token.
        /// </summary>
        public int EndIndex { get; internal init; } = -1;

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
        /// Gets the start index of this token in another context.
        /// </summary>
        /// <param name="targetContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private static int GetContextualStartIndex(this TokenInstance instance, TokenContextInstance targetContext)
        {
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (instance.StartIndex < 0) throw new Exception("StartIndex must be a positive number.");

            bool parentFound = false;

            TokenContextInstance parent = instance.Context.Parent;

            while (parent != null)
            {
                if (parent == targetContext)
                {
                    parentFound = true;
                    break;
                }

                parent = parent.Parent;
            }

            if (parentFound != true) throw new Exception("TokenInstance is not contained by the target targetContext.");

            return instance.StartIndex + parent.AbsoluteOffset;
        }
    }
}
