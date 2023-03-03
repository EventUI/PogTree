﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;

namespace YoggTree.Core.Tokens.Whitespace
{
    public class WhitespaceHorizontal : TokenDefinition, IWhitespaceToken
    {
        public WhitespaceHorizontal() 
            : base(TokenRegexStore.Whitespace_Horizontal, "<horizontal whitespace>")
        {
        }
    }
}