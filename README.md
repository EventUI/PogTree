# PogTree
Regex-based parser for decomposing text or code files into a strongly-typed object model.

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

### Simple Example

#### Finding all quoted strings in a block of text.

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


In this example the parser was told to only look for the 'start/end string' characters of ", ', and `. - when any of those characters were encountered, the parser would create a new StingContext which continue until it meet an unescaped QuotedStringToken whose value is the same as the token that triggered the creation of the StringContext. This is why both strings wrapped in "" and '' were captured, and the rule against ending the context upon encountering an escaped QuotedStringToken prevented the first quoted string from ending early.