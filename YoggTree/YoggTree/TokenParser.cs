/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree
{
    public class TokenParser
    {
        private TokenContextRegistry _contextRegistry = null;

        public TokenContextRegistry ContextRegistry
        {
            get
            {
                return _contextRegistry;
            }
        }

        public TokenParser()
        {
            _contextRegistry = new TokenContextRegistry();
        }

        public TokenParser(TokenContextRegistry contextRegistry)
        {
            if (contextRegistry == null) throw new ArgumentNullException(nameof(contextRegistry));
            _contextRegistry = contextRegistry;
        }

        public TokenContextInstance Parse(TokenContextDefinition contextDefition, string contents)
        {
            var parseSession = new TokenParseSession(new TokenContextInstance(contextDefition, contents), new TokenContextRegistry(_contextRegistry));
            parseSession.RootContext.WalkContent();

            return parseSession.RootContext;
        }

        public TokenContextInstance Parse<T>(string contents) where T : TokenContextDefinition, new()
        {
            return Parse(new T(), contents);
        }
    }
}
