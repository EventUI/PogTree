/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core
{
    /// <summary>
    /// Represents the containing unit for parsing a body of text.
    /// </summary>
    public abstract class TokenParseSession
    {
        private IReadOnlyList<ITokenDefinition> _roDefinitions;
        private ReadOnlyMemory<char> _contents = ReadOnlyMemory<char>.Empty;
        private ReadOnlyDictionary<Guid, IReadOnlyList<TokenInstance>> _allTokenInstances;

        protected List<ITokenDefinition> _tokenDefinitions = new List<ITokenDefinition>();
        protected TokenParseContext _rootContext = null;

        /// <summary>
        /// The full contents of the string being parsed.
        /// </summary>
        public ReadOnlyMemory<char> Contents
        {
            get
            {
                return _contents;
            }
        }

        /// <summary>
        /// All of the possible tokens that can be found in the string being parsed.
        /// </summary>
        public IReadOnlyList<ITokenDefinition> DefinedTokens { get { return _roDefinitions; } }

        /// <summary>
        /// All of the tokens contained within the
        /// </summary>
        public ReadOnlyDictionary<Guid, IReadOnlyList<TokenInstance>> AllTokenInstances
        {
            get { return _allTokenInstances; }
        }

        /// <summary>
        /// The root parsing content representing the parse results for the whole file.
        /// </summary>
        public TokenParseContext RootContext { get { return _rootContext; } }

        /// <summary>
        /// Makes a new ParseSession for the given string and token set.
        /// </summary>
        /// <param name="contents">The string content to parse.</param>
        /// <param name="tokens">All of the tokens that can be found in the content.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public TokenParseSession(IEnumerable<ITokenDefinition> tokens) 
        { 
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            _tokenDefinitions = tokens.ToList();
            _roDefinitions = _tokenDefinitions.AsReadOnly();
        }

        /// <summary>
        /// Triggers the parsing of the ParseSession's content.
        /// </summary>
        /// <returns></returns>
        public TokenParseContext Parse(string contents)
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));            
            _contents = new ReadOnlyMemory<char>(contents.ToCharArray());

            _rootContext = MakeRootContext();
            if (_rootContext == null) throw new Exception("RootContext cannot be null.");

            GetAllTokenInstances();

            _rootContext.WalkContent();
            return _rootContext;
        }

        /// <summary>
        /// Overridable function for making the Root parse context.
        /// </summary>
        /// <returns></returns>
        protected abstract TokenParseContext MakeRootContext();

        /// <summary>
        /// Gets all the token instances found in the Content by using the TokenDefinitions Regex's.
        /// </summary>
        private void GetAllTokenInstances()
        {
            Dictionary<Guid, List<TokenInstance>> tokenDic = new Dictionary<Guid, List<TokenInstance>>();

            //get every match of every token definition and build a dictionary out of the results
            foreach (var tokenDef in DefinedTokens)
            {
                List<TokenInstance> instances = null;
                if (tokenDic.ContainsKey(tokenDef.ID) == false)
                {
                    instances = new List<TokenInstance>();
                    tokenDic.Add(tokenDef.ID, instances);
                }
                else
                {
                    instances = tokenDic[tokenDef.ID];
                }

                foreach (var result in Regex.EnumerateMatches(Contents.Span, tokenDef.Token.ToString(), tokenDef.Token.Options))
                {
                    var instance = new TokenInstance(tokenDef, _rootContext, result.Index, Contents.Slice(result.Index, result.Length));
                    instances.Add(instance);
                }
            }

            //we have to do a bit of awkwardness here to transform our mutable list above into a read-only list that then gets wrapped in a read only dictionary container.
            Dictionary<Guid, IReadOnlyList<TokenInstance>> intermediary = new Dictionary<Guid, IReadOnlyList<TokenInstance>>();

            foreach (var tokenList in tokenDic)
            {
                tokenList.Value.Sort();
                intermediary.Add(tokenList.Key, tokenList.Value.AsReadOnly());
            }

            _allTokenInstances = intermediary.AsReadOnly();
        }
    }
}
