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
    internal class Theory_ContextReaderReverse : Theory_ContextReader, IEnumerable<object[]>
    {
        public Theory_ContextReaderReverse()
        {
            _reverse = true;
        }  
    }
}
