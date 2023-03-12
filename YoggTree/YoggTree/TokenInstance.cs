using System.Xml.Linq;
/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree
{
    /// <summary>
    /// Represents an instance of a TokenDefinition that was found in a TokenContextInstance's Content.
    /// </summary>
    public sealed record TokenInstance
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
        /// If this token successfully started a new context as it was being processed, this was the context that was started and can be drilled into recursively. 
        /// </summary>
        public TokenContextInstance StartedContextInstance { get; internal init; } = null;

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
        internal TokenInstance(TokenDefinition tokenDefinition, TokenContextInstance context, int tokenStartIndex, ReadOnlyMemory<char> value)
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
        }

        /// <summary>
        /// Gets a clone of this token that is relative to a different (parent) context.
        /// </summary>
        /// <param name="targetContext"></param>
        /// <returns></returns>
        public TokenInstance GetContextualInstance(TokenContextInstance targetContext)
        {
            int contextualStartIndex = GetContextualStartIndex(targetContext);
            int delta = contextualStartIndex - StartIndex;

            return this with { Context = targetContext, EndIndex = EndIndex + delta, StartIndex = StartIndex + delta };
        }

        /// <summary>
        /// Gets a clone of this token that is relative to the entire parse session.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public TokenInstance GetAbsoluteInstance()
        {
            if (Context?.ParseSession == null) throw new Exception("Token does not belong to a parse session, cannot get absolute instance.");
            return GetContextualInstance(Context?.ParseSession?.RootContext);
        }

        /// <summary>
        /// Gets the start index of this token in another context.
        /// </summary>
        /// <param name="targetContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private int GetContextualStartIndex(TokenContextInstance targetContext)
        {
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (StartIndex < 0) throw new Exception("StartIndex must be a positive number.");

            bool parentFound = false;

            TokenContextInstance parent = Context.Parent;

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

            return StartIndex + parent.AbsoluteOffset;
        }

        public override string ToString()
        {
            return $"{TokenDefinition.GetType().Name} @:{StartIndex}=\"{Contents}\"";
        }
    }
}
