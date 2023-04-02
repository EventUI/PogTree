/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using YoggTree.Core.Spools;

namespace YoggTree
{
    /// <summary>
    /// Represents the containing unit for parsing a body of text.
    /// </summary>
    public class TokenParseSession
    {
        private ReadOnlyMemory<char> _contents = ReadOnlyMemory<char>.Empty;
        private Dictionary<Guid, TokenSpool> _tokenSpools = new Dictionary<Guid, TokenSpool>();
        private TokenContextInstance _rootContext = null;
        private TokenContextRegistry _contextRegistry = null;

        /// <summary>
        /// All of the TokenSpools contained within the session.
        /// </summary>
        internal Dictionary<Guid, TokenSpool> TokenSpools
        {
            get
            {
                return _tokenSpools;
            }
        }

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
        /// The root parsing content representing the parse results for the whole file.
        /// </summary>
        public TokenContextInstance RootContext
        {
            get
            {
                return _rootContext;
            }
        }

        /// <summary>
        /// The registry of replacement contexts to use during the parse operation.
        /// </summary>
        public TokenContextRegistry ContextRegistry
        {
            get
            {
                return _contextRegistry;
            }
        }

        /// <summary>
        /// Makes a new ParseSession for the given string and token set.
        /// </summary>
        /// <param name="contents">The string content to parse.</param>
        /// <param name="tokens">All of the tokens that can be found in the content.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        internal TokenParseSession(TokenContextInstance rootContext, TokenContextRegistry contextRegistry)
        {
            if (rootContext == null) throw new ArgumentNullException(nameof(rootContext));
            if (contextRegistry == null) throw new ArgumentNullException(nameof(contextRegistry));

            _contents = rootContext.Contents;
            _contextRegistry = contextRegistry;
            _rootContext = rootContext;
            _rootContext.ParseSession = this;
        }
    }
}
