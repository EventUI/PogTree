/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Spools
{
    internal class SpooledResult
    {
        public int StartIndex { get; set; } = -1;

        public int Length { get; set; } = -1;
    }

    internal static class SpooledResultExtensions
    {
        public static bool IsEmpty(this SpooledResult result)
        {
            if (result == null) return true;
            if (result.StartIndex < 0 && result.Length < 0) return true;

            return false;
        }

        public static bool IsAfter(this SpooledResult result, int index)
        {
            if (result == null) return false;
            if (result.StartIndex >= index) return true;

            return false;
        }

        public static bool IsBefore(this SpooledResult result, int index)
        {
            if (result == null) return false;
            if (result.StartIndex < index) return true;

            return false;
        }
    }
}
