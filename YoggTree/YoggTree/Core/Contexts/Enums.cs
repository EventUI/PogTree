/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Contexts
{
    public enum LiteralContentEscapeCharacterFlags
    {
        None = 0,
        Backslash = 1,
        DoubleBackslash = 2
    }
}
