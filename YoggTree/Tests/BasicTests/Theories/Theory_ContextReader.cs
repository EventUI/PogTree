using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Tokens;
using YoggTreeTest.Common;

namespace YoggTreeTest.Theories
{
    internal class Theory_ContextReader : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            return GetTestArgs().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private List<object[]> GetTestArgs()
        {
            List<object[]> args = new List<object[]>();

            GetSimpleIteration(args);

            return args;
        }

        private static void GetSimpleIteration(List<object[]> args)
        {
            string contents = "";
            var iterationArgs = new TestIterationArgs()
            {
                ContentToParse = contents,
                TestName = "Empty string"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(iterationArgs));

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
                ExpectedContexts = new List<TestContextInstance>() { instance1 },
                ExpectedTokens = new List<TestTokenInstance>() { instance1.Tokens[0] }
            };

            args.Add(YoggTreeTestHelper.ToObjArray(iterationArgs));

            iterationArgs = iterationArgs with
            {
                TestName = "Basic Recursive",
                Recursive = true,
                ExpectedContexts = new List<TestContextInstance>() { instance1, instance2 },
                ExpectedTokens = new List<TestTokenInstance>() { instance2.Tokens[0], instance2.Tokens[1], instance2.Tokens[2] }
            };

            args.Add(YoggTreeTestHelper.ToObjArray(iterationArgs));
        }
    }
}
