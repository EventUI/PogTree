/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree
{
    /// <summary>
    /// Represents an object used to invoke the token parsing behavior.
    /// </summary>
    public class TokenParser
    {
        private TokenContextRegistry _contextRegistry = null;

        /// <summary>
        /// A registry of tokens to replace at runtime.
        /// </summary>
        public TokenContextRegistry ContextRegistry
        {
            get
            {
                return _contextRegistry;
            }
        }

        /// <summary>
        /// creates a new TokenParser with a blank TokenContextRegistry.
        /// </summary>
        public TokenParser()
        {
            _contextRegistry = new TokenContextRegistry();
        }

        /// <summary>
        /// Creates a new instance of the TokenParser with the provided TokenContextRegistry.
        /// </summary>
        /// <param name="contextRegistry">The registry to use for all content parsed by this object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenParser(TokenContextRegistry contextRegistry)
        {
            if (contextRegistry == null) throw new ArgumentNullException(nameof(contextRegistry));
            _contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Parses a string of content using the given token context definition as a starting point. Creates an copy of the ContextRegistry to use for this parse session.
        /// </summary>
        /// <param name="contextDefition">The context definition to use to begin the parsing process. Note that this can be swapped out if the ContextRegistry contains a matching key.</param>
        /// <param name="contents">The string to parse.</param>
        /// <returns></returns>
        public TokenContextInstance Parse(TokenContextDefinition contextDefition, string contents)
        {
            if (_contextRegistry.IsEmpty == false)
            {
                var replacement = _contextRegistry.GetContext(contextDefition.GetType());
                if (replacement != null && contextDefition.GetType() != replacement.GetType())
                {
                    contextDefition = replacement;
                }
            }

            var parseSession = new TokenParseSession(new TokenContextInstance(contextDefition, contents), new TokenContextRegistry(_contextRegistry));
            parseSession.RootContext.WalkContent();

            return parseSession.RootContext;
        }

        /// <summary>
        /// Parses a string of content using the given token context definition as a starting point. Creates an copy of the ContextRegistry to use for this parse session.
        /// </summary>
        /// <typeparam name="T">>The type of context definition to use to begin the parsing process. Note that this can be swapped out if the ContextRegistry contains a matching key. Must have a parameterless constructor.</typeparam>
        /// <param name="contents">The string to parse.</param>
        /// <returns></returns>
        public TokenContextInstance Parse<T>(string contents) where T : TokenContextDefinition, new()
        {
            return Parse(new T(), contents);
        }
    }
}
