using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogTree.Core.Tokens;
using PogTreeTest.Common;

namespace PogTreeTest.Theories
{
    internal class Theory_ContextReader : IEnumerable<object[]>
    {
        protected bool _reverse = false;

        public IEnumerator<object[]> GetEnumerator()
        {
            return GetTestArgs().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected List<object[]> GetTestArgs()
        {
            List<object[]> args = new List<object[]>();

            GetSimpleIteration(args);
            GetRecursiveIteration(args);

            return args;
        }

        private void GetSimpleIteration(List<object[]> args)
        {
            string contents = "";
            var iterationArgs = new TestIterationArgs()
            {
                ContentToParse = contents,
                TestName = "Empty string"
            };

            args.Add(PogTreeTestHelper.ToObjArray(iterationArgs));

            contents = "[text]";
            var instance2 = new TestContextInstance(new TestContext())
            {
                Contents = "[text]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.TextContent("text"),
                    TestTokens.CloseBracket
                }
            };

            var instance1 = new TestContextInstance(new TestContext())
            {
                Contents = contents,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[text]", instance2)
                }
            };

            iterationArgs = new TestIterationArgs()
            {
                TestName = "Basic Non-Recursive",
                ContentToParse = contents,
                Recursive = false,
                Reverse = _reverse,
                ExpectedContexts = new List<TestContextInstance>() { instance1 },
                ExpectedTokens = new List<TestTokenInstance>() { instance1.Tokens[0] }
            };

            args.Add(PogTreeTestHelper.ToObjArray(iterationArgs));

            iterationArgs = iterationArgs with
            {
                TestName = "Basic Recursive",
                Recursive = true,
                Reverse = _reverse,
                ExpectedContexts = new List<TestContextInstance>() { instance1, instance2 },
                ExpectedTokens = new List<TestTokenInstance>() { instance2.Tokens[0], instance2.Tokens[1], instance2.Tokens[2] }
            };

            args.Add(PogTreeTestHelper.ToObjArray(iterationArgs));
        }

        private void GetRecursiveIteration(List<object[]> args)
        {
            string contentToParse = "[]{}";

            TestContextInstance child1 = new TestContextInstance(new TestContext())
            {
                Contents = "[]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket
                }
            };

            TestContextInstance child2 = new TestContextInstance(new TestContext())
            {
                Contents = "{}",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBrace,
                    TestTokens.CloseBrace,
                }
            };

            TestContextInstance parent = new TestContextInstance(new TestContext())
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[]", child1),
                    TestTokens.ChildContext("{}", child2)
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child1, child2
                }
            };

            TestIterationArgs iterationArgs = new TestIterationArgs()
            {
                TestName = "Two Contexts",
                ContentToParse = contentToParse,
                Recursive = true,
                Reverse = _reverse,
                ExpectedContexts = new List<TestContextInstance>() { parent, child1, child2 },
                ExpectedTokens = new List<TestTokenInstance>() { TestTokens.OpenBracket, TestTokens.CloseBracket, TestTokens.OpenBrace, TestTokens.CloseBrace }

            };

            args.Add(PogTreeTestHelper.ToObjArray(iterationArgs));

            /***************************************************************************/

            contentToParse = "[{[[()]]}]";
            TestContext testContext = new TestContext();

            var child_1_5 = new TestContextInstance(testContext)
            {
                Contents = "()",
                Depth = 5,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenParens,
                    TestTokens.CloseParens
                }
            };

            var child_1_4 = new TestContextInstance(testContext)
            {
                Contents = "[()]",
                Depth = 4,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.ChildContext("()", child_1_5),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_5
                }
            };

            var child_1_3 = new TestContextInstance(testContext)
            {
                Contents = "[[()]]",
                Depth = 3,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.ChildContext("[()]", child_1_4),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_4
                }
            };

            var child_1_2 = new TestContextInstance(testContext)
            {
                Contents = "{[[()]]}",
                Depth = 2,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBrace,
                    TestTokens.ChildContext("[[()]]", child_1_3),
                    TestTokens.CloseBrace
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_3
                }
            };

            var child_1_1 = new TestContextInstance(testContext)
            {
                Contents = "[{[[()]]}]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.ChildContext("{[[()]]}", child_1_2),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_2
                }
            };

            var child = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[{[[()]]}]", child_1_1)
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_1
                }
            };

            TestContextInstance expected = child;

            iterationArgs = new TestIterationArgs()
            {
                TestName = "Deep Recursion",
                ContentToParse = contentToParse,
                Recursive = true,
                Reverse = _reverse,
                ExpectedContexts = new List<TestContextInstance>() { child, child_1_1, child_1_2, child_1_3, child_1_4, child_1_5 },
                ExpectedTokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.OpenBrace,
                    TestTokens.OpenBracket,
                    TestTokens.OpenBracket,
                    TestTokens.OpenParens,
                    TestTokens.CloseParens,
                    TestTokens.CloseBracket,
                    TestTokens.CloseBracket,
                    TestTokens.CloseBrace,
                    TestTokens.CloseBracket
                }
            };

            args.Add(PogTreeTestHelper.ToObjArray(iterationArgs));
        }
    }
}
