/**Copyright (c) 2023 Richard H Stannard

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
using YoggTree.Tokens.Basic;
using YoggTree.Tokens.Composed;

namespace YoggTree.Tokens
{
    public class TokenBuilder
    {
        private ComposedTokenBase _token = null;

        internal TokenBuilder(ComposedTokenBase token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));    
            _token = token;
        }

        public ComposedTokenBase GetToken()
        {
            return (ComposedTokenBase)_token;
        }

        public TokenBuilder AddCanComeBeforeCheck<TToken>(Func<TokenInstance, TToken, bool> canComeBefore) where TToken: TokenDefinition
        {
            _token.AddCheckCanComeBefore(canComeBefore);
            return this;
        }

        public TokenBuilder AddCanComeAfterCheck<TToken>(Func<TokenInstance, TToken, bool> canComeAfter) where TToken : TokenDefinition
        {
            _token.AddCheckCanComeAfter(canComeAfter);
            return this;
        }

        public TokenBuilder AddIsValidCheck<TToken>(Func<TokenInstance, TToken, bool> isValid) where TToken : TokenDefinition
        {
            _token.AddCheckIsValidTokenInstance(isValid);
            return this;
        }

        public TokenBuilder AddCanComeBeforeCheck<TToken>(Func<TokenInstance, TToken, bool> canComeBefore, Func<TToken, bool> shouldHandle) where TToken : TokenDefinition
        {
            _token.AddCheckCanComeBefore(canComeBefore, shouldHandle);
            return this;
        }

        public TokenBuilder AddCanComeAfterCheck<TToken>(Func<TokenInstance, TToken, bool> canComeAfter, Func<TToken, bool> shouldHandle) where TToken : TokenDefinition
        {
            _token.AddCheckCanComeAfter(canComeAfter, shouldHandle);
            return this;
        }

        public TokenBuilder AddIsValidCheck<TToken>(Func<TokenInstance, TToken, bool> isValid, Func<TToken, bool> shouldHandle) where TToken : TokenDefinition
        {
            _token.AddCheckIsValidTokenInstance(isValid, shouldHandle);
            return this;
        }

        public static TokenBuilder Create(Regex regex, string name)
        {
            return new TokenBuilder(new ComposedToken(regex, name));
        }

        public static TokenBuilder Create(Regex regex, string name, TokenCreateMode mode, string contextKey = null)
        {
            ComposedTokenBase token = null;

            if (mode == TokenCreateMode.Default)
            {
                token = new ComposedToken(regex, name);
            }
            else if (mode == TokenCreateMode.ContextStarter)
            {
                token = new ComposedStartToken(regex, name, contextKey);
            }
            else if (mode == TokenCreateMode.ContextEnder)
            {
                token = new ComposedEndToken(regex, name, contextKey);
            }
            else if (mode == TokenCreateMode.ContextStarterAndEnder)
            {
                token = new ComposedStartAndEndToken(regex, name, contextKey);
            }
            else
            {
                throw new ArgumentException(nameof(mode));
            }

            return new TokenBuilder(token);
        }

        public static TokenBuilder Create<TToken>() where TToken : ComposedTokenBase, new()
        {
            return new TokenBuilder(new TToken());
        }
    }
}
