/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTreeTest.Common
{
    public static class TestTokens
    {
        public static TestTokenInstance OpenBracket = new TestTokenInstance(StandardTokens.OpenBracket) { Contents = "[" };

        public static TestTokenInstance CloseBracket = new TestTokenInstance(StandardTokens.CloseBracket) { Contents = "]" };

        public static TestTokenInstance OpenBrace = new TestTokenInstance(StandardTokens.OpenCurlyBrace) { Contents = "{" };

        public static TestTokenInstance CloseBrace = new TestTokenInstance(StandardTokens.CloseCurlyBrace) { Contents = "}" };

        public static TestTokenInstance OpenParens = new TestTokenInstance(StandardTokens.OpenParenthesis) { Contents = "(" };

        public static TestTokenInstance CloseParens = new TestTokenInstance(StandardTokens.CloseParenthsis) { Contents = ")" };

        public static TestTokenInstance HorizontalWhitespace(string whitespace)
        {
            return new TestTokenInstance(StandardTokens.WhitespaceHorizontal, whitespace, TokenInstanceType.RegexResult);
        }

        public static TestTokenInstance VerticalWhitespace(string whitespace)
        {
            return new TestTokenInstance(StandardTokens.WhitespaceVertical, whitespace, TokenInstanceType.RegexResult);
        }

        public static TestTokenInstance TextContent(string content)
        {
            return new TestTokenInstance(StandardTokens.TextContent, content, TokenInstanceType.TextPlaceholder);
        }

        public static TestTokenInstance ChildContext(string contextContent, TestContextInstance childContext)
        {
            return new TestTokenInstance(StandardTokens.Empty, contextContent, TokenInstanceType.ContextPlaceholder)
            {
                TestContextInstance = childContext
            };
        }
    }
}
