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
    /// Represents an instance of a TokenDefinitionBase that was found in a TokenParseContextBase's Content.
    /// </summary>
    public sealed record TokenInstance : IEquatable<TokenInstance>, IContextIndexable, IComparable<TokenInstance>
    {
        /// <summary>
        /// The definition of the rules for the token.
        /// </summary>
        public TokenDefinitionBase TokenDefinition { get; } = null;

        /// <summary>
        /// The context in which the token was found.
        /// </summary>
        public TokenParseContextBase Context { get; internal init; } = null;

        /// <summary>
        /// The index at which the token begins.
        /// </summary>
        public int TokenStartIndex { get; internal init; } = -1;

        /// <summary>
        /// The value of the token that was found.
        /// </summary>
        public ReadOnlyMemory<char> Value { get; } = null;

        /// <summary>
        /// The ending index of the token.
        /// </summary>
        public int TokenEndIndex { get; internal init; } = -1;

        internal TokenInstance(TokenDefinitionBase tokenDefinition, TokenParseContextBase context, int tokenStartIndex, ReadOnlyMemory<char> value)
        {
            if (tokenDefinition == null) throw new ArgumentNullException("token");
            if (context == null) throw new ArgumentNullException("context");
            if (tokenStartIndex < 0) throw new ArgumentOutOfRangeException(nameof(tokenStartIndex));
            if (value.IsEmpty == true) throw new ArgumentException("value cannot be empty.");

            TokenDefinition = tokenDefinition;
            Context = context;
            TokenStartIndex = tokenStartIndex;
            Value = value;
            TokenEndIndex = TokenStartIndex + value.Length;
        }   

        public override string ToString()
        {
            return $"{TokenDefinition.ToString()}  @{TokenStartIndex}";
        }

        public bool Equals(TokenInstance other)
        {
            if (other == null) return false; 
            if (other.TokenDefinition.ID != TokenDefinition.ID) return false;
            if (other.TokenStartIndex!= TokenStartIndex) return false;
            if (other.Value.Span.SequenceEqual(Value.Span) == false) return false;
            if (other.TokenEndIndex != TokenEndIndex) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int GetContextIndex(int localIndex, TokenParseContextBase targetParent)
        {
            List<TokenParseContextBase> parentStack = new List<TokenParseContextBase>();
            var parent = Context;

            while (parent != null)
            {
                if (parent == targetParent) break;

                parentStack.Add(parent);
                parent= parent.Parent;
            }

            if (parentStack.Contains(targetParent) == false) throw new Exception($"{targetParent.ToString()} was not a parent of this {GetType().Name}");
            parentStack.Reverse();

            int index = targetParent.StartIndex;

            foreach (var higherParent in parentStack)
            {
                index += higherParent.StartIndex;
            }

            return index + localIndex;
        }

        public int GetAbsoluteIndex(int localIndex, TokenParseSession session)
        {
            if (Context.ParseSession != session) throw new Exception($"This {GetType().Name} is not contained by the provided {nameof(session)}");

            List<TokenParseContextBase> parentStack = new List<TokenParseContextBase>();
            var parent = Context;

            while (parent != null)
            {
                parentStack.Add(parent);
                parent = parent.Parent;
            }

            parentStack.Reverse();

            int index = 0;
            foreach (var higherParent in parentStack)
            {
                index += higherParent.StartIndex;
            }

            return index + localIndex;
        }

        public int CompareTo(TokenInstance other)
        {
            return (TokenStartIndex - other.TokenStartIndex);
        }
    }
}
