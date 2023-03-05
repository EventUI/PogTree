/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YoggTree.Core.Spools
{
    internal class TokenSpool
    {
        public TokenDefinition Token { get; }

        public SpooledResult[] ResultSpool { get; } = null;

        public int CurrentIndex { get; set; } = 0;

        public int SpoolSize { get; } = 0;

        public bool EndOfContent { get; set; } = false;

        public TokenSpool(TokenDefinition token, int spoolSize)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (spoolSize < 0) throw new ArgumentOutOfRangeException(nameof(spoolSize));

            Token = token;
            SpoolSize = spoolSize;
            ResultSpool = new SpooledResult[spoolSize];
        }
    }

    internal static class TokenSpoolExtensions
    {
        internal static SpooledResult GetNextResult(this TokenSpool spool, int startIndex, ReadOnlyMemory<char> content)
        {
            if (spool.EndOfContent == true) return spool.ResultSpool[0];
            SpooledResult lastResult = (spool.CurrentIndex == 0) ? null : spool.ResultSpool[spool.CurrentIndex - 1];
            for (int x = spool.CurrentIndex; x < spool.SpoolSize; x++)
            {
                var curSpool = spool.ResultSpool[x];
                if (curSpool == null) break;

                lastResult = curSpool;
                if (curSpool.IsEmpty() == false && curSpool.IsAfter(startIndex) == true)
                {
                    return curSpool;
                }
            }

            startIndex = lastResult == null ? 0 : lastResult.StartIndex + lastResult.Length;

            spool.FillSpool(startIndex, content);
            spool.CurrentIndex = 0;

            return spool.GetNextResult(startIndex, content);
        }

        internal static void FillSpool(this TokenSpool spool, int startIndex, ReadOnlyMemory<char> content)
        {
            int y = 0;
            foreach (var result in spool.Token.Token.EnumerateMatches(content.Span, startIndex))
            {
                if (y == spool.SpoolSize) break;

                var curResult = spool.ResultSpool[y];
                if (curResult == null)
                {
                    curResult = new SpooledResult();
                    spool.ResultSpool[y] = curResult;
                }

                spool.ResultSpool[y].Length = result.Length;
                spool.ResultSpool[y].StartIndex = result.Index;

                y++;
            }

            if (y == 0)
            {
                spool.EndOfContent = true;
            }

            if (y < spool.SpoolSize)
            {
                for (int x = y; x < spool.SpoolSize; x++)
                {
                    var curResult = spool.ResultSpool[x];
                    if (curResult == null) continue;

                    spool.ResultSpool[x].Length = -1;
                    spool.ResultSpool[x].StartIndex = -1;
                }
            }
        }
    }
}
