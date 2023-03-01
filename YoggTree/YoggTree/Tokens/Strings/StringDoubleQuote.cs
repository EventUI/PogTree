﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Contexts;
using YoggTree.Core;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Strings
{
    public class StringDoubleQuote : TokenDefinition, IContextStarter, IContextEnder, IStringToken
    {
        public StringDoubleQuote()
            : base(TokenRegexStore.DoubleQuote, "\"")
        {
        }

        public string ContextStartKey { get; } = "StringDoubleQuote";

        public string ContextEndKey { get; } = "StringDoubleQuote";

        public override TokenParseContext CreateContext(TokenParseContext parent, TokenInstance start)
        {
            return new LiteralContentContext(parent, start, LiteralContentEscapeCharacterFlags.Backslash);
        }
    }
}
