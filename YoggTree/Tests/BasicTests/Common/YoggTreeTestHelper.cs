/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YoggTree.Core.Tokens;

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

        public static bool ValidateSeekResults<T>(TestParseArgs<T> parseArgs) where T : TokenContextDefinition, new()
        {
            var parser = new TokenParser();
            var result = parser.Parse(new T(), parseArgs.ContentToParse);

            WalkTokens(result);

            return true;
        }

        private static void WalkTokens(TokenContextInstance contextInstance)
        {
            if (contextInstance.Parent != null)
            {
                if (contextInstance.Tokens.Count == 0)
                {
                    if (contextInstance.StartToken.GetNextToken() != contextInstance.EndToken)
                    {
                        throw new Exception($"Token mismatch: StartToken's nect token for empty context {contextInstance} did not match EndToken. Expected: {contextInstance.EndToken} Actual: {contextInstance.StartToken.GetNextToken()}");
                    }

                    if (contextInstance.EndToken.GetPreviousToken() != contextInstance.StartToken)
                    {
                        throw new Exception($"Token mismatch: EndToken's previous token for empty context {contextInstance} did not match StartToken. Expected: {contextInstance.StartToken} Actual: {contextInstance.EndToken.GetPreviousToken()}");
                    }
                }
                if (contextInstance.Tokens.Count > 0 && contextInstance.Parent != null) //this check doesn't apply to the root context
                {
                    if (contextInstance.Tokens[0] != contextInstance.StartToken)
                    {
                        throw new Exception($"Token mismatch: StartToken for context {contextInstance} did not match at index 0. Expected: {contextInstance.StartToken} Actual: {contextInstance.Tokens[0]}");
                    }
                    else if (contextInstance.Tokens[contextInstance.Tokens.Count - 1] != contextInstance.EndToken)
                    {
                        throw new Exception($"Token mismatch: EndToken for context {contextInstance} did not match at index {contextInstance.Tokens.Count - 1}. Expected: {contextInstance.EndToken} Actual: {contextInstance.Tokens[contextInstance.Tokens.Count - 1]}");
                    }
                }
            }

            for (int x = 0; x < contextInstance.Tokens.Count; x++)
            {
                var curToken = contextInstance.Tokens[x];

                if (x == 0) //first token of new context
                {
                    if (contextInstance.Parent == null)
                    {
                        if (curToken.GetPreviousToken() != null) throw new Exception("Starting token in root context had a previous token.");
                    }
                    else
                    {
                        
                        int index = -1;
                        var placeholder = contextInstance.Parent.Tokens.FirstOrDefault(token => { index++; return token.GetChildContext() == contextInstance; });
                        if (placeholder == null) throw new Exception($"Context placeholder {placeholder} not found in parent context {contextInstance.Parent}");

                        if (index == 0 && contextInstance.Parent.Depth == 0)
                        {
                            if (placeholder.GetPreviousToken() != null) throw new Exception("Starting token in first child context had a previous token.");
                        }
                        else
                        {
                            if (placeholder.GetChildContext().StartToken != curToken) throw new Exception($"Token mismatch: Starting token for context {contextInstance} did not match placeholder's preceding token at index {x}. Expected: {contextInstance.StartToken} Actual: {placeholder.GetPreviousToken()}");
                        }
                    }
                }
                else if (x < contextInstance.Tokens.Count - 1) //token in middle of context
                {
                    if (curToken.GetPreviousToken() != contextInstance.Tokens[x -1]) throw new Exception($"Token mismatch: Token for context {contextInstance} did not match the context's preceding token at index {x - 1}. Expected: {contextInstance.Tokens[x - 1]} Actual: {curToken.GetPreviousToken()}");
                    if (curToken.GetNextToken() != contextInstance.Tokens[x + 1]) throw new Exception($"Token mismatch: Token for context {contextInstance} did not match the context's preceding token at index {x + 1}. Expected: {contextInstance.Tokens[x + 1]} Actual: {curToken.GetNextToken()}");
                }
                else
                {
                    if (contextInstance.Parent == null)
                    {
                        if (curToken.GetNextToken() != null) throw new Exception("Ending token in root context had a following token.");
                    }
                    else
                    {
                        if (curToken != contextInstance.EndToken) throw new Exception($"Token mismatch: Token for context {contextInstance} did not match the context's EndToken. Expected: {contextInstance.EndToken} Actual: {curToken}");
                    }
                }

                if (curToken.TokenInstanceType == TokenInstanceType.ContextPlaceholder)
                {
                    WalkTokens(curToken.GetChildContext());
                }
            }
        }

        public static object[] ToObjArray(params object[] args)
        {
            return args;
        }
    }
}
