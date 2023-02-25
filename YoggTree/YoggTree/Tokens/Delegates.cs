using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoggTree.Core;

namespace YoggTree.Tokens
{
    public delegate bool CanComeAfterPredicate(TokenInstance previousToken, TokenParseContextBase context);

    public delegate bool CanComeBeforePredicate(TokenInstance nextToken, TokenParseContextBase context);

    public delegate bool IsValidInstancePredicate(TokenInstance instance, TokenParseContextBase context);

    public delegate bool CanComeAfterPredicate<T>(TokenInstance previousToken, T definition, TokenParseContextBase context) where T : TokenDefinitionBase;

    public delegate bool CanComeBeforePredicate<T>(TokenInstance nextToken, T definition, TokenParseContextBase context) where T : TokenDefinitionBase;

    public delegate bool IsValidInstancePredicate<T>(TokenInstance instance, T definition, TokenParseContextBase context) where T : TokenDefinitionBase;
}
