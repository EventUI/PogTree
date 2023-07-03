/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree.Core.Tokens
{
    /// <summary>
    /// List of pre-compiled Regex's used by the standard tokens.
    /// </summary>
    public static class TokenRegexStore
    {
        /// <summary>
        /// Regex for getting horizontal whitespace.
        /// </summary>
        public static Regex Whitespace_Horizontal { get; } = new Regex("[^\\S\\n\\r]+", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting vertical whitespace.
        /// </summary>
        public static Regex Whitespace_Vertical { get; } = new Regex("\\n\\r|\\r\\n|\\r|\\n", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a double-quote character.
        /// </summary>
        public static Regex DoubleQuote { get; } = new Regex("\"", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a single-quote character.
        /// </summary>
        public static Regex SingleQuote { get; } = new Regex("'", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a grave character.
        /// </summary>
        public static Regex Grave { get; } = new Regex("`", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting an open curly brace.
        /// </summary>
        public static Regex Brace_OpenCurly { get; } = new Regex("\\{", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a close curly brace.
        /// </summary>
        public static Regex Brace_CloseCurly { get; } = new Regex("\\}", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting an open bracket.
        /// </summary>
        public static Regex Brace_OpenBracket { get; } = new Regex("\\[", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a close bracket.
        /// </summary>
        public static Regex Brace_CloseBracket { get; } = new Regex("\\]", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a backslash.
        /// </summary>
        public static Regex Backslash { get; } = new Regex("\\\\", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a forward slash.
        /// </summary>
        public static Regex Forwardslash { get; } = new Regex("\\/", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a open parenthesis.
        /// </summary>
        public static Regex Parenthesis_Open { get; } = new Regex("\\(", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a closed parenthesis.
        /// </summary>
        public static Regex Parenthesis_Close { get; } = new Regex("\\)", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for getting a &lt; character.
        /// </summary>
        public static Regex LessThan { get; } = new Regex("<", RegexOptions.NonBacktracking | RegexOptions.Compiled);

        /// <summary>
        /// Regex for a getting &gt; character.
        /// </summary>
        public static Regex GreaterThan { get; } = new Regex(">", RegexOptions.NonBacktracking | RegexOptions.Compiled);
    }
}
