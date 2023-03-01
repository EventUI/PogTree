/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Contexts
{
    public class LiteralContentContext : TokenParseContext
    {
        public LiteralContentEscapeCharacterFlags EscapeCharacterFlags { get; } = LiteralContentEscapeCharacterFlags.None;

        public LiteralContentContext(TokenParseSession session, LiteralContentEscapeCharacterFlags escapeFlags)
            : base(session, "<literal>")
        {
            EscapeCharacterFlags = escapeFlags;
        }

        public LiteralContentContext(TokenParseContext parent, TokenInstance start, LiteralContentEscapeCharacterFlags escapeFlags)
            : base(parent, start, "<literal>")
        {
            EscapeCharacterFlags = escapeFlags;
        }

        protected override bool StartsNewContext(TokenInstance tokenInstance)
        {
            return false;
        }

        protected override bool EndsCurrentContext(TokenInstance tokenInstance)
        {
            if (base.EndsCurrentContext(tokenInstance) == false) return false;

            if (tokenInstance.StartIndex == 0) return false;
            char previousChar = Contents.Span[tokenInstance.StartIndex - 1];

            if (EscapeCharacterFlags.HasFlag(LiteralContentEscapeCharacterFlags.Backslash) == true && previousChar == '\\') return false;
            if (EscapeCharacterFlags.HasFlag(LiteralContentEscapeCharacterFlags.DoubleBackslash) && tokenInstance.StartIndex > 2)
            {
                char nextPreviousChar = Contents.Span[tokenInstance.StartIndex - 2];
                if (EscapeCharacterFlags.HasFlag(LiteralContentEscapeCharacterFlags.DoubleBackslash) == true && previousChar == '\\' && nextPreviousChar == '\\') return false;
            }

            return true;
        }

        protected override bool IsValidInContext(TokenInstance token)
        {
            return false;
        }

        protected override TokenParseContext CreateNewContext(TokenInstance startToken)
        {
            return null;
        }
    }
}
