/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using BasicTests.Theories;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using PogTreeTest.Theories;

namespace PogTreeTest.Specs
{
    public class Spec_Sparse_Parse
    {
        protected ITestOutputHelper _output = null;

        public Spec_Sparse_Parse(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory(DisplayName = "Basic sparse parse tests.")]
        [ClassData(typeof(Theory_Sparse_Parse<SparseContext<TestContext>, TestContext>))]
        public void ParseDefaultTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.CompareParseResults(testParseArgs);
        }

        [Theory(DisplayName = "Seek ahead sparse parse tests.")]
        [ClassData(typeof(Theory_Sparse_Parse<SparseContext<SeekAheadContext>, SeekAheadContext>))]
        public void SeekAheadDefaultTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            PogTreeTestHelper.CompareParseResults(testParseArgs);
        }
    }
}
