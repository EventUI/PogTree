/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTreeTest.Common
{
    public class TestContext : TokenContextDefinition
    {
        public TestContext()
            : base("TestContext", StandardTokens.GetAllStandardTokens())
        {
        }
    }

    public class EmptyContext : TokenContextDefinition
    {
        public EmptyContext()
            : base("Empty")
        {

        }
    }

    public class SeekAheadContext : TokenContextDefinition
    {
        public SeekAheadContext()
            :base("SeekAhead", StandardTokens.GetAllStandardTokens())
        {

        }

        public override bool StartsNewContext(TokenInstance tokenInstance)
        {
            var nextInstance = tokenInstance.GetNextToken();

            while (nextInstance != null)
            {
                nextInstance = nextInstance.GetNextToken();
            }

            return base.StartsNewContext(tokenInstance);
        }
    }
}
