using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoggTree.Core.Tokens
{
    /// <summary>
    /// Represents a token that is an empty placeholder for a TokenDefinition in certain TokenInstances.
    /// </summary>
    public class EmptyToken : TokenDefinition
    {
        public EmptyToken()
            :base(new Regex(""), "<empty>")
        {

        }
    }
}
