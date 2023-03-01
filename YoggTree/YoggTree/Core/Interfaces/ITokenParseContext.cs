/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree.Core.Interfaces
{
    public interface ITokenParseContext
    {
        int AbsoluteOffset { get; }
        IReadOnlyList<TokenParseContext> ChildContexts { get; }
        ReadOnlyMemory<char> Contents { get; }
        TokenInstance CurrentToken { get; }
        int Depth { get; }
        int EndIndex { get; }
        TokenInstance EndToken { get; }
        Guid ID { get; }
        string Name { get; }
        TokenParseContext Parent { get; }
        TokenParseSession ParseSession { get; }
        int StartIndex { get; }
        TokenInstance StartToken { get; }
        IReadOnlyList<TokenInstance> Tokens { get; }

        int GetAbsoluteIndex();
        ReadOnlyMemory<char> GetContents(IContentSpan start, IContentSpan end = null);
        ReadOnlyMemory<char> GetContents(IContentSpan start, int? end = null);
        ReadOnlyMemory<char> GetContents(int start, int? end = null);
        int GetContextualIndex(TokenParseContext targetContext);
        string ToString();
    }
}