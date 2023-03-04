/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Text.RegularExpressions;
using YoggTree.Core.Interfaces;

namespace YoggTree.Core
{
    /// <summary>
    /// Represents the definition of a token found in a string.
    /// </summary>
    public class TokenDefinition : ITokenDefinition
    {
        private Guid _id = Guid.NewGuid();

        protected readonly string _name = null;
        protected readonly Regex _token = null;
        protected HashSet<string> _tags = new HashSet<string>();

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
        /// Tags that mark a token as belonging to a category of tokens.
        /// </summary>
        public HashSet<string> Tags
        { 
            get 
            { 
                return _tags; 
            }
        }   

        /// <summary>
        /// Creates a new TokenDefinition.
        /// </summary>
        /// <param name="token">The regular expression used to identify the token/</param>
        /// <param name="name">The human readable name of the token.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenDefinition(Regex token, string name)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (String.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));

            _name = name;
            _token = token;
        }


        /// <summary>
        /// Determines whether or not a token can come after the previous token found in the context's Content.
        /// </summary>
        /// <param name="previousToken">The token found immediately before this one.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        public virtual bool CanComeAfter(TokenInstance previousToken)
        {
            return true;
        }

        /// <summary>
        /// Determines whether or not a token can come before the next token found in the content's Content.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool CanComeBefore(TokenInstance nextToken)
        {
            return true;
        }

        /// <summary>
        /// Determines if the token instance that was found is "valid" and does not fall into some exception case.
        /// </summary>
        /// <param name="instance">The current token instance.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        public virtual bool IsValidInstance(TokenInstance instance)
        {
            return true;
        }

        /// <summary>
        /// Creates a new TokenContextInstance that corresponds to the TokenDefinition.
        /// </summary>
        /// <param name="parent">The parent context that is spawning this one.</param>
        /// <param name="start">The token instance that triggered the creation of the new context.</param>
        /// <returns></returns>
        public virtual TokenContextInstance CreateContext(TokenContextInstance parent, TokenInstance start)
        {
            return new TokenContextInstance(parent.TokenContextDefinition, parent, start);
        }

        public override string ToString()
        {
            return $"{GetType().Name}-{_name}-({_token.ToString()})";
        }
    }
}