/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "(" characters.
    /// </summary>
    public class Parenthesis_Open : TokenDefinition
    {
        public Parenthesis_Open()
            :base(TokenRegexStore.Parenthesis_Open, "(", TokenTypeFlags.ContextStarter, "Parenthesis")
        {

        }
    }

    /// <summary>
    /// Token for finding ")" characters.
    /// </summary>
    public class Parenthesis_Close : TokenDefinition
    {
        public Parenthesis_Close()
            : base(TokenRegexStore.Parenthesis_Close, ")", TokenTypeFlags.ContextEnder, "Parenthesis")
        {

        }
    }
}
