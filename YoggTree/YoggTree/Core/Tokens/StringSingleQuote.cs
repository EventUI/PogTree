/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Contexts;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens
{
    public class StringSingleQuote : TokenDefinition, IStringToken
    {
        public StringSingleQuote() 
            : base(TokenRegexStore.SingleQuote, "'", TokenTypeFlags.ContextEnder | TokenTypeFlags.ContextStarter, "StringSingleQuote") 
        { 
        }
    }
}
