# PogTree
Regex-based parser for decomposing code files into a strongly-typed object model.

### Summary
PogTree is a utility designed to use Regex's to search and find patterns in code files to produce an object graph that can be traversed to interpret the contents of the file.

### Concepts

PogTree is based around two central concepts: tokens and contexts.
1. A token is an object representing a Regex result found in a string.
2. A context is an object that starts with a particular token, ends with a particular token, and has a set of possible tokens that can be found within it.

Both tokens and contexts have objects representing their definitions and objects representing instances of those definitions. 
1. A token is defined with a type that derives from **TokenDefinition**. Every definition should be its own type, be immutable, and have a unique Regex that it is searching for.
2. A token definition that appears in a string becomes an object called a **TokenInstance** that represents the definition, context, and location of the definition's Regex in its context's content string.
3. A context is defined with a type that derives from **TokenContextDefinition**. TokenDefinition-derived types must be added to the TokenContextDefinition-derived type in order for the context to find them: contexts will only search for tokens that they has been instructed to search for. Furthermore, TokenContextDefinitions decide when a TokenInstance of a particular definition should - or should not - start a child context. 
4. An instance of a context definition that has been created comes in the form of a **TokenContextInstance** that contains the TokenContextDefinition defining its behavior and all the TokenInstances that were found based on the definition's list of applicable tokens. TokenContextInstances can only be created by other TokenContextInstances when they have been instructed to do so based on the occurrence of a particular token or sequence of tokens.

It is because contexts spawn child contexts that the raw text content can be broken down into a hierarchy of context instances and token instances that make it easier to reason about building an object model to represent the contents of the file.

### Examples

#### Finding all quoted strings in a block of code.
In this example the parser is told to only look for the 'start/end string' characters of ", ', and `. - when any of those characters are encountered, the parser will create a new StingContext which continues until it meets an unescaped QuotedStringToken whose value is the same as the token that triggered the creation of the StringContext. This results in 4 string contexts, each containing the quoted strings in order of occurrence. 

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





#### Parsing YUIDoc information out of multi-line comments.

In this more complicated example, the parser is told to look for only multi-line comment starts at the top level of its hierarchy, then when within a MultiLineCommentContext the parser begins looking for the start tokens for a YUIDoc parameter info comments and for the ending multi-line comment token. The result is a hierarchy of contexts that contains two YUIDocContexts that contain the full text of both YUIDoc parameter directives, which can easily be extracted using a TokenReader to recursively seek through the hierarchy to find them.


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
