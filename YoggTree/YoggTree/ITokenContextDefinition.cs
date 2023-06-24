/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/




namespace YoggTree
{
    public interface ITokenContextDefinition
    {
        ContextDefinitionFlags Flags { get; }
        Guid ID { get; }
        string Name { get; }
        IReadOnlyCollection<TokenDefinition> ValidTokens { get; }

        void AddToken(Type tokenType);
        void AddToken<TToken>() where TToken : TokenDefinition, new();
        void AddToken<TToken>(Func<TToken> factory) where TToken : TokenDefinition;
        void AddTokens(IEnumerable<Type> tokenTypes);
        bool EndsCurrentContext(TokenInstance tokenInstance);
        bool HasToken(TokenInstance token);
        bool HasToken(Type tokenDefinitionType);
        bool HasToken<TToken>() where TToken : TokenDefinition;
        void RemoveToken<TToken>() where TToken : TokenDefinition;
        bool StartsNewContext(TokenInstance tokenInstance);
    }
}