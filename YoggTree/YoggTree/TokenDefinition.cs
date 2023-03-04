/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using YoggTree.Core.DelegateSet;
using YoggTree.Core.Interfaces;
using YoggTree.Core.Tokens;

namespace YoggTree
{
    /// <summary>
    /// Represents the definition of a token found in a string.
    /// </summary>
    public abstract class TokenDefinition
    {
        private Guid _id = Guid.NewGuid();
        private DelegateItemOridinalProvider _counterProvider = new DelegateItemOridinalProvider();
        private DelegateSetCollection<CanComeAfterPredicate<TokenDefinition>, TokenDefinition> _canComeAfters = null;
        private DelegateSetCollection<CanComeBeforePredicate<TokenDefinition>, TokenDefinition> _canComeBefores = null;
        private DelegateSetCollection<IsValidInstancePredicate<TokenDefinition>, TokenDefinition> _isValidInstances = null;
        private DelegateSetCollection<CreateTokenParseContext<TokenDefinition>, TokenDefinition> _tokenParseContextFactories = null;

        protected readonly string _name = null;
        protected readonly Regex _token = null;
        protected HashSet<string> _tags = new HashSet<string>();
        protected readonly TokenTypeFlags _flags = TokenTypeFlags.Defaul;
        protected readonly string _contextKey = null;

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

        public TokenTypeFlags Flags
        {
            get
            {
                return _flags;
            }
        }

        public string ContextKey
        {
            get { return _contextKey; }
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TokenDefinition(Regex token, string name, TokenTypeFlags flags, string contextKey = null)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(name) == true) throw new ArgumentException(nameof(name));
            if (flags.HasFlag(TokenTypeFlags.ContextStarter) || flags.HasFlag(TokenTypeFlags.ContextEnder))
            {
                if (string.IsNullOrEmpty(contextKey) == true) throw new ArgumentException("When using ContextStarter or ContextEnder flags, the contextKey must be a non-empty string.");
                _contextKey = contextKey;
            }

            _name = name;
            _token = token;
            _flags = flags;
        }

        /// <summary>
        /// Determines whether or not a token can come after the previous token found in the context's Content.
        /// </summary>
        /// <param name="previousToken">The token found immediately before this one.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        internal bool CanComeAfter(TokenInstance previousToken)
        {
            if (_canComeAfters != null)
            {
                var dele = _canComeAfters.GetFirstDelegate(previousToken.TokenDefinition);
                if (dele != null) return dele(previousToken, previousToken.TokenDefinition);
            }

            return HandleCanComeAfter(previousToken);
        }

        /// <summary>
        /// Determines whether or not a token can come before the next token found in the content's Content.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal bool CanComeBefore(TokenInstance nextToken)
        {
            if (_canComeBefores != null)
            {
                var dele = _canComeBefores.GetFirstDelegate(nextToken.TokenDefinition);
                if (dele != null) return dele(nextToken, nextToken.TokenDefinition);
            }

            return HandleCanComeBefore(nextToken);
        }

        /// <summary>
        /// Determines if the token instance that was found is "valid" and does not fall into some exception case.
        /// </summary>
        /// <param name="instance">The current token instance.</param>
        /// <param name="context">The context in which the token was found.</param>
        /// <returns></returns>
        internal bool IsValidInstance(TokenInstance instance)
        {
            if (_isValidInstances != null)
            {
                var dele = _isValidInstances.GetFirstDelegate(instance.TokenDefinition);
                if (dele != null) return dele(instance, instance.TokenDefinition);
            }

            return HandleIsValidInstance(instance);
        }

        public TokenContextInstance CreateContext(TokenInstance start)
        {
            if (_tokenParseContextFactories != null)
            {
                var dele = _tokenParseContextFactories.GetFirstDelegate(start.TokenDefinition);
                if (dele == null) return dele(start.Context.Parent, start, start.TokenDefinition);
            }

            return HandleCreateContext(start);
        }

        protected internal void AddCheckCanComeAfter<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeAfterDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_canComeAfters == null) _canComeAfters = new DelegateSetCollection<CanComeAfterPredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _canComeAfters.AddHandler<TTokenDef>(comeAfterDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _canComeAfters.AddHandler<TTokenDef>(comeAfterDele);
            }
        }

        protected internal void AddCheckCanComeBefore<TTokenDef>(Func<TokenInstance, TTokenDef, bool> comeBeforeDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_canComeBefores == null) _canComeBefores = new DelegateSetCollection<CanComeBeforePredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _canComeBefores.AddHandler<TTokenDef>(comeBeforeDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _canComeBefores.AddHandler<TTokenDef>(comeBeforeDele);
            }
        }

        protected internal void AddCheckIsValidTokenInstance<TTokenDef>(Func<TokenInstance, TTokenDef, bool> isValidDele, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_isValidInstances == null) _isValidInstances = new DelegateSetCollection<IsValidInstancePredicate<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _isValidInstances.AddHandler<TTokenDef>(isValidDele, iTokenDef => shouldHandle((TTokenDef)iTokenDef));
            }
            else
            {
                _isValidInstances.AddHandler<TTokenDef>(isValidDele);
            }
        }

        protected internal void AddTokenParseContextFactory<TTokenDef>(Func<TokenContextInstance, TokenInstance, TTokenDef, TokenContextInstance> contextFactory, Func<TTokenDef, bool> shouldHandle = null) where TTokenDef : TokenDefinition
        {
            if (_tokenParseContextFactories == null) _tokenParseContextFactories = new DelegateSetCollection<CreateTokenParseContext<TokenDefinition>, TokenDefinition>(_counterProvider);

            if (shouldHandle != null)
            {
                _tokenParseContextFactories.AddHandler<TTokenDef>(contextFactory, tokenDef => shouldHandle((TTokenDef)tokenDef));
            }
            else
            {
                _tokenParseContextFactories.AddHandler<TTokenDef>(contextFactory);
            }
        }

        protected virtual bool HandleCanComeAfter(TokenInstance previousToken)
        {
            return true;
        }

        protected virtual bool HandleCanComeBefore(TokenInstance nextToken)
        {
            return true;
        }

        protected virtual bool HandleIsValidInstance(TokenInstance instance)
        {
            return true;
        }

        protected virtual TokenContextInstance HandleCreateContext(TokenInstance start)
        {
            return new TokenContextInstance(start.Context.TokenContextDefinition, start.Context, start);
        }

        protected internal void AddTag(string tag)
        {
            if (tag == null) return;
            if (_tags.Contains(tag) == false)
            {
                _tags.Add(tag);
            }
        }

        protected internal void AddTags(IEnumerable<string> tags)
        {
            if (tags == null) return;
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}-{_name}-({_token.ToString()})";
        }
    }
}