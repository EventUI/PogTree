/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            var token = result.Tokens.FirstOrDefault()?.PeekNextToken();
            while (token != null)
            {
                tokenWalkCount++;
                var nextToken = token.PeekNextToken();
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

        public static bool CompareReaderResults(TestIterationArgs iterationArgs)
        {
            var parser = new TokenParser();
            var parsed = parser.Parse(new TestContext(), iterationArgs.ContentToParse);

            var reader = parsed.GetReader();
            foreach (var token in iterationArgs.ExpectedTokens)
            {
                TokenInstance nextToken = reader.GetNextToken(iterationArgs.Recursive);
                if (nextToken == null)
                {
                    throw new Exception($"Unexpected end of tokens at Position {reader.Position} at Depth {reader.Depth}");
                }

                if (nextToken.TokenDefinition.GetType() != token.TokenDefinition.GetType())
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {token.TokenDefinition} Actual: {nextToken.TokenDefinition}");
                }

                if (nextToken.TokenInstanceType != token.InstanceType)
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {token.InstanceType} Actual: {nextToken.TokenInstanceType}");
                }
            }

            reader.Seek(parsed);

            foreach (var context in iterationArgs.ExpectedContexts)
            {
                TokenContextInstance nextInstance = reader.GetNextContext(iterationArgs.Recursive);
                if (nextInstance == null)
                {
                    throw new Exception($"Unexpected end of contexts at Position {reader.Position} at Depth {reader.Depth}");
                }

                if (nextInstance.ContextDefinition.GetType() != context.ContextDefinition.GetType())
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {context.ContextDefinition} Actual: {nextInstance.ContextDefinition}");
                }

                if (nextInstance.Depth != context.Depth)
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {context.Depth} Actual: {reader.Depth}");
                }
            }

            return true;
        }

        public static bool CompareReaderResultsReverse(TestIterationArgs iterationArgs)
        {
            var parser = new TokenParser();
            var parsed = parser.Parse(new TestContext(), iterationArgs.ContentToParse);

            iterationArgs.ExpectedContexts.Reverse();
            iterationArgs.ExpectedTokens.Reverse();

            var reader = parsed.GetReader();
            reader.Seek(ReaderSeekLocation.LastToken, iterationArgs.Recursive);
            bool first = true;
            foreach (var token in iterationArgs.ExpectedTokens)
            {
                TokenInstance nextToken = null;

                if (first == true) //the first token when going in reverse is the very last token, calling get previous token will get the one before it if you are at the end.
                {
                    nextToken = reader.CurrentToken;
                    first = false;
                }
                else
                {
                    nextToken = reader.GetPreviousToken(iterationArgs.Recursive);
                }

                if (nextToken == null)
                {
                    throw new Exception($"Unexpected end of tokens at Position {reader.Position} at Depth {reader.Depth}");
                }

                if (nextToken.TokenDefinition.GetType() != token.TokenDefinition.GetType())
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {token.TokenDefinition} Actual: {nextToken.TokenDefinition}");
                }

                if (nextToken.TokenInstanceType != token.InstanceType)
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {token.InstanceType} Actual: {nextToken.TokenInstanceType}");
                }
            }

            reader.Seek(ReaderSeekLocation.FirstToken, true);
            reader.Seek(ReaderSeekLocation.LastContext, iterationArgs.Recursive);

            first = true;
            foreach (var context in iterationArgs.ExpectedContexts)
            {
                TokenContextInstance nextInstance = null;
                if (first == true) //the first item in the reversed array of contexts will never match due to the asymmetric way recursion works in reverse for contexts.
                {
                    first = false;
                    continue;
                }

                nextInstance = reader.GetPreviousContext(iterationArgs.Recursive);                

                if (nextInstance == null)
                {
                    throw new Exception($"Unexpected end of contexts at Position {reader.Position} at Depth {reader.Depth}");
                }

                if (nextInstance.ContextDefinition.GetType() != context.ContextDefinition.GetType())
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {context.ContextDefinition} Actual: {nextInstance.ContextDefinition}");
                }

                if (nextInstance.Depth != context.Depth)
                {
                    throw new Exception($"Token mismatch at Position {reader.Position} at Depth {reader.Depth}. Expected: {context.Depth} Actual: {reader.Depth}");
                }
            }

            return true;
        }

        private static void WalkTokens(TokenContextInstance contextInstance)
        {
            if (contextInstance.Parent != null)
            {
                if (contextInstance.Tokens.Count == 0)
                {
                    if (contextInstance.StartToken.PeekNextToken() != contextInstance.EndToken)
                    {
                        throw new Exception($"Token mismatch: StartToken's nect token for empty context {contextInstance} did not match EndToken. Expected: {contextInstance.EndToken} Actual: {contextInstance.StartToken.PeekNextToken()}");
                    }

                    if (contextInstance.EndToken.PeekPreviousToken() != contextInstance.StartToken)
                    {
                        throw new Exception($"Token mismatch: EndToken's previous token for empty context {contextInstance} did not match StartToken. Expected: {contextInstance.StartToken} Actual: {contextInstance.EndToken.PeekPreviousToken()}");
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
                        if (curToken.PeekPreviousToken() != null) throw new Exception("Starting token in root context had a previous token.");
                    }
                    else
                    {
                        
                        int index = -1;
                        var placeholder = contextInstance.Parent.Tokens.FirstOrDefault(token => { index++; return token.GetChildContext() == contextInstance; });
                        if (placeholder == null) throw new Exception($"Context placeholder {placeholder} not found in parent context {contextInstance.Parent}");

                        if (index == 0 && contextInstance.Parent.Depth == 0)
                        {
                            if (placeholder.PeekPreviousToken() != null) throw new Exception("Starting token in first child context had a previous token.");
                        }
                        else
                        {
                            if (placeholder.GetChildContext().StartToken != curToken) throw new Exception($"Token mismatch: Starting token for context {contextInstance} did not match placeholder's preceding token at index {x}. Expected: {contextInstance.StartToken} Actual: {placeholder.PeekPreviousToken()}");
                        }
                    }
                }
                else if (x < contextInstance.Tokens.Count - 1) //token in middle of context
                {
                    if (curToken.PeekPreviousToken() != contextInstance.Tokens[x -1]) throw new Exception($"Token mismatch: Token for context {contextInstance} did not match the context's preceding token at index {x - 1}. Expected: {contextInstance.Tokens[x - 1]} Actual: {curToken.PeekPreviousToken()}");
                    if (curToken.PeekNextToken() != contextInstance.Tokens[x + 1]) throw new Exception($"Token mismatch: Token for context {contextInstance} did not match the context's preceding token at index {x + 1}. Expected: {contextInstance.Tokens[x + 1]} Actual: {curToken.PeekNextToken()}");
                }
                else
                {
                    if (contextInstance.Parent == null)
                    {
                        if (curToken.PeekNextToken() != null) throw new Exception("Ending token in root context had a following token.");
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
