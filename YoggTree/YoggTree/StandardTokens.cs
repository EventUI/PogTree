/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using YoggTree.Core.Tokens;

namespace YoggTree
{
    /// <summary>
    /// Standard tokens that occur in most files that will be parsed.
    /// </summary>
    public static class StandardTokens
    {
        public readonly static OpenBracketToken OpenBracket = new OpenBracketToken();

        public readonly static CloseBracketToken CloseBracket = new CloseBracketToken();

        public readonly static OpenCurlyBraceToken OpenCurlyBrace = new OpenCurlyBraceToken();

        public readonly static CloseCurlyBraceToken CloseCurlyBrace = new CloseCurlyBraceToken();

        public readonly static StringDoubleQuote DoubleQuote = new StringDoubleQuote();

        public readonly static StringSingleQuote SingleQuote = new StringSingleQuote();

        public readonly static StringGrave Grave = new StringGrave();

        public readonly static WhitespaceHorizontal WhitespaceHorizontal = new WhitespaceHorizontal();

        public readonly static WhitespaceVertical WhitespaceVertical = new WhitespaceVertical();

        public readonly static Backslash Backslash = new Backslash();

        public readonly static Forwardslash Forwardslash = new Forwardslash();

        /// <summary>
        /// Gets the horizontal and vertical whitespace tokens.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetWhitespaceTokens()
        {
            return new List<TokenDefinition>()
            {
                WhitespaceHorizontal,
                WhitespaceVertical
            };
        }

        /// <summary>
        /// Gets the grave, single, and double quote tokens.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetStringTokens()
        {
            return new List<TokenDefinition>()
            {
                DoubleQuote,
                SingleQuote,
                Grave
            };
        }

        /// <summary>
        /// Gets the "[" and "]" tokens.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetBracketTokens()
        {
            return new List<TokenDefinition>()
            {
                OpenBracket,
                CloseBracket
            };
        }

        /// <summary>
        /// Gets the "{" and "}" tokens.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetCurlyBraceTokens()
        {
            return new List<TokenDefinition>()
            {
                OpenCurlyBrace,
                CloseCurlyBrace
            };
        }

        /// <summary>
        /// Gets the forward and backward slash characters.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetSlashTokens()
        {
            return new List<TokenDefinition>()
            {
                Backslash,
                Forwardslash
            };
        }

        /// <summary>
        /// Gets all the tokens in this object.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetAllStandardTokens()
        {
            var allTokens = new List<TokenDefinition>();
            allTokens.AddRange(GetWhitespaceTokens());
            allTokens.AddRange(GetStringTokens());
            allTokens.AddRange(GetBracketTokens());
            allTokens.AddRange(GetCurlyBraceTokens());
            allTokens.AddRange(GetSlashTokens());
            return allTokens;
        }
    }
}
