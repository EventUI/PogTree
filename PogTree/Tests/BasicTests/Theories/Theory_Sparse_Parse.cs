/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogTreeTest.Theories
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
            MakeAdvancedArgs(args);

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));
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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));
        }

        private static void MakeAdvancedArgs(List<object[]> args)
        {
            string contentToParse = "  [abc 123 ] lol spaces [someMore[  ]Content]";

            var child1 = new TestContextInstance(new TChild())
            {
                Contents = "[abc 123 ]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.TextContent("abc"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("123"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.SparseCloseBracketToken
                }
            };

            var child2_1 = new TestContextInstance(new TChild())
            {
                Contents = "[  ]",
                Depth = 2,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.HorizontalWhitespace("  "),
                    TestTokens.SparseCloseBracketToken
                },

            };

            var child2 = new TestContextInstance(new TChild())
            {
                Contents = "[someMore[  ]Content]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.SparseOpenBracket<TChild>(),
                    TestTokens.TextContent("someMore"),
                    TestTokens.ChildContext("[  ]", child2_1),
                    TestTokens.TextContent("Content"),
                    TestTokens.SparseCloseBracketToken
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child2_1
                }
            };


            var expected = new TestContextInstance(new T())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("  "),
                    TestTokens.ChildContext("[abc 123 ]", child1),
                    TestTokens.TextContent(" lol spaces "),
                    TestTokens.ChildContext("[someMore[  ]Content]", child2)
                },
                ChildContexts = new List<TestContextInstance> { child1, child2 }
            };

            var testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Peers with noise"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));
        }
    }
}
