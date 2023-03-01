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

namespace YoggTree.Core.Tokens.Composed
{
    public class ComposedStartToken : ComposedTokenBase, IContextStarter
    {
        protected string _contextStartKey = null;

        public string ContextStartKey { get { return _contextStartKey; } }

        public string ContextKey { get { return _contextStartKey; } }

        public ComposedStartToken(Regex token, string name, string contextStartKey)
            : base(token, name)
        {
            if (string.IsNullOrEmpty(contextStartKey) == true) throw new ArgumentException(nameof(contextStartKey) + " cannot be null or empty");
            _contextStartKey = contextStartKey;
        }
    }
}
