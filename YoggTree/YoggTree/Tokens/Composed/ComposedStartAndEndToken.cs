/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Composed
{
    public class ComposedStartAndEndToken : ComposedTokenBase, IContextStarter, IContextEnder
    {
        protected string _contextStartEndKey = null;

        public string ContextStartKey { get { return _contextStartEndKey; } }

        public string ContextEndKey { get { return _contextStartEndKey; } }

        public ComposedStartAndEndToken(Regex token, string name, string contextStartEndKey) 
            : base(token, name)
        {
            if (string.IsNullOrEmpty(contextStartEndKey) == true) throw new ArgumentException(nameof(contextStartEndKey) + " cannot be null or empty.");
            _contextStartEndKey = contextStartEndKey;
        }
    }
}
