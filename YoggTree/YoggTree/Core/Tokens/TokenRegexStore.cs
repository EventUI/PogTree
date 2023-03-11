/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Text.RegularExpressions;

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// List of pre-compiled Regex's used by the standard tokens.
    /// </summary>
    public static class TokenRegexStore
    {
        public static Regex Whitespace_Horizontal { get; } = new Regex("[^\\S\\n\\r]+", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Whitespace_Vertical { get; } = new Regex("\\n\\r|\\r\\n|\\r|\\n", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex DoubleQuote { get; } = new Regex("\"", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex SingleQuote { get; } = new Regex("'", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Grave { get; } = new Regex("`", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_OpenCurly { get; } = new Regex("\\{", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_CloseCurly { get; } = new Regex("\\}", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_OpenBracket { get; } = new Regex("\\[", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Brace_CloseBracket { get; } = new Regex("\\]", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Backslash { get; } = new Regex("\\\\", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        public static Regex Forwardslash { get; } = new Regex("\\/", RegexOptions.NonBacktracking | RegexOptions.Compiled);
    }
}
