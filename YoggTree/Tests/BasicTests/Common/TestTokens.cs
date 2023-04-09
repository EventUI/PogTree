/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Tokens;

namespace YoggTreeTest.Common
{
    public static class TestTokens
    {
        public static TestTokenInstance OpenBracket = new TestTokenInstance(new OpenBracketToken()) { Contents = "[" };

        public static TestTokenInstance CloseBracket = new TestTokenInstance(new CloseBracketToken()) { Contents = "]" };

        public static TestTokenInstance OpenBrace = new TestTokenInstance(new OpenCurlyBraceToken()) { Contents = "{" };

        public static TestTokenInstance CloseBrace = new TestTokenInstance(new CloseCurlyBraceToken()) { Contents = "}" };

        public static TestTokenInstance OpenParens = new TestTokenInstance(new OpenParenthesisToken()) { Contents = "(" };

        public static TestTokenInstance CloseParens = new TestTokenInstance(new CloseParenthesisToken()) { Contents = ")" };

        public static TestTokenInstance SparseCloseBracketToken = new TestTokenInstance(new SparseCloseBracketToken(), "]", TokenInstanceType.RegexResult);

        public static TestTokenInstance HorizontalWhitespace(string whitespace)
        {
            return new TestTokenInstance(new WhitespaceHorizontalToken(), whitespace, TokenInstanceType.RegexResult);
        }

        public static TestTokenInstance VerticalWhitespace(string whitespace)
        {
            return new TestTokenInstance(new WhitespaceHorizontalToken(), whitespace, TokenInstanceType.RegexResult);
        }

        public static TestTokenInstance TextContent(string content)
        {
            return new TestTokenInstance(new TextContentToken(), content, TokenInstanceType.TextPlaceholder);
        }

        public static TestTokenInstance ChildContext(string contextContent, TestContextInstance childContext)
        {
            return new TestTokenInstance(new EmptyToken(), contextContent, TokenInstanceType.ContextPlaceholder)
            {
                TestContextInstance = childContext
            };
        }

        public static TestTokenInstance SparseOpenBracket<TChildContext>() where TChildContext : TokenContextDefinition, new()
        {
            return new TestTokenInstance(new SparseOpenBracketToken<TChildContext>(), "[", TokenInstanceType.RegexResult);
        }
    }

    public class SparseOpenBracketToken<TChildContext> : TokenDefinition where TChildContext : TokenContextDefinition, new()
    {
        public SparseOpenBracketToken()
            : base(new System.Text.RegularExpressions.Regex("\\["), "[", TokenDefinitionFlags.ContextStarter, "SparseBracket")
        {

        }

        public override TokenContextDefinition GetNewContextDefinition(TokenInstance start)
        {
            var child = new TChildContext();

            child.RemoveToken<OpenBracketToken>();
            child.RemoveToken<CloseBracketToken>();
            child.AddToken<SparseOpenBracketToken<TChildContext>>();
            child.AddToken<SparseCloseBracketToken>();

            return child;
        }
    }

    public class SparseCloseBracketToken : TokenDefinition
    {
        public SparseCloseBracketToken()
            : base(new System.Text.RegularExpressions.Regex("\\]"), "]", TokenDefinitionFlags.ContextEnder, "SparseBracket")
        {

        }
    }
}
