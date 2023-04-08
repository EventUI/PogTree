/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Spools
{
    /// <summary>
    /// Class for containing the results of Regex.ValueMatchEnumerators in an array of SpooledResult instances that are created, populated, and reused lazily. 
    /// </summary>
    internal class TokenSpool
    {
        /// <summary>
        /// The TokenDefinition containing the Regex to use to find matches in a Span of chars.
        /// </summary>
        public TokenDefinition Token { get; }

        /// <summary>
        /// An array of SpooledResults that are re-used over and over rather than being created and destroyed as matches are used and found.
        /// </summary>
        public SpooledResult[] ResultSpool { get; } = null;

        /// <summary>
        /// The current index of the spool in the ResultSpool array. This is incremented every time a value is taken from the spool and is determined to be the next result in the content string relative to all the other tokens in a TokenContextInstance.
        /// </summary>
        public int CurrentSpoolIndex { get; set; } = 0;

        /// <summary>
        /// The index the TokenSpool was at the last time it pulled out a token.
        /// </summary>
        public int CurrentContentIndex { get; set; } = 0;

        /// <summary>
        /// The length of the ResultSpool.
        /// </summary>
        public int SpoolSize { get; } = 0;

        /// <summary>
        /// Whether or not this spool reached the end of the last content it was asked to search.
        /// </summary>
        public bool EndOfContent { get; set; } = false;

        /// <summary>
        /// Creates a new TokenSpool.
        /// </summary>
        /// <param name="token">The definition of the token to find matches for.</param>
        /// <param name="spoolSize">The number of elements to keep in the ResultSpool array.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TokenSpool(TokenDefinition token, int spoolSize)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (spoolSize < 0) throw new ArgumentOutOfRangeException(nameof(spoolSize));

            Token = token;
            SpoolSize = spoolSize;
            ResultSpool = new SpooledResult[spoolSize];
        }
    }

    /// <summary>
    /// Extension methods for TokenSpools.
    /// </summary>
    internal static class TokenSpoolExtensions
    {
        /// <summary>
        /// Gets the next result in the content span or returns an empty SpooledResult if the end of the content has been reached.
        /// </summary>
        /// <param name="spool">The token spool to get the next result from.</param>
        /// <param name="startIndex">The starting index in the content to search for matches from.</param>
        /// <param name="content">The content to search for matches in using the TokenSpool's TokenDefinition's Regex.</param>
        /// <returns></returns>
        internal static (SpooledResult Spool, int CurrentSpoolIndex) GetNextResult(this TokenSpool spool, int startIndex, ReadOnlyMemory<char> content)
        {
            SpooledResult lastResult = null;
            int lastIndex = 0;

            if (startIndex >= spool.CurrentContentIndex) //if we're looking forwards down the spool, start looking at all the next results and find the one before the one that is after the start index.
            {
                if (spool.EndOfContent == true) return (spool.ResultSpool[spool.CurrentSpoolIndex], -1); //the last spooling operation ran out of content - no more work to do. (This assumes we're getting the same content over and over and not different spans of chars.)

                lastResult = (spool.CurrentSpoolIndex == 0) ? null : spool.ResultSpool[spool.CurrentSpoolIndex - 1]; //get the previously used result and use it as the starting point to begin spooling again should CurrentSpoolIndex == SpoolSize.
                for (int x = spool.CurrentSpoolIndex; x < spool.SpoolSize; x++)
                {
                    var curSpool = spool.ResultSpool[x];
                    if (curSpool == null) break;

                    lastIndex = curSpool.StartIndex + curSpool.Length;

                    lastResult = curSpool;
                    if (curSpool.IsEmpty() == false && curSpool.IsAfter(startIndex) == true)
                    {
                        return (curSpool, x);
                    }
                }
            }
            else //if we're looking backwards back up the spool, start at the beginning of the spool to see if any of the spooled results straddle the index we're looking for
            {
                int x = 0;
                SpooledResult previousResult = null;
                while (x < spool.SpoolSize)
                {
                    var curSpool = spool.ResultSpool[x];
                    if (curSpool == null || curSpool.IsEmpty()) break;

                    if (curSpool.IsBefore(startIndex) == true)
                    {
                        previousResult = curSpool;
                    }
                    else if (previousResult.IsEmpty() == false && curSpool.IsAfter(startIndex) == true) //we need to have a result both before AND after the target index - if we have nothing before it, we could be too far down the content to find what we're looking for. If nothing comes after it, we need to see forwards.
                    {
                        lastResult = curSpool;
                        break;
                    }

                    x++;
                }

                if (lastResult == null) //not in range, spool starting at the start index and look forwards from there.
                {
                    spool.FillSpool(startIndex, content);
                    spool.CurrentSpoolIndex = 0;
                    spool.EndOfContent = false;
                    spool.CurrentContentIndex = startIndex;

                    return spool.GetNextResult(startIndex, content);
                }
                else
                {
                    return (lastResult, x);
                }                
            }

            if (lastIndex < startIndex && lastResult.IsEmpty()) //we didn't find anything after the starting index, we're done
            {
                spool.EndOfContent = true;
                return (null, -1);
            }

            startIndex = lastResult.IsEmpty() == true ? 0 : lastResult.StartIndex + lastResult.Length; //we restart the spool AFTER the last usable result so we don't re-spool old results. This should only resolve to 0 on the first attempt to use the spool.

            spool.FillSpool(startIndex, content); //refill the spool from the new index
            spool.CurrentSpoolIndex = 0;
            spool.CurrentContentIndex = startIndex;

            //keep recursively going in this loop until we find a result or run out of results.
            //NOTE: Possible problem and a fix for later - this COULD cause a stack overflow should the number of calls (numRegexResults/SpoolSize) exceed the amount that can fit on the stack. An iterative solution is needed instead if that happens.
            return spool.GetNextResult(startIndex, content);
        }

        /// <summary>
        /// Fills the spool with new data or clears out old data.
        /// </summary>
        /// <param name="spool">The spool to fill with data.</param>
        /// <param name="startIndex">The starting index in the content to search for matches from.</param>
        /// <param name="content">The content to pull matches from.</param>
        internal static void FillSpool(this TokenSpool spool, int startIndex, ReadOnlyMemory<char> content)
        {
            int y = 0;
            foreach (var result in spool.Token.Token.EnumerateMatches(content.Span, startIndex))
            {
                if (y == spool.SpoolSize) break; //if we have found a spool's worth of results, stop getting more results as the spool is now full.

                //lazily make SpooledResults to fill the spool as more results are found than are presently in the spool (up to it's maximum)
                var curResult = spool.ResultSpool[y];
                if (curResult == null)
                {
                    curResult = new SpooledResult();
                    spool.ResultSpool[y] = curResult;
                }

                //assign the values from the ValueMatch to the element at the current index in the spool. We do this because the ValueMatches returned by the regex iterator are ref structs and can't be put into arrays or lists.
                curResult.Length = result.Length;
                curResult.StartIndex = result.Index;

                y++;
            }

            //if we found no more matches, mark the end of content flag so we know not to search in this spool again for the given set of content.
            if (y == 0)
            {
                spool.EndOfContent = true;
            }

            //if we didn't fill the entire spool, set all the SpooledResults to their default values that came after the last match that we found - this makes the SpooledResult.IsEmpty() extension return true so we know the result isn't a valid result.
            if (y < spool.SpoolSize)
            {
                for (int x = y; x < spool.SpoolSize; x++)
                {
                    var curResult = spool.ResultSpool[x];
                    if (curResult == null) continue;

                    curResult.Length = -1;
                    curResult.StartIndex = -1;
                }
            }
        }
    }
}
