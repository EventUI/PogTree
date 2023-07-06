using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PogTreeTest.Examples
{
    public class MultipleContextEnding
    {
        [Fact]
        public void TestMultipleEnds()
        {
            var content = """
                //a single line comment

                /*Multi-line comment.*/

                /*Summary
                @param {SomeValue} a The first value.
                @param {SomeOtherValue} b The second value.*/
                function Test(a, b)
                {
                    console.log(a + b);
                }
                """;

            var parser = new TokenParser();
            TokenContextInstance root = parser.Parse<RootContext>(content);

            //get a token reader and dig recursively into the hierarchy of contexts and pull out the two YUIDoc contexts,
            //which sit next to each other in their parent MultiLineCommentContext, which is the second ChildContext of the RootContext.
            TokenReader reader = root.GetReader();
            TokenContextInstance yui1 = reader.GetNextContext<YUIDocContext>(true);
            TokenContextInstance yui2 = reader.GetNextContext<YUIDocContext>(true);

            Assert.Equal("@param {SomeValue} a The first value.\r\n", yui1.GetText()); //the \r\n is added by C#'s multi-line string syntax-sugar, so even though they don't appear literally in that string it's implicitly there between the end of "." and the beginning of "@param"
            Assert.Equal("@param {SomeOtherValue} b The second value.", yui2.GetText());
        }

        /// <summary>
        /// Token that marks the beginning of a multi-line comment and spawns MultiLineCommentContexts when encountered.
        /// </summary>
        public class MultiLineCommentStart : TokenDefinition
        {
            public MultiLineCommentStart()
                : base(new Regex("\\/\\*"), "/*", TokenDefinitionFlags.ContextStarter, "MultiLineComment")
            {

            }

            public override TokenContextDefinition GetNewContextDefinition(TokenInstance start)
            {
                return new MultiLineCommentContext();
            }
        }

        /// <summary>
        /// Token that marks the ending of a multi-line comment.
        /// </summary>
        public class MultiLineCommentEnd : TokenDefinition
        {
            public MultiLineCommentEnd()
                : base(new Regex("\\*\\/"), "*/", TokenDefinitionFlags.ContextEnder, "MultiLineComment")
            {

            }
        }

        /// <summary>
        /// Token that marks be beginning/end of a YUIDoc parameter info context and spawns YUIDocContexts.
        /// </summary>
        public class YUIDocParamStart : TokenDefinition
        {
            public YUIDocParamStart()
                : base(new Regex("@param"), "@param", TokenDefinitionFlags.ContextStarter | TokenDefinitionFlags.ContextEnder, "YUIDocParamStart")
            {

            }

            public override TokenContextDefinition GetNewContextDefinition(TokenInstance start)
            {
                return new YUIDocContext();
            }
        }

        /// <summary>
        /// Context for reading multi-line comments. Looks for its end token and the YUIDoc start token to spawn child YUIDocContexts.
        /// </summary>
        public class MultiLineCommentContext : TokenContextDefinition
        {
            public MultiLineCommentContext()
                : base("MultiLineComment")
            {
                AddToken<MultiLineCommentEnd>();
                AddToken<YUIDocParamStart>();
            }
        }

        /// <summary>
        /// Context for capturing a YUIDoc directives. Looks for the end token for a multi-line comment and for the beginning of another YUIDoc token (which ends this context and starts a peer context)
        /// </summary>
        public class YUIDocContext : TokenContextDefinition
        {
            public YUIDocContext()
                : base("YuiDoc")
            {
                AddToken<MultiLineCommentEnd>(); //ends on either the end of the multi-line comment
                AddToken<YUIDocParamStart>(); //or ends when another YUIDoc directive appears.
            }

            public override bool StartsNewContext(TokenInstance tokenInstance)
            {
                return false; //doesn't start any child contexts under itself
            }

            public override bool EndsCurrentContext(TokenInstance tokenInstance)
            {
                //Look ahead to see if the NEXT token after the current one is one of the context enders. If so, end the context.
                //If we wait to check the tokenInstance passed in to this method, we will already either be bleeding into what should
                //be a new context or the end of a parent context.
                if (tokenInstance.PeekNextToken().Is<YUIDocParamStart>() || tokenInstance.PeekNextToken().Is<MultiLineCommentEnd>()) 
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Root context definition that is told to only look for multi-line comment starts.
        /// </summary>
        public class RootContext : TokenContextDefinition
        {
            public RootContext()
                :base("root")
            {
                AddToken<MultiLineCommentStart>();
            }
        }


    }
}
