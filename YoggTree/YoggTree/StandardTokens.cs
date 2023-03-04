/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Tokens;

namespace YoggTree
{
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

        public static List<TokenDefinition> GetWhitespaceTokens()
        {
            return new List<TokenDefinition>()
            {
                WhitespaceHorizontal,
                WhitespaceVertical
            };
        }

        public static List<TokenDefinition> GetStringTokens()
        {
            return new List<TokenDefinition>()
            {
                DoubleQuote,
                SingleQuote,
                Grave
            };
        }

        public static List<TokenDefinition> GetBracketTokens()
        {
            return new List<TokenDefinition>()
            {
                OpenBracket,
                CloseBracket
            };
        }

        public static List<TokenDefinition> GetCurlyBraceTokens()
        {
            return new List<TokenDefinition>()
            {
                OpenCurlyBrace,
                CloseCurlyBrace
            };
        }

        public static List<TokenDefinition> GetAllStandardTokens()
        {
            var allTokens = new List<TokenDefinition>();
            allTokens.AddRange(GetWhitespaceTokens());
            allTokens.AddRange(GetStringTokens());
            allTokens.AddRange(GetBracketTokens());
            allTokens.AddRange(GetCurlyBraceTokens());

            return allTokens;
        }
    }
}
