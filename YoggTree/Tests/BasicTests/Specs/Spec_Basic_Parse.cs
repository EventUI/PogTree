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
using PogTreeTest.Common;

namespace PogTreeTest.Specs
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

            PogTreeTestHelper.CompareParseResults(testParseArgs);
        }

        [Theory(DisplayName = "Token seek tests.")]
        [ClassData(typeof(Theory_Basic_Parse<TestContext>))]
        public void TokenSeekTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.CompareSeekResults(testParseArgs);
        }

        [Theory(DisplayName = "Token list matching tests.")]
        [ClassData(typeof(Theory_Basic_Parse<TestContext>))]
        public void MatchTokenList<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.ValidateSeekResults(testParseArgs);
        }

        [Theory(DisplayName = "Token lazy seeking tests.")]
        [ClassData(typeof(Theory_Basic_Parse<SeekAheadContext>))]
        public void TestLazySeeking<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.CompareParseResults(testParseArgs);
        }

        [Theory(DisplayName = "Token list matching lazy seeking behavior tests.")]
        [ClassData(typeof(Theory_Basic_Parse<SeekAheadContext>))]
        public void MatchLazyTokenList<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.ValidateSeekResults(testParseArgs);
        }

        [Theory(DisplayName = "Lazy token seek tests.")]
        [ClassData(typeof(Theory_Basic_Parse<SeekAheadContext>))]
        public void LazyTokenSeekTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.CompareSeekResults(testParseArgs);
        }

    }
}
