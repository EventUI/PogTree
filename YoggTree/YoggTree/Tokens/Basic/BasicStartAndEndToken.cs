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
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Basic
{
    public class BasicStartAndEndToken : TokenDefinitionBase, IContextStarter, IContextEnder, IBasicToken
    {
        private string _contextKey = null;

        public string ContextStartKey
        { 
            get { return _contextKey; } 
        }

        public string ContextEndKey
        {
            get { return _contextKey; }
        }

        public BasicStartAndEndToken(Regex token, string name, string contextKey) 
            : base(token, name)
        {
            if (string.IsNullOrEmpty(contextKey) == true) throw new ArgumentException(nameof(contextKey) + " cannot be null or empty.");

            _contextKey = contextKey;
        }
    }
}
