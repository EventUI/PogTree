/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core
{
    /// <summary>
    /// Represents an instance of a TokenDefinitionBase that was found in a TokenParseContext's Content.
    /// </summary>
    public sealed record TokenInstance : IContentSpan, IEquatable<TokenInstance>, IComparable<TokenInstance>
    {
        /// <summary>
        /// The definition of the rules for the token.
        /// </summary>
        public ITokenDefinition TokenDefinition { get; } = null;

        /// <summary>
        /// The targetContext in which the token was found.
        /// </summary>
        public TokenParseContext Context { get; internal init; } = null;

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

        internal TokenInstance(ITokenDefinition tokenDefinition, TokenParseContext context, int tokenStartIndex, ReadOnlyMemory<char> value)
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

        public override string ToString()
        {
            return $"{TokenDefinition.ToString()}  @{StartIndex}";
        }

        public bool Equals(TokenInstance other)
        {
            if (other == null) return false; 
            if (other.TokenDefinition.ID != TokenDefinition.ID) return false;
            if (other.StartIndex != StartIndex) return false;
            if (other.Contents.Span.SequenceEqual(Contents.Span) == false) return false;
            if (other.EndIndex != EndIndex) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int GetContextualIndex(TokenParseContext targetContext)
        {
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (StartIndex < 0) throw new Exception("StartIndex must be a positive number.");

            bool parentFound = false;

            TokenParseContext parent = Context.Parent;

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

            return StartIndex + targetContext.AbsoluteOffset;
        }

        public int GetAbsoluteIndex()
        {
            if (Context?.ParseSession == null) throw new Exception("Token does not belong to a parse session, cannot calculate absolute index.");
            return GetContextualIndex(Context.ParseSession.RootContext);
        }

        public TokenInstance GetContextualInstance(TokenParseContext targetContext)
        {
            int contextualStartIndex = GetContextualIndex(targetContext);
            int delta = contextualStartIndex - StartIndex;

            return this with { Context = targetContext, EndIndex = EndIndex + delta, StartIndex = StartIndex + delta };
        }

        public TokenInstance GetAbsoluteInstance()
        {
            if (Context?.ParseSession == null) throw new Exception("Token does not belong to a parse session, cannot get absolute instance.");
            return GetContextualInstance(Context?.ParseSession?.RootContext);
        }

        public int CompareTo(TokenInstance other)
        {
            return (StartIndex - other.StartIndex);
        }

        TokenParseContext IContentSpan.GetContext()
        {
            return Context;
        }
    }
}
