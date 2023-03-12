/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTreeTest.Common
{
    public static class YoggTreeTestHelper
    {
        public static bool CompareParseResults<T>(TestParseArgs<T> parseArgs) where T : TokenContextDefinition, new()
        {
            var parser = new TokenParser();
            var result = parser.Parse(new T(), parseArgs.ContentToParse);

            return parseArgs.Expected.CompareToActual(result);
        }

        public static object[] ToObjArray(params object[] args)
        {
            return args;
        }
    }
}
