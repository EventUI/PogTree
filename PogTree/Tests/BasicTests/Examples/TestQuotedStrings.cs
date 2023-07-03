using PogTree.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PogTreeTest.Examples
{
    public class TestQuotedStrings
    {
        /// <summary>
        /// Marks the beginning of a quoted string, either with a double quote, single quote, or grave. This token both starts and ends contexts.
        /// </summary>
        public class QuotedStringToken : TokenDefinition
        {
            public QuotedStringToken()
                : base(new Regex("\"|'|`"), "QuotedString", TokenDefinitionFlags.ContextStarter | TokenDefinitionFlags.ContextEnder, "QuotedString")
            {

            }

            public override TokenContextDefinition GetNewContextDefinition(TokenInstance start)
            {
                return new StringContext(); //when told to make a new context, make a new StringContext
            }
        }

        /// <summary>
        /// Context that contains a quoted string and only ends when a non-escaped QuotedStringToken of the same value as the context's StartToken.
        /// </summary>
        public class StringContext : TokenContextDefinition
        {
            public StringContext()
                : base("StringContext")
            {
                AddToken<QuotedStringToken>();
                AddToken<BackslashToken>();
            }

            public override bool EndsCurrentContext(TokenInstance tokenInstance)
            {
                if (base.EndsCurrentContext(tokenInstance) == true) //it has the right flags and ContextKey
                {
                    if (tokenInstance.Contents.Span.SequenceEqual(tokenInstance.Context.StartToken.Contents.Span) == false) return false; //make sure it matches the start token

                    TokenInstance previousToken = tokenInstance.PeekPreviousToken();
                    if (previousToken.Is<BackslashToken>() == true && previousToken.EndIndex == tokenInstance.StartIndex) return false; //if it is directly adjacent to a backslash its escaped, we don't end the context.

                    return true;
                }

                return false;
            }

            public override bool StartsNewContext(TokenInstance tokenInstance)
            {
                return false;
            }
        }

        /// <summary>
        /// A basic context that only searches for QuotedStringTokens.
        /// </summary>
        public class QuotedTextFileContext : TokenContextDefinition
        {
            public QuotedTextFileContext()
                : base("QuotedTextFileContext")
            {
                AddToken<QuotedStringToken>();
            }
        }

        [Fact]
        public void TestQuotedString()
        {
            string text = """
            "Hello, \"World\"." said the programmer
            as he got his demo code to technically 'work'.
            """;

            var parser = new TokenParser();
            TokenContextInstance parsed = parser.Parse<QuotedTextFileContext>(text);

            TokenReader reader = parsed.GetReader();
            var firstQuotedText = reader.GetNextContext<StringContext>();
            var secondSingleQuotedText = reader.GetNextContext<StringContext>();

            if (firstQuotedText.GetText() != "\"Hello, \\\"World\\\".\"") throw new Exception("First quoted text was not correct.");
            if (secondSingleQuotedText.GetText() != "'work'") throw new Exception("second quoted text was not correct.");
        }
    }
}
