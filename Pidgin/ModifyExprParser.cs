using Pidgin;
using Pidgin.Expression;
using Sprache;
using static Pidgin.Parser;
using static Pidgin.Parser<string>;

namespace ParserCombinatorConsole.Pidgin
{
    public static class ModifyExprParser
    {

        private static readonly Parser<IEnumerable<string>, Expr> _modExpr
            = Token("foo");

        public static Expr ParseModifyExpression(List<string> input)
            => _modExpr.ParseOrThrow(input);
    }
}
