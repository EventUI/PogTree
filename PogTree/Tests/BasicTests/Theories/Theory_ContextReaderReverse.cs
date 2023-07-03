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
    internal class Theory_ContextReaderReverse : Theory_ContextReader, IEnumerable<object[]>
    {
        public Theory_ContextReaderReverse()
        {
            _reverse = true;
        }  
    }
}
