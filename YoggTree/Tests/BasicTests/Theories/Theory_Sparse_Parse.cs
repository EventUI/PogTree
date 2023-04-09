using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTreeTest.Theories
{
    internal class Theory_Sparse_Parse<T, TChild> : IEnumerable<Object[]> where T : SparseContext<TChild>, new() where TChild : TestContext, new()
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            return GetTestArgs().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static List<object[]> GetTestArgs()
        {
            List<object[]> args = new List<object[]>();

            MakeSimpleParseArgs(args);
            MakeGapArgs(args);

            return args;
        }

        private static void MakeSimpleParseArgs(List<Object[]> args)
        {
            string contentToParse = "";
            TestContextInstance expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0
            };

            var testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Empty String Test"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = " ";

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent(" ")
                }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Single space."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = "\tabc";

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("\tabc")
                }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Random text."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));
        }

        private static void MakeGapArgs(List<object[]> args)
        {          
            string contentToParse = "[]";

            var child1 = new TestContextInstance(new TChild())
            {
                Contents = "[]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.SparseCloseBracketToken
                }
            };

            var expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[]", child1)
                },
                ChildContexts = new List<TestContextInstance> { child1 }
            };

            var testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Empty brackets."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = "[ abc 123]";

            child1 = new TestContextInstance(new TChild())
            {
                Contents = "[ abc 123]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("abc"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("123"),
                    TestTokens.SparseCloseBracketToken
                }
            };

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[ abc 123]", child1)
                },
                ChildContexts = new List<TestContextInstance> { child1 }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Brackets with noise."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = "test [ abc 123]";

            child1 = new TestContextInstance(new TChild())
            {
                Contents = "[ abc 123]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("abc"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("123"),
                    TestTokens.SparseCloseBracketToken
                }
            };

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("test "),
                    TestTokens.ChildContext("[ abc 123]", child1)
                },
                ChildContexts = new List<TestContextInstance> { child1 }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Brackets with noise and leading text."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = "[ abc 123]test ";

            child1 = new TestContextInstance(new TChild())
            {
                Contents = "[ abc 123]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("abc"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("123"),
                    TestTokens.SparseCloseBracketToken
                }
            };

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {

                    TestTokens.ChildContext("[ abc 123]", child1),
                    TestTokens.TextContent("test "),
                },
                ChildContexts = new List<TestContextInstance> { child1 }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Brackets with noise and lagging text."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            /************************************************************************************/

            contentToParse = "test[ abc 123]test ";

            child1 = new TestContextInstance(new TChild())
            {
                Contents = "[ abc 123]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("abc"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("123"),
                    TestTokens.SparseCloseBracketToken
                }
            };

            expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("test"),
                    TestTokens.ChildContext("[ abc 123]", child1),
                    TestTokens.TextContent("test "),
                },
                ChildContexts = new List<TestContextInstance> { child1 }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Brackets with noise, leading and lagging text."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));
        }
    }
}
