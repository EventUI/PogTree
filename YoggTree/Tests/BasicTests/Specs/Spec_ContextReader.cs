using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace YoggTreeTest.Specs
{
    public class Spec_ContextReader
    {
        protected ITestOutputHelper _output = null;

        public Spec_ContextReader(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BasicTest()
        {

        }
    }
}
