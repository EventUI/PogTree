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
        public int ContentLineNumber { get; } = -1;

        public int ContentColumnNumber { get; } = -1;

        public TokenSyntaxErrorExecption(string message, int contentLineNumber, int contentColumnNumber)
            : base(message)
        {
            ContentLineNumber = contentLineNumber;
            ContentColumnNumber = contentColumnNumber;
        }
    }
}
