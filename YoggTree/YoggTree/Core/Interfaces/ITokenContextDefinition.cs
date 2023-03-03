/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/


namespace YoggTree.Core.Interfaces
{
    public interface ITokenContextDefinition
    {
        Guid ID { get; }
        string Name { get; }
        IReadOnlyList<TokenDefinition> ValidTokens { get; }

        TokenContextInstance CreateNewContext(TokenInstance startToken);
        bool EndsCurrentContext(TokenInstance tokenInstance);
        bool IsValidInContext(TokenInstance token);
        bool StartsNewContext(TokenInstance tokenInstance);
    }
}