/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoggTree.Tokens
{
    public static partial class TokenRegexStore
    {
        public static Regex Whitespace_Horizontal { get; } = new Regex("[^\\S\\n\\r]+", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Whitespace_Vertical { get; } = new Regex("\\n\\r|\\r\\n|\\r|\\n", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex DoubleQuote { get; } = new Regex("\\\"", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex SingleQuote { get; } = new Regex("'", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Grave { get; } = new Regex("`", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_OpenCurly { get; } = new Regex("\\{", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_CloseCurly { get; } = new Regex("\\}", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_OpenBracket { get; } = new Regex("\\[", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_CloseBracket { get; } = new Regex("\\]", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Backslash { get; } = new Regex("\\\\", RegexOptions.NonBacktracking | RegexOptions.Compiled);
    }
}
