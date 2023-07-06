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
        [Fact]
        public void TestQuotedString()
        {
            string code = """
            function HelloWord()
            {
                var string1 = "A string in quotes";
                var string2 = `A string in graves`;
                var string3 = 'A string in single quotes';

                console.log("Hello World!");
                console.log(string1);
                console.log(string2);
                console.log(string3);
            }
            """;

            //turn the string into a hierarchy of the contexts and tokens defined above
            var parser = new TokenParser();
            TokenContextInstance rootContext = parser.Parse<QuotedTextFileContext>(code);

            //use the reader to get the child contexts that will contain the strings from the code block above.
            TokenReader reader = rootContext.GetReader();
            TokenContextInstance quotedText = reader.GetNextContext<StringContext>();
            TokenContextInstance graveText = reader.GetNextContext<StringContext>();
            TokenContextInstance singleQuotedText = reader.GetNextContext<StringContext>();
            TokenContextInstance helloWorld = reader.GetNextContext<StringContext>();

            Assert.Equal("\"A string in quotes\"", quotedText.GetText());
            Assert.Equal("`A string in graves`", graveText.GetText());
            Assert.Equal("'A string in single quotes'", singleQuotedText.GetText());
            Assert.Equal("\"Hello World!\"", helloWorld.GetText());
        }


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
                return false; //no tokens will start a new context from within a string context, even though a QuotedStringToken would normally do so.
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
    }
}
