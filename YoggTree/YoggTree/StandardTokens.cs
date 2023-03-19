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

        public readonly static StringDoubleQuoteToken DoubleQuote = new StringDoubleQuoteToken();

        public readonly static StringSingleQuoteToken SingleQuote = new StringSingleQuoteToken();

        public readonly static StringGraveToken Grave = new StringGraveToken();

        public readonly static WhitespaceHorizontalToken WhitespaceHorizontal = new WhitespaceHorizontalToken();

        public readonly static WhitespaceVerticalToken WhitespaceVertical = new WhitespaceVerticalToken();

        public readonly static BackslashToken Backslash = new BackslashToken();

        public readonly static ForwardslashToken Forwardslash = new ForwardslashToken();

        public readonly static Parenthesis_OpenToken OpenParenthesis = new Parenthesis_OpenToken();

        public readonly static Parenthesis_CloseToken CloseParenthsis = new Parenthesis_CloseToken();

        public readonly static TextContentToken TextContent = new TextContentToken();

        public readonly static EmptyToken Empty = new EmptyToken();

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
        /// Gets the "(" and ")" characters.
        /// </summary>
        /// <returns></returns>
        public static List<TokenDefinition> GetParenthesisTokens()
        {
            return new List<TokenDefinition>()
            {
                OpenParenthesis,
                CloseParenthsis
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
            allTokens.AddRange(GetParenthesisTokens());

            return allTokens;
        }
    }
}
