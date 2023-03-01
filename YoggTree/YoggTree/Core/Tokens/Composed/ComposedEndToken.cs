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
using YoggTree.Core.Tokens.Composed;

namespace YoggTree.Core.Tokens.Composed
{
    public class ComposedEndToken : ComposedTokenBase, IContextEnder
    {
        protected string _contextEndKey = null;

        public string ContextEndKey { get { return _contextEndKey; } }

        public ComposedEndToken(Regex token, string name, string contextEndKey)
            : base(token, name)
        {
            if (string.IsNullOrEmpty(contextEndKey) == true) throw new ArgumentException(nameof(contextEndKey) + " cannot be null or empty.");
            _contextEndKey = contextEndKey;
        }
    }
}
