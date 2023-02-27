/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.Interfaces;
using YoggTree.Tokens.Basic;
using YoggTree.Tokens.Composed;

namespace YoggTree.Tokens
{
    public class TokenListBuilder
    {
        private List<ITokenDefinition> _tokens = new List<ITokenDefinition>();
        private HashSet<string> _tokenPatterns = new HashSet<string>();

        public TokenListBuilder() 
        {
        }

        public TokenListBuilder(IEnumerable<ITokenDefinition> tokens)
        {
            if (tokens != null)
            {
               foreach (var token in tokens)
               {
                   AddToken(token);
               }
            }
        }

        public List<ITokenDefinition> GetTokens()
        {
            return new List<ITokenDefinition>(_tokens);
        }

        public TokenListBuilder AddToken(TokenBuilder tokenBuilder)
        {
            return AddToken(tokenBuilder.GetToken());            
        }

        public TokenListBuilder AddToken(Func<ITokenDefinition> factory)
        {
            return AddToken(factory());
        }

        public TokenListBuilder AddToken<TToken>(Func<TokenBuilder, TToken> factory) where TToken : IComposedToken, new()
        {
            return AddToken(factory(TokenBuilder.Create<TToken>()));
        }

        public TokenListBuilder AddToken(Regex regex, string name, Func<TokenBuilder, ITokenDefinition> factory)
        {
            return AddToken(factory(TokenBuilder.Create(regex, name)));
        }

        public TokenListBuilder AddToken(Regex regex, string name, TokenCreateMode mode, string contextKey, Func<TokenBuilder, ITokenDefinition> factory)
        {
            return AddToken(factory(TokenBuilder.Create(regex, name, mode, contextKey)));
        }

        public TokenListBuilder AddToken(ITokenDefinition token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            if (_tokenPatterns.Contains(token.Token.ToString()) == true)
            {
                var matchingToken = GetTokenFromRegex(token.Token.ToString());
                throw new ArgumentException($"The pattern {token.Token.ToString()} already exists in token {matchingToken.Name}");
            }

            _tokenPatterns.Add(token.Token.ToString());
            _tokens.Add(token);

            return this;
        }

        public TokenListBuilder AddToken(Regex regex, string name)
        {
            return AddToken(new BasicToken(regex, name));
        }

        public TokenListBuilder AddToken(Regex regex, string name, TokenCreateMode mode, string contextKey = null)
        {
            if (mode == TokenCreateMode.Default)
            {
                return AddToken(new BasicToken(regex, name));
            }
            else if (mode == TokenCreateMode.ContextStarter)
            {
                return AddToken(new BasicStartToken(regex, name, contextKey));
            }
            else if (mode == TokenCreateMode.ContextEnder)
            {
                return AddToken(new BasicEndToken(regex, name, contextKey));
            }
            else if (mode == TokenCreateMode.ContextStarterAndEnder)
            {
                return AddToken(new BasicStartAndEndToken(regex, name, contextKey));
            }
            else
            {
                throw new ArgumentException(nameof(mode));
            }           
        }

        public ITokenDefinition GetTokenFromRegex(string pattern)
        {
            if (pattern == null || _tokenPatterns.Contains(pattern) == false) return null;

            foreach (var token in _tokens)
            {
                if (token.Token.ToString() == pattern) return token;
            }

            return null;
        }

        public ITokenDefinition GetTokenFromRegex(Regex regex)
        {
            return GetTokenFromRegex(regex?.ToString());
        }
    }
}
