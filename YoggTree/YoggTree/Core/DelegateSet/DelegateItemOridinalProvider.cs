/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.DelegateSet
{
    internal class DelegateItemOridinalProvider
    {
        protected int _ordinal = 0;

        public int Ordinal { get { return _ordinal; } }

        public int GetNextOrdinal()
        {
            return _ordinal++;
        }
    }
}
