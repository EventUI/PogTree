/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

namespace YoggTree
{
    public interface ITokenDefinition
    {
        string ContextKey { get; }
        TokenDefinitionFlags Flags { get; }
        Guid ID { get; }
        string Name { get; }
        int SpoolSize { get; }
        Regex Token { get; }

        bool CanComeAfter(TokenInstance previousToken, TokenInstance validatingToken);
        bool CanComeBefore(TokenInstance nextToken, TokenInstance validatingToken);
        TokenContextDefinition GetNewContextDefinition(TokenInstance start);
        bool IsValidInstance(TokenInstance instance);
        string ToString();
    }
}