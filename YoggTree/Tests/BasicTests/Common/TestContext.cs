/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogTree.Core.Tokens;

namespace PogTreeTest.Common
{
    public class TestContext : TokenContextDefinition
    {
        public TestContext()
            : base("TestContext")
        {
            AddTokens();
        }

        protected TestContext(string name) 
            : base(name)
        {
            AddTokens();
        }

        private void AddTokens()
        {
            AddTokens(new List<Type>()
            {
                typeof(OpenBracketToken),
                typeof(CloseBracketToken),
                typeof(OpenCurlyBraceToken),
                typeof(CloseCurlyBraceToken),
                typeof(StringDoubleQuoteToken),
                typeof(StringGraveToken),
                typeof(WhitespaceHorizontalToken),
                typeof(WhitespaceVerticalToken),
                typeof(BackslashToken),
                typeof(ForwardSlashToken),
                typeof(CloseParenthesisToken),
                typeof(OpenParenthesisToken)
            });
        }
    }

    public class EmptyContext : TokenContextDefinition
    {
        public EmptyContext()
            : base("Empty")
        {

        }
    }

    public class SeekAheadContext : TestContext
    {
        public SeekAheadContext()
            :base("SeekAhead")
        {
        }

        public override bool StartsNewContext(TokenInstance tokenInstance)
        {
            var nextInstance = tokenInstance.PeekNextToken();
            List<TokenInstance> tokens = new List<TokenInstance>();
            tokens.Add(tokenInstance);

            while (nextInstance != null)
            {                
                if (nextInstance != null) tokens.Add(nextInstance); 
                nextInstance = nextInstance.PeekNextToken();
            }

            return base.StartsNewContext(tokenInstance);
        }
    }

    public class SparseContext<TChildContext> : TokenContextDefinition where TChildContext : TokenContextDefinition, new()
    {
        public SparseContext()
            : base("SparseContext")
        {
            AddTokens(new List<Type>()
            {
                typeof(SparseOpenBracketToken<TChildContext>),
                typeof(SparseCloseBracketToken),
            });
        }
    }
}
