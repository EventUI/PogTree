/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogTree.Core.Tokens
{
    /// <summary>
    /// Token for finding "&lt;" characters (intended for XML-like markup languages).
    /// </summary>
    public class TagStartToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new TagStartToken.
        /// </summary>
        public TagStartToken()
            :base(TokenRegexStore.LessThan, "<", TokenDefinitionFlags.ContextStarter, "MarkupTag")
        {

        }
    }

    /// <summary>
    /// Token for finding a "&gt;" characters (intended for XML-like markup languages)
    /// </summary>
    public class TagEndToken : TokenDefinition
    {
        /// <summary>
        /// Creates a new TagEndToken.
        /// </summary>
        public TagEndToken()
            : base(TokenRegexStore.GreaterThan, ">", TokenDefinitionFlags.ContextEnder, "MarkupTag")
        {

        }
    }
}
