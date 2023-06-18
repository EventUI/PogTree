/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree
{
    internal class TokenContextInstanceLocation
    {
        public TokenContextInstance ContextInstance { get; set; } = null;

        public int Position { get; set; } = 0;

        public int Depth { get; set; } = 0;
    }
}
