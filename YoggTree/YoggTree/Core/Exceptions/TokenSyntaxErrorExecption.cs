/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a token identifies that is has come before or after a token that is determined to be invalid.
    /// </summary>
    public class TokenSyntaxErrorExecption : Exception
    {
        /// <summary>
        /// The line number where the exception occurred. The line number is relative to the entire content string being parsed.
        /// </summary>
        public int ContentLineNumber { get; } = -1;

        /// <summary>
        /// The column number on the line where the exception occurred.
        /// </summary>
        public int ContentColumnNumber { get; } = -1;

        public TokenSyntaxErrorExecption(string message, int contentLineNumber, int contentColumnNumber)
            : base(message)
        {
            ContentLineNumber = contentLineNumber;
            ContentColumnNumber = contentColumnNumber;
        }
    }
}
