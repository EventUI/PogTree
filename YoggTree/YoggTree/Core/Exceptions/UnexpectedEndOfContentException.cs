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
    /// Exception that is thrown when the end of content is reached outside of a context which does not have the Unbounded flag set and is not the root context.
    /// </summary>
    public class UnexpectedEndOfContentException : Exception
    {
        /// <summary>
        /// Creates a new UnexpectedEndOfContentException.
        /// </summary>
        /// <param name="message">The message to give the exception.</param>
        public UnexpectedEndOfContentException(string message) 
            : base(message)
        {
        }
    }
}
