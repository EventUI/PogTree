using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Tokens.Basic;

namespace YoggTree.Tokens
{
    public class TokenListBuilder
    {
        protected List<TokenDefinitionBase> _tokens = new List<TokenDefinitionBase>();
        private HashSet<string> _tokenPatterns = new HashSet<string>();

        public TokenListBuilder() 
        {
        }

        public TokenListBuilder(IEnumerable<TokenDefinitionBase> tokens)
        {
            if (tokens != null)
            {
               foreach (var token in tokens)
                {
                    AddToken(token);
                }
            }
        }

        public List<TokenDefinitionBase> GetTokens()
        {
            return new List<TokenDefinitionBase>(_tokens);
        }

        public TokenListBuilder AddToken(TokenDefinitionBase token)
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

        public TokenListBuilder AddTokenContextStart(Regex regex, string name, string startEndContextKey)
        {
            return AddToken(new BasicStartToken(regex, name, startEndContextKey));
        }

        public TokenListBuilder AddTokenContextEnd(Regex regex, string name, string endEndContextKey)
        {
            return AddToken(new BasicEndToken(regex, name, endEndContextKey));
        }

        public TokenListBuilder AddTokenContextStartAndEnd(Regex regex, string name, string contextKey)
        {
            return AddToken(new BasicStartAndEndToken(regex, name, contextKey));    
        }

        public TokenDefinitionBase GetTokenFromRegex(string pattern)
        {
            if (pattern == null || _tokenPatterns.Contains(pattern) == false) return null;

            foreach (var token in _tokens)
            {
                if (token.Token.ToString() == pattern) return token;
            }

            return null;
        }

        public TokenDefinitionBase GetTokenFromRegex(Regex regex)
        {
            return GetTokenFromRegex(regex?.ToString());
        }
    }
}
