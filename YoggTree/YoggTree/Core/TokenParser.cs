/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core
{
    public static class TokenParser
    {
        public static TokenContextInstance Parse(TokenContextDefinition contextDefition, string contents)
        {
            var parseSession = new TokenParseSession(new TokenContextInstance(contextDefition, contents));
            parseSession.RootContext.WalkContent();

            return parseSession.RootContext;
        }
    }
}
