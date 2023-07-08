/**Copyright (c) 2023 Richard H Stannard

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
