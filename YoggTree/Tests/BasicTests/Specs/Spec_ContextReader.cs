using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using YoggTreeTest.Theories;

namespace YoggTreeTest.Specs
{
    public class Spec_ContextReader
    {
        protected ITestOutputHelper _output = null;

        public Spec_ContextReader(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(Theory_ContextReader))]
        public void BasicReaderTest(TestIterationArgs args)
        {
            YoggTreeTestHelper.CompareReaderResults(args);
        }
    }
}
