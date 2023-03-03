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
using YoggTree.Core.Contexts;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core.Tokens.Strings
{
    public class StringGrave : TokenDefinition, IContextStarter, IContextEnder, IStringToken
    {
        public StringGrave()
            : base(TokenRegexStore.Grave, "`")
        {
        }

        public string ContextStartKey { get; } = "StringGrave";

        public string ContextEndKey { get; } = "StringGrave";

    }
}