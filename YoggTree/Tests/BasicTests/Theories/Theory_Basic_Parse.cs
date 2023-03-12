/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTreeTest.Common;

namespace BasicTests.Theories
{
    public class Theory_Basic_Parse : IEnumerable<object[]>
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
            TestContextInstance expected = new TestContextInstance(new TestContext())
            {
                Contents = contentToParse,
                Depth = 0
            };

            var testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Empty String Test"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            contentToParse = " ";
            expected = expected with
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    new TestTokenInstance(StandardTokens.WhitespaceHorizontal)
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

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

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

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));                
        }

        private static void AddSimpleHierarchyTests(List<object[]> args)
        {
            string contentToParse = "[]";
            TestContext testContext = new TestContext();
            TestContextInstance expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket , 
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    new TestContextInstance(testContext)
                    {
                        Contents = "[]",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBracket, 
                            TestTokens.CloseBracket
                        }
                    }
                }
            };

            var testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy One Layer Test"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            contentToParse = "[]{}";
            testContext = new TestContext();
            expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket,
                    TestTokens.OpenBrace,
                    TestTokens.CloseBrace
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    new TestContextInstance(testContext)
                    {
                        Contents = "[]",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBracket,
                            TestTokens.CloseBracket
                        }
                    },
                    new TestContextInstance(testContext)
                    {
                        Contents = "{}",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBrace,
                            TestTokens.CloseBrace,
                        }
                    }
                }
            };

            testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Two Peers One Layer Test"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            contentToParse = "[[]]";
            testContext = new TestContext();
            expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    new TestContextInstance(testContext)
                    {
                        Contents = "[[]]",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBracket,
                            TestTokens.OpenBracket,
                            TestTokens.CloseBracket,
                            TestTokens.CloseBracket
                        },
                        ChildContexts= new List<TestContextInstance>()
                        {
                            new TestContextInstance(testContext)
                            {
                                Contents = "[]",
                                Depth = 2,
                                Tokens = new List<TestTokenInstance>()
                                {
                                    TestTokens.OpenBracket,
                                    TestTokens.CloseBracket
                                }
                            }
                        }
                    }                    
                }
            };

            testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Two layers of the same hierarchy."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));
        }

        private static void AddComplexHierarchyTests(List<object[]> args)
        {
            string contentToParse = "[{[[()]]}]";
            TestContext testContext = new TestContext();

            TestContextInstance expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    new TestContextInstance(testContext)
                    {
                        Contents = "[{[[()]]}]",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBracket,
                            TestTokens.OpenBrace,
                            TestTokens.CloseBrace,
                            TestTokens.CloseBracket
                        },
                        ChildContexts = new List<TestContextInstance>()
                        {
                            new TestContextInstance(testContext)
                            {
                                Contents = "{[[()]]}",
                                Depth = 2,
                                Tokens = new List<TestTokenInstance>()
                                {
                                    TestTokens.OpenBrace,
                                    TestTokens.OpenBracket,
                                    TestTokens.CloseBracket,
                                    TestTokens.CloseBrace                                   
                                },
                                ChildContexts = new List<TestContextInstance>()
                                {
                                    new TestContextInstance(testContext)
                                    {
                                        Contents = "[[()]]",
                                        Depth = 3,
                                        Tokens = new List<TestTokenInstance>()
                                        {
                                            TestTokens.OpenBracket,
                                            TestTokens.OpenBracket,
                                            TestTokens.CloseBracket,
                                            TestTokens.CloseBracket
                                        },
                                        ChildContexts = new List<TestContextInstance>()
                                        {
                                            new TestContextInstance(testContext)
                                            {
                                                Contents = "[()]",
                                                Depth = 4,
                                                Tokens = new List<TestTokenInstance>()
                                                {
                                                    TestTokens.OpenBracket,
                                                    TestTokens.OpenParens,
                                                    TestTokens.CloseParens,
                                                    TestTokens.CloseBracket
                                                },
                                                ChildContexts= new List<TestContextInstance>()
                                                {
                                                    new TestContextInstance(testContext)
                                                    {
                                                        Contents = "()",
                                                        Depth = 5,
                                                        Tokens = new List<TestTokenInstance>()
                                                        {
                                                            TestTokens.OpenParens,
                                                            TestTokens.CloseParens
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Many Different Layers"
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));

            contentToParse = "abcd[cats{are[\tgreat[pets] to] keep}not lonely]efgh";
            testContext = new TestContext();

            expected = new TestContextInstance(testContext)
            {
                Contents = contentToParse,
                Depth = 0,
                Tokens = new List<TestTokenInstance>()
                {
                    TestTokens.TextContent("abcd"),
                    TestTokens.OpenBracket,
                    TestTokens.CloseBracket,
                    TestTokens.TextContent("efgh")
                },
                ChildContexts = new List<TestContextInstance>()
                {
                    new TestContextInstance(testContext)
                    {
                        Contents = "[cats{are[\tgreat[pets] to] keep}not lonely]",
                        Depth = 1,
                        Tokens = new List<TestTokenInstance>()
                        {
                            TestTokens.OpenBracket,
                            TestTokens.TextContent("cats"),
                            TestTokens.OpenBrace,
                            TestTokens.CloseBrace,
                            TestTokens.TextContent("not"),
                            TestTokens.HorizontalWhitespace(" "),
                            TestTokens.TextContent("lonely"),
                            TestTokens.CloseBracket
                        },
                        ChildContexts = new List<TestContextInstance>()
                        {
                            new TestContextInstance(testContext)
                            {
                                Contents = "{are[\tgreat[pets] to] keep}",
                                Depth = 2,
                                Tokens = new List<TestTokenInstance>()
                                {
                                    TestTokens.OpenBrace,
                                    TestTokens.TextContent("are"),
                                    TestTokens.OpenBracket,
                                    TestTokens.CloseBracket,
                                    TestTokens.HorizontalWhitespace(" "),
                                    TestTokens.TextContent("keep"),
                                    TestTokens.CloseBrace
                                },
                                ChildContexts = new List<TestContextInstance>()
                                {
                                    new TestContextInstance(testContext)
                                    {
                                        Contents = "[\tgreat[pets] to]",
                                        Depth = 3,
                                        Tokens = new List<TestTokenInstance>()
                                        {
                                            TestTokens.OpenBracket,
                                            TestTokens.HorizontalWhitespace("\t"),
                                            TestTokens.TextContent("great"),
                                            TestTokens.OpenBracket,
                                            TestTokens.CloseBracket,
                                            TestTokens.HorizontalWhitespace(" "),
                                            TestTokens.TextContent("to"),
                                            TestTokens.CloseBracket
                                        },
                                        ChildContexts = new List<TestContextInstance>()
                                        {
                                            new TestContextInstance(testContext)
                                            {
                                                Contents = "[pets]",
                                                Depth = 4,
                                                Tokens = new List<TestTokenInstance>()
                                                {
                                                    TestTokens.OpenBracket,
                                                    TestTokens.TextContent("pets"),
                                                    TestTokens.CloseBracket
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            testArgs = new TestParseArgs<TestContext>()
            {
                Expected = expected,
                ContentToParse = contentToParse,
                TestName = "Hierarchy Many Different Layers with random text noise."
            };

            args.Add(YoggTreeTestHelper.ToObjArray(testArgs));
        }
    }
}
