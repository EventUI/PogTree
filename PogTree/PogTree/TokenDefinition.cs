﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree
{
    /// <summary>
    /// Represents the definition of a token found in a string.
    /// </summary>
    public abstract class TokenDefinition : ITokenDefinition
    {
        private Guid _id = Guid.NewGuid();
        private readonly string _name = null;
        private readonly Regex _token = null;
        private readonly TokenDefinitionFlags _flags = TokenDefinitionFlags.None;
        private readonly string _contextKey = null;
        private readonly int _spoolSize = 25;

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
        /// Flags indicating special behavior to be taken when this token is encountered.
        /// </summary>
        public TokenDefinitionFlags Flags
        {
            get
            {
                return _flags;
            }
        }

        /// <summary>
        /// In the event that this token has a ContextStarter and/or a ContextEnder flag, this is the key used by the TokenContextInstance used to determine if the start token and end token of a context match.
        /// </summary>
        public string ContextKey
        {
            get { return _contextKey; }
        }

        /// <summary>
        /// When being used by a TokenContextInstance, this is the size of the buffer of results to keep at a time. The default is 25 - but Regexes with a large number of results (like whitespace) should have a higher SpoolSizes.
        /// </summary>
        public int SpoolSize
        {
            get { return _spoolSize; }
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
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));

            _name = name;
            _token = token;
        }

        /// <summary>
        /// Creates a new TokenDefinition.
        /// </summary>
        /// <param name="token">The regular expression used to identify the token/</param>
        /// <param name="name">The human readable name of the token.</param>
        /// <param name="spoolSize">The size of the spool of results to build. Large spools are more performant, smaller spools use less memory.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenDefinition(Regex token, string name, int spoolSize)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));
            if (_spoolSize < 0) throw new ArgumentOutOfRangeException(nameof(spoolSize));

            _name = name;
            _token = token;
            _spoolSize = spoolSize;

        }

        /// <summary>
        /// Creates a new TokenDefinition.
        /// </summary>
        /// <param name="token">The regular expression used to identify the token/</param>
        /// <param name="name">The human readable name of the token.</param>
        /// <param name="spoolSize">The size of the spool of results to build. Large spools are more performant, smaller spools use less memory.</param>
        /// <param name="contextKey">If this token starts or ends a context, this is the matching "key" to the corresponding start or end token.</param>
        /// <param name="flags">Flags for indicating special behavior of this token definition.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenDefinition(Regex token, string name, TokenDefinitionFlags flags, string contextKey = null, int? spoolSize = null)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));
            if (flags.HasFlag(TokenDefinitionFlags.ContextStarter) || flags.HasFlag(TokenDefinitionFlags.ContextEnder))
            {
                if (string.IsNullOrEmpty(contextKey) == true) throw new ArgumentException("When using ContextStarter or ContextEnder flags, the contextKey must be a non-empty string.");
                _contextKey = contextKey;
            }

            if (spoolSize.HasValue == true)
            {
                if (spoolSize.Value < 0) throw new ArgumentOutOfRangeException(nameof(spoolSize));
                _spoolSize = spoolSize.Value;
            }

            _name = name;
            _token = token;
            _flags = flags;
        }

        /// <summary>
        /// Determines whether or not a token can come after the previous token found in the context's Content.
        /// </summary>
        /// <param name="previousToken">The token found immediately before this one.</param>
        /// <param name="validatingToken">The token instance on which the validation method is being called.</param>
        /// <returns></returns>
        public virtual bool CanComeAfter(TokenInstance previousToken, TokenInstance validatingToken)
        {
            return true;
        }

        /// <summary>
        /// Determines whether or not a token can come before the next token found in the content's Content.
        /// </summary>
        /// <param name="nextToken">The token found immediately after this one.</param>
        /// <param name="validatingToken">The token instance on which the validation method is being called.</param>
        /// <returns></returns>
        public virtual bool CanComeBefore(TokenInstance nextToken, TokenInstance validatingToken)
        {
            return true;
        }

        /// <summary>
        /// Determines if the token instance that was found is "valid" and does not fall into some exception case.
        /// </summary>
        /// <param name="instance">The current token instance.</param>
        /// <returns></returns>
        public virtual bool IsValidInstance(TokenInstance instance)
        {
            return true;
        }

        /// <summary>
        /// Returns the type of child context to create When this token has signaled that it starts a child context.
        /// </summary>
        /// <param name="start">The token that has been flagged as the start of a new child context.</param>
        /// <returns></returns>
        public virtual TokenContextDefinition GetNewContextDefinition(TokenInstance start)
        {
            return start.Context.ContextDefinition;
        }

        /// <summary>
        /// Gets a string version of this TokenDefinition's metadata.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}-{_name}-({_token.ToString()})";
        }
    }
}