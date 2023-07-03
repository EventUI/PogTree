/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree
{
    /// <summary>
    /// Represents an object used to invoke the token parsing behavior.
    /// </summary>
    public class TokenParser
    {
        private TokenContextCollection _contextRegistry = null;

        /// <summary>
        /// A registry of tokens to replace at runtime.
        /// </summary>
        public TokenContextCollection ContextRegistry
        {
            get
            {
                return _contextRegistry;
            }
        }

        /// <summary>
        /// creates a new TokenParser with a blank TokenContextCollection.
        /// </summary>
        public TokenParser()
        {
            _contextRegistry = new TokenContextCollection();
        }

        /// <summary>
        /// Creates a new instance of the TokenParser with the provided TokenContextCollection.
        /// </summary>
        /// <param name="contextRegistry">The registry to use for all content parsed by this object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenParser(TokenContextCollection contextRegistry)
        {
            if (contextRegistry == null) throw new ArgumentNullException(nameof(contextRegistry));
            _contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Parses a string of content using the given token context definition as a starting point. Creates an copy of the ContextRegistry to use for this parse session.
        /// </summary>
        /// <param name="contextDefinition">The context definition to use to begin the parsing process. Note that this can be swapped out if the ContextRegistry contains a matching key.</param>
        /// <param name="contents">The string to parse.</param>
        /// <returns></returns>
        public TokenContextInstance Parse(TokenContextDefinition contextDefinition, string contents)
        {
            if (_contextRegistry.IsEmpty == false)
            {
                var replacement = _contextRegistry.GetContext(contextDefinition.GetType());
                if (replacement != null && contextDefinition.GetType() != replacement.GetType())
                {
                    contextDefinition = replacement;
                }
            }

            var parseSession = new TokenParseSession(new TokenContextInstance(contextDefinition, contents), new TokenContextCollection(_contextRegistry));
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
