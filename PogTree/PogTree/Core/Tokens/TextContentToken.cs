﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree.Core.Tokens
{
    /// <summary>
    /// Placeholder token for spans of text that is between two sequential tokens. WARNING: Never include in the ValidTokens list as this will match the entire string - this is used by the TokenContextInstance to make spans of plain text on the fly.
    /// </summary>
    public sealed class TextContentToken : TokenDefinition
    {
        /// <summary>
        /// A singleton instance of the TextContentToken.
        /// </summary>
        public static TextContentToken Instance { get; } = new TextContentToken();

        /// <summary>
        /// Creates a new TextContextToken.
        /// </summary>
        public TextContentToken()
            : base(new Regex(".+"), "<text content>")
        {

        }

        /// <summary>
        /// Gets a string that identifies this token as StringContentToken.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}-{Name})";
        }
    }
}
