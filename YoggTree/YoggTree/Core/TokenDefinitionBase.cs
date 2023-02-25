/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoggTree.Core
{
    /// <summary>
    /// Represents the definition of a token found in a string.
    /// </summary>
    public abstract class TokenDefinitionBase
    {
        private Guid _id = Guid.NewGuid();

        protected readonly string _name = null;
        protected readonly Regex _token = null;
        protected bool _startsNewContext = false;
        protected TokenDefinitionBase _contextEndToken = null;

        /// <summary>
        /// The unique ID of this token definition.
        /// </summary>
        public Guid ID
        {
            get { return _id; }
        }

        /// <summary>
        /// The human-readable name of the token.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The regular expression used to find the token in the string.
        /// </summary>
        public Regex Token
        {
            get { return _token; }
        }

        /// <summary>
        /// Creates a new TokenDefinitionBase.
        /// </summary>
        /// <param name="token">The regular expression used to identify the token/</param>
        /// <param name="name">The human readable name of the token.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenDefinitionBase(Regex token, string name)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (String.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));

            _name = name;
            _token = token;
        }

        /// <summary>
        /// Gets the next previousToken of this token in the context's Content.
        /// </summary>
        /// <param name="currentPosition">The position in the content to search from.</param>
        /// <param name="context">The context to search for the next token in.</param>
        /// <param name="startingIndex">Optional. The starting index in the array of TokenInstances to begin searching at.</param>
        /// <returns></returns>
        public (TokenInstance Instance, int Index) GetNextToken(int currentPosition, TokenParseContextBase context, int startingIndex = 0)
        {
            if (context.ParseSession.AllTokenInstances.TryGetValue(ID, out IReadOnlyList<TokenInstance> tokens) == false) return (null, -1);

            for (int x = startingIndex; x < tokens.Count; x++) 
            { 
                TokenInstance tokenInstance = tokens[x];
                if (tokenInstance.TokenStartIndex >= currentPosition) return (tokenInstance, x);
            }

            return (null, -1);
        }

        /// <summary>
        /// Determines whether or not a token can come after the previous token found in the context's Content.
        /// </summary>
        /// <param name="previousToken">The token found immediately before this one.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        public virtual bool CanComeAfter(TokenInstance previousToken, TokenParseContextBase context)
        {
            return true;
        }

        /// <summary>
        /// Determines whether or not a token can come before the next token found in the content's Content.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool CanComeBefore(TokenInstance nextToken, TokenParseContextBase context)
        {
            return true;
        }

        /// <summary>
        /// Determines if the token instance that was found is "valid" and does not fall into some exception case.
        /// </summary>
        /// <param name="instance">The current token instance.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        public virtual bool IsValidInstance(TokenInstance instance, TokenParseContextBase context)
        {
            return true;
        }

        public override string ToString()
        {
            return $"{GetType().Name}-{_name}-({_token.ToString()})";
        }
    }

    public sealed class EmptyTokenDefinition : TokenDefinitionBase
    {
        internal EmptyTokenDefinition()
            : base(new Regex("[^.]"), "Empty")
        {
        }
    }

    /// <summary>
    /// Special TokenDefinitionBase for plain text that exists between other TokenInstances.
    /// </summary>
    public sealed class ContentToken : TokenDefinitionBase
    {
        internal ContentToken()
            : base(new Regex(""), "<content>")
        {
        }
    }
}