/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core.Tokens;

namespace YoggTree.Core.DelegateSet
{
    internal class DelegateSetItem<T, TPredicate> where T : Delegate
    {
        public int Ordinal { get; init; } = -1;

        public T Delegate { get; init; } = null;

        public Func<TPredicate, bool> Predicate { get; init; }

        public Type TypeConstraint { get; init; } = null;
    }
}
