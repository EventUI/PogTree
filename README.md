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

### Regexes

In PogTree it is best to use short, simple Regex statements rather than huge complicated ones - the reason for this is that PogTree is based around the idea that a context only looks for a subset of all possible Regexes (Tokens) that need to be found in a string, and flags certain Tokens as starters for a new contexts where a **different** subset of all Tokens apply. 

The contextual breakdown makes it much easier to target or isolate specific patterns specifying exactly what Tokens should be found in a context - there is no "token inheritance" and the tokens found in a parent context are not included in a child context unless they are explicitly added to it. Being able to select exactly which tokens to look for in a context effectively eliminates "noise" Regex matches that are not applicable for what the context is trying to encapsulate. 

The caveat to the contextual hierarchy is that a context needs to have an "end" token that signals the end of that context - if this token is never found, the parser will throw an exception when it runs out of content (unless the context is flagged as being 'unbounded').

### How it Works

The way the parser works is that it moves through the string in a single forwards-only sweep, lazily finding the minimum number of Regex results possible to complete the parse operation - this means that when the parser encounters a new context, it has no idea when (or if) that context will end because none of the tokens in that context have been found yet. 

The parser rises back up out of the recursive hierarchy when it finds a token that ends the current context - it then resumes the parse operation in the current context's parent context until it has reached the end of the top-level "root" context (which is implicitly unbounded). 

### Examples

#### Finding all quoted strings in a block of code.
In this example the parser is told to only look for the 'start/end string' characters of ", ', and `. - when any of those characters are encountered, the parser will create a new StingContext which continues until it meets an unescaped QuotedStringToken whose value is the same as the token that triggered the creation of the StringContext. This results in 4 string contexts, each containing the quoted strings in order of occurrence. 

Note that the parser did not try and make a "sub" StringContext for the quotes/graves around the word "string" within the first three strings - this is because the StringContext has been told to make no new child contexts, so the normal context creation logic didn't apply even if a token would normally start a context.

Also note that the double-quotes within the double-quotes did not trip up the parser - this is because the StringContext is told to ignore a token that would normally end the context, but not if it is adjacent to a BackslashToken.

    public class TestQuotedStrings //A XUnit test class
    {
        [Fact]
        public void TestQuotedString()
        {
            string code = """
            function HelloWord()
            {
                var string1 = "A \"string\" in quotes";
                var string2 = `A 'string' in graves`;
                var string3 = 'A `string` in single quotes';

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

            Assert.Equal("\"A \\\"string\\\" in quotes\"", quotedText.GetText()); //C#'s multi-line string syntax sugar automatically escapes characters that need escaping, so we have to include the escaped versions of the characters in our normal string
            Assert.Equal("`A 'string' in graves`", graveText.GetText());
            Assert.Equal("'A `string` in single quotes'", singleQuotedText.GetText());
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

In this more complicated example, the parser is told to look for only multi-line comment starts at the top level of its hierarchy, then when within a MultiLineCommentContext the parser begins looking for the start tokens for a YUIDoc parameter info comments and for the ending multi-line comment token. 

The result is a hierarchy of contexts that contains two YUIDocContexts that contain the full text of both YUIDoc parameter directives, which can easily be extracted using a TokenReader to recursively seek through the hierarchy to find them. 

The other "matching" YUIDoc parameter directives (the one in the single-line comment and the one in the string) are ignored because they are not being looked for outside of a MultiLineCommentContext, so the "noise" matches that would satisfy the YUIDocParamStart's Regex pattern are ignored as they are not being looked for in a PlainTextContext.


    public class TestYUIDocExample //A XUnit test
    {
        [Fact]
        public void TestYUIDoc()
        {
            var content = """
                //@param {SomeType} This won't get picked up by the parser because it's not in a multi-line quote

                var commonFailureCase = "@param {SomeType} This also won't get picked up by the parser."

                /*
                A
                Multi
                -line 
                comment. 
                This DOES get captured by the parser and becomes its own MultiLineCommentContext
                */

                /**The actual set of YUIDoc directives we want to capture are the two below.
                @param {SomeType} a The first value.
                @param {SomeType} b The second value.*/
                function Test(a, b)
                {
                    console.log(a + b);
                }
                """;

            var parser = new TokenParser();
            TokenContextInstance root = parser.Parse<PlainTextContext>(content);

            //get a token reader and dig recursively into the hierarchy of contexts and pull out the two YUIDoc contexts,
            //which sit next to each other in their parent MultiLineCommentContext, which is the second ChildContext of the PlainTextContext.
            TokenReader reader = root.GetReader();
            TokenContextInstance yui1 = reader.GetNextContext<YUIDocContext>(true);
            TokenContextInstance yui2 = reader.GetNextContext<YUIDocContext>(true);

            Assert.Equal("@param {SomeType} a The first value.\r\n", yui1.GetText()); //the \r\n is added by C#'s multi-line string syntax-sugar, so even though they don't appear literally in that string it's implicitly there between the end of "." and the beginning of "@param"
            Assert.Equal("@param {SomeType} b The second value.", yui2.GetText());
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
        public class PlainTextContext : TokenContextDefinition
        {
            public PlainTextContext()
                :base("root")
            {
                AddToken<MultiLineCommentStart>();
            }
        }
    }
