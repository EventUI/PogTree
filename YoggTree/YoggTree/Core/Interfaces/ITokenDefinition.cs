/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System.Text.RegularExpressions;

namespace YoggTree.Core.Interfaces
{
    public interface ITokenDefinition
    {
        Guid ID { get; }
        string Name { get; }
        Regex Token { get; }

        bool CanComeAfter(TokenInstance previousToken);
        bool CanComeBefore(TokenInstance nextToken);
        TokenParseContext CreateContext(TokenParseContext parent, TokenInstance start);
        (TokenInstance Instance, int Index) GetNextToken(int currentPosition, TokenParseContext context, int startingIndex = 0);
        bool IsValidInstance(TokenInstance instance);
    }
}