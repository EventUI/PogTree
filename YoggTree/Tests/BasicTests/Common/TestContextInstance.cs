/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoggTreeTest.Common
{
    public record TestContextInstance
    {
        public TokenContextDefinition ContextDefinition { get; }

        public string Contents { get; init; }

        public int Depth { get; init; }

        public List<TestTokenInstance> Tokens { get; init; } = new List<TestTokenInstance>();

        public List<TestContextInstance> ChildContexts { get; init; } = new List<TestContextInstance>();

        public TestContextInstance(TokenContextDefinition contextDefinition)
        {
            if (contextDefinition == null) throw new ArgumentNullException(nameof(contextDefinition));
            ContextDefinition = contextDefinition;
        }

        public bool CompareToActual(TokenContextInstance instance)
        {
            if (ContextDefinition.ID != instance.ContextDefinition.ID)
            {
                if (ContextDefinition.GetType() != instance.ContextDefinition.GetType()) throw new Exception($"Context definition mismatch. \nExpected: {ContextDefinition} \nActual: {instance.ContextDefinition}");
            }

            if (instance.Contents.ToString() != Contents) throw new Exception($"{ContextDefinition} mismatched expected contents. \nExpected: \"{Contents}\" Actual: \n\"{instance.Contents}\"");
            if (Depth != instance.Depth) throw new Exception($"{ContextDefinition} mismatched expected depth. Expected: \"{Depth}\" Actual: \"{instance.Depth}\"");
            if (Tokens.Count != instance.Tokens.Count) throw new Exception($"{ContextDefinition} mismatched expected token count. Expected: \"{Tokens.Count}\" Actual: \"{instance.Tokens.Count}\"");

            TokenInstance previousToken = null;
            for (int x = 0; x < Tokens.Count; x++)
            {
                var expectedToken = Tokens[x];
                var resultToken = instance.Tokens[x];

                expectedToken.CompareToActual(resultToken);

                if (previousToken != null)
                {
                    var childContext = previousToken.GetChildContext();
                    if (childContext != null)
                    {
                        if (previousToken.StartIndex + childContext.Contents.Length != resultToken.StartIndex) throw new Exception($"Token index sequence error. Current ({resultToken}) and previous ({previousToken}) overlapped after previous token started new child context.");
                    }
                    else
                    {
                        if (previousToken.StartIndex + previousToken.Contents.Length != resultToken.StartIndex) throw new Exception($"Token index sequence error. Current ({resultToken}) and previous ({previousToken}) overlapped.");
                    }
                }

                previousToken = resultToken;
            }

            if (ChildContexts.Count != instance.ChildContexts.Count) throw new Exception($"{ContextDefinition} mismatched child context count. Expected: \"{ChildContexts.Count}\" Actual: \"{instance.ChildContexts.Count}\"");

            for (int x = 0; x < ChildContexts.Count; x++)
            {
                var expectedChild = ChildContexts[x];
                var actualChild = instance.ChildContexts[x];

                expectedChild.CompareToActual(actualChild);
            }

            return true;
        }
    }

    public record TestTokenInstance
    {
        public TokenInstanceType InstanceType { get; } = TokenInstanceType.None;

        public TokenDefinition TokenDefinition { get; }

        public string Contents { get; init; }

        public TestContextInstance TestContextInstance { get; init; }

        public TestTokenInstance(TokenDefinition tokenDefinition)
        {
            if (tokenDefinition == null) throw new ArgumentNullException(nameof(tokenDefinition));

            TokenDefinition = tokenDefinition;
            InstanceType = TokenInstanceType.RegexResult;
        }

        public TestTokenInstance(TokenDefinition tokenDefinition, string contents, TokenInstanceType instanceType)
        {
            if (tokenDefinition == null) throw new ArgumentNullException(nameof(tokenDefinition));

            TokenDefinition = tokenDefinition;
            Contents = contents;
            InstanceType = instanceType;
        }

        public bool CompareToActual(TokenInstance instance)
        {
            if (Contents != instance.Contents.ToString()) throw new Exception($"{TokenDefinition} mismatched expected contents. \nExpected: \"{Contents}\" \nActual: \"{instance.Contents}\"");
            if (InstanceType != instance.TokenInstanceType) throw new Exception(($"{TokenDefinition} mismatched expected InstanceType. Expected: \"{InstanceType}\" Actual: \"{instance.TokenInstanceType}\""));
            if (instance.TokenDefinition.ID != TokenDefinition.ID)
            {
                if (instance.TokenDefinition.GetType() != TokenDefinition.GetType()) throw new Exception($"Token definition mismatch. \nExpected: {TokenDefinition} \nActual: {instance.TokenDefinition}");
            }            

            if (TestContextInstance != null)
            {
                if (TestContextInstance.Contents.ToString() != instance.GetChildContext()?.Contents.ToString()) throw new Exception($"Contents of TestContextInstance did not match: Expected: \"{TestContextInstance.Contents}\" \n Actual: {instance.GetChildContext().Contents}");
            }

            return true;
        }
    }

    public record TestParseArgs<T> where T : TokenContextDefinition, new()
    {
        public string TestName { get; init; }

        public TestContextInstance Expected { get; init; }

        public string ContentToParse { get; init; }
    }

    public record TestIterationArgs
    {
        public string TestName { get; set; }

        public string ContentToParse { get; set; }

        public bool Recursive { get; set; } = false;

        public bool Reverse { get; set; } = false; 

        public List<TestTokenInstance> ExpectedTokens { get; set; } = new List<TestTokenInstance>();

        public List<TestContextInstance> ExpectedContexts { get; set; } = new List<TestContextInstance>();
    }
}
