/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Interfaces
{
    public interface IContentSpan
    {
        public int StartIndex { get; }
        
        public int EndIndex { get; }

        public ReadOnlyMemory<char> Contents { get; }

        public TokenContextInstance GetContext();

        int GetContextualIndex(TokenContextInstance targetContext);

        public int GetAbsoluteIndex();
    }
}
