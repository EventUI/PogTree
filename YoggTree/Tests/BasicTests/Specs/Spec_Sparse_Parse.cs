using BasicTests.Theories;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using YoggTreeTest.Theories;

namespace YoggTreeTest.Specs
{
    public class Spec_Sparse_Parse
    {
        protected ITestOutputHelper _output = null;

        public Spec_Sparse_Parse(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory(DisplayName = "Basic parse tests.")]
        [ClassData(typeof(Theory_Sparse_Parse<SparseContext<TestContext>, TestContext>))]
        public void ParseDefaultTests<T>(TestParseArgs<T> testParseArgs) where T : TokenContextDefinition, new()
        {
            _output.WriteLine($"Testing: {testParseArgs.TestName}");

            YoggTreeTestHelper.CompareParseResults(testParseArgs);
        }
    }
}
