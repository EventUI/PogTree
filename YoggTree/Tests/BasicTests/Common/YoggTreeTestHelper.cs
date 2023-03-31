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

        public static bool CompareSeekResults<T>(TestParseArgs<T> parseArgs) where T : TokenContextDefinition, new()
        {
            var parser = new TokenParser();
            var result = parser.Parse(new T(), parseArgs.ContentToParse);

            int tokenWalkCount = 0;
            var token = result.Tokens.FirstOrDefault()?.GetNextToken();
            while (token != null)
            {
                tokenWalkCount++;
                var nextToken = token.GetNextToken();
                if (nextToken == null)
                {
                    var possibleEndToken = token?.GetAbsoluteInstance();
                    if (possibleEndToken?.EndIndex != parseArgs.ContentToParse.Length)
                    {
                        throw new Exception("Token seek failed to reach the end of the graph.");
                    }
                }

                token = nextToken;
            }

            return true;
        }

        public static object[] ToObjArray(params object[] args)
        {
            return args;
        }
    }
}
