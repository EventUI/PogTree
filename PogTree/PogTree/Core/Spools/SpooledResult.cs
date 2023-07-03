/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace PogTree.Core.Spools
{
    /// <summary>
    /// Represents the index and length of a Regex find result in a string.
    /// </summary>
    internal class SpooledResult
    {
        /// <summary>
        /// The index in the content string at which the Regex match was found.
        /// </summary>
        public int StartIndex { get; set; } = -1;

        /// <summary>
        /// The length of the Regex match that was found.
        /// </summary>
        public int Length { get; set; } = -1;
    }

    /// <summary>
    /// Extension methods for SpooledResults.
    /// </summary>
    internal static class SpooledResultExtensions
    {
        /// <summary>
        /// Determines if the SpooledResult has any valid data.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns></returns>
        public static bool IsEmpty(this SpooledResult result)
        {
            if (result == null) return true;
            if (result.StartIndex < 0 && result.Length < 0) return true;

            return false;
        }

        /// <summary>
        /// Determines if the SpooledResult starts at or after the given index.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="index">The index to check against.</param>
        /// <returns></returns>
        public static bool IsAfter(this SpooledResult result, int index)
        {
            if (result == null) return false;
            if (result.StartIndex >= index) return true;

            return false;
        }

        /// <summary>
        /// Determines if the SpooledResult starts before the given index.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="index">The index to check against.</param>
        /// <returns></returns>
        public static bool IsBefore(this SpooledResult result, int index)
        {
            if (result == null) return false;
            if (result.StartIndex < index) return true;

            return false;
        }
    }
}
