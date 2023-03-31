/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using BasicTests.Theories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using YoggTreeTest.Common;

namespace YoggTreeTest.Specs
{
    public class Spec_Basic_Parse
    {
        protected ITestOutputHelper _output = null;

        public Spec_Basic_Parse(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory(DisplayName = "Basic parse tests.")]
        [ClassData(typeof(Theory_Basic_Parse<TestContext>))]
        public void ParseDefaultTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            YoggTreeTestHelper.CompareParseResults(testParseArgs);
        }

        [Theory(DisplayName = "Token seek tests.")]
        [ClassData(typeof(Theory_Basic_Parse<TestContext>))]
        public void TokenSeekTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            YoggTreeTestHelper.CompareSeekResults(testParseArgs);
        }

        [Theory(DisplayName = "Token lazy seeking tests.")]
        [ClassData(typeof(Theory_Basic_Parse<SeekAheadContext>))]
        public void TestLazySeeking<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            YoggTreeTestHelper.CompareParseResults(testParseArgs);
        }
    }
}
