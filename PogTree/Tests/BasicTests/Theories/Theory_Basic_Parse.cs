﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogTree.Core.Tokens;
using PogTreeTest.Common;

namespace BasicTests.Theories
{
    public class Theory_Basic_Parse<T> : IEnumerable<object[]> where T : TokenContextDefinition, new()
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            return GetTestArgs().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static List<object[]>  GetTestArgs()
        {
            var args = new List<object[]>();
            AddSimpleTestArgs(args);
            AddSimpleHierarchyTests(args);
            AddComplexHierarchyTests(args);

            return args;
        }

        private static void AddSimpleTestArgs(List<object[]> args)
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

            contentToParse = " ";
            expected = expected with
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    new TestTokenInstance(new WhitespaceHorizontalToken())
                    {
                        Contents = contentToParse,
                    }
                }
            };

            testArgs = testArgs with
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Single Space Test"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

            contentToParse = "a";
            expected = expected with
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("a")
                }
            };

            testArgs = testArgs with
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Single character"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));                
        }

        private static void AddSimpleHierarchyTests(List<object[]> args)
        {
            string contentToParse = "[]";
            T testContext = new T();
            TestContextInstance childInstance = new TestContextInstance(testContext)
            {
                Contents = "[]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket
                }
            };

            TestContextInstance expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[]", childInstance)
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    childInstance
                }
            };

            var testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy One Layer Test"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

            /*****************************************************************************************/

            contentToParse = "[]{}";
            testContext = new T();
            TestContextInstance child1 = new TestContextInstance(testContext)
            {
                Contents = "[]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket
                }
            };

            TestContextInstance child2 = new TestContextInstance(testContext)
            {
                Contents = "{}",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBrace,
                    TestTokens.CloseBrace,
                }
            };

            expected = new TestContextInstance(testContext)
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

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Two Peers One Layer Test"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

            /*****************************************************************************************/

            contentToParse = "[[]]";
            testContext = new T();

            var childSub1 = new TestContextInstance(testContext)
            {
                Contents = "[]",
                Depth = 2,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket,
                }
            };

            child1 = new TestContextInstance(testContext)
            {
                Contents = "[[]]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.ChildContext("[]", childSub1),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    childSub1     
                }
            };

            expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.ChildContext("[[]]", child1),
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child1   
                }
            };

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Two layers of the same hierarchy."
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));
        }

        private static void AddComplexHierarchyTests(List<object[]> args)
        {
            string contentToParse = "[{[[()]]}]";
            T testContext = new T();

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

            var testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Many Different Layers"
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));

            /****************************************************************************************/

            contentToParse = "abcd[cats{are[\tgreat[pets] to] keep}as friends]efgh";
            testContext = new T();

            child_1_4 = new TestContextInstance(testContext)
            {
                Contents = "[pets]",
                Depth = 4,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.TextContent("pets"),
                    TestTokens.CloseBracket
                }
            };

            child_1_3 = new TestContextInstance(testContext)
            {
                Contents = "[\tgreat[pets] to]",
                Depth = 3,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.HorizontalWhitespace("\t"),
                    TestTokens.TextContent("great"),
                    TestTokens.ChildContext("[pets]", child_1_4),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("to"),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_4
                }
            };

            child_1_2 = new TestContextInstance(testContext)
            {
                Contents = "{are[\tgreat[pets] to] keep}",
                Depth = 2,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBrace,
                    TestTokens.TextContent("are"),
                    TestTokens.ChildContext("[\tgreat[pets] to]", child_1_3),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("keep"),
                    TestTokens.CloseBrace
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_3
                }
            };

            child_1_1 = new TestContextInstance(testContext)
            {
                Contents = "[cats{are[\tgreat[pets] to] keep}as friends]",
                Depth = 1,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.TextContent("cats"),
                    TestTokens.ChildContext("{are[\tgreat[pets] to] keep}", child_1_2),
                    TestTokens.TextContent("as"),
                    TestTokens.HorizontalWhitespace(" "),
                    TestTokens.TextContent("friends"),
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    child_1_2
                }
            };

            child = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("abcd"),
                    TestTokens.ChildContext("[cats{are[\tgreat[pets] to] keep}as friends]", child_1_1),
                    TestTokens.TextContent("efgh")
                },
                ChildContexts = new List<TestContextInstance>()
                { 
                    child_1_1
                }
            };

            expected = child;

            testArgs = new TestParseArgs<T>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Many Different Layers with random text noise."
            };

            args.Add(PogTreeTestHelper.ToObjArray(testArgs));
        }
    }
}
