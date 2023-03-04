using Xunit.Abstractions;

namespace BasicTests
{
    public class BasicFunctionality
    {
        protected ITestOutputHelper _output = null;

        public class TestContext : TokenContextDefinition
        {
            public TestContext()
                :base("TestContext", StandardTokens.GetAllStandardTokens())
            {

            }
        }

        public class EmptyContext : TokenContextDefinition
        {
            public EmptyContext()
                :base("Empty")
            {

            }
        }

        public BasicFunctionality(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test_EmptyString()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("");

            Assert.True(result.Tokens.Count == 0);
        }

        [Fact]
        public void Test_OnlyWhitespace()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>(" ");

            Assert.True(result.Tokens.Count == 1);
            Assert.True(result.Tokens[0].TokenDefinition == StandardTokens.WhitespaceHorizontal);
        }

        [Fact]
        public void Test_NoMatch()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("a");

            Assert.True(result.Tokens.Count == 0);
        }

        [Fact]
        public void Test_OneChildContext()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[]");

            Assert.True(result.ChildContexts.Count == 1);
        }

        [Fact]
        public void Test_TwoChildContexts()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[]{}");

            Assert.True(result.ChildContexts.Count == 2);
        }

        [Fact]
        public void Test_NestedSameChildContexts()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[[]]");

            Assert.True(result.ChildContexts.Count == 1);
            Assert.True(result.ChildContexts[0].ChildContexts.Count == 1);
        }

        [Fact]
        public void Test_NestedDifferentChildContexts()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[{}]");

            Assert.True(result.ChildContexts.Count == 1);
            Assert.True(result.ChildContexts[0].ChildContexts.Count == 1);
        }

        [Fact]
        public void Test_NestedManyDifferentChildContexts()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[{[[]]}]");

            var childContext = result.ChildContexts[0];
            while (childContext != null)
            {
                _output.WriteLine($"Depth{childContext.Depth} :  {childContext.Contents.ToString()}");
                childContext = (childContext.ChildContexts.Count > 0) ? childContext.ChildContexts[0] : null;
            }
        }

        [Fact]
        public void Test_NestedManyDifferentChildContextsWithNoise()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[cats{are[great[pets]]}]");

            var childContext = result.ChildContexts[0];
            while (childContext != null)
            {
                _output.WriteLine($"Depth{childContext.Depth} :  {childContext.Contents.ToString()}");
                childContext = (childContext.ChildContexts.Count > 0) ? childContext.ChildContexts[0] : null;
            }
        }

        [Fact]
        public void Test_NestedManyDifferentChildContextsWithWhiteNoise()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[\n{\r[\t[   ]]}]");

            var childContext = result.ChildContexts[0];
            while (childContext != null)
            {
                _output.WriteLine($"Depth{childContext.Depth} :  {childContext.Contents.ToString()}");
                childContext = (childContext.ChildContexts.Count > 0) ? childContext.ChildContexts[0] : null;
            }
        }

        [Fact]
        public void Test_NestedManyDifferentChildContextsWithSeperateHierarchies()
        {
            var parser = new TokenParser();
            var result = parser.Parse<TestContext>("[{[[]]}][{[]}]");

            foreach (var context in result.ChildContexts)
            {
                _output.WriteLine($"Top - Depth{context.Depth} :  {context.Contents.ToString()}");

                var next = context.ChildContexts.FirstOrDefault();
                while (next != null)
                {
                    _output.WriteLine($"Deep - Depth{next.Depth} :  {next.Contents.ToString()}");
                    next = next.ChildContexts.FirstOrDefault();
                }
            }
        }
    }
}