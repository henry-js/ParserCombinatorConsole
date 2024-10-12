using Pidgin;
using Pidgin.Expression;
using Sprache;
using static Pidgin.Parser<char>;
using static Pidgin.Parser<string>;
using static Pidgin.Parser;

namespace ParserCombinatorConsole.PidginParser;

public static class FilterGrammar
{
    private static readonly Parser<char, char> _colon
        = Token(':');
    private static readonly Parser<char, char> _dash
        = Token('-');
    private static readonly Parser<char, char> _lParen
        = Try(Char('('));
    private static readonly Parser<char, char> _rParen
        = Try(SkipWhitespaces.Then(Char(')')));
    private static readonly Parser<char, string> _string
        = Token(c => c != '\'')
            .ManyString()
            .Between(Char('\''));
    private static Parser<char, Func<Expr, Expr, Expr>> Binary(Parser<char, BinaryOperator> op)
        => op.Select<Func<Expr, Expr, Expr>>(type => (l, r) => new BinaryFilter(l, type, r));
    private static readonly Parser<char, Func<Expr, Expr, Expr>> _and
        = Binary(
            Try(OneOf(
                Try(String("and").Between(SkipWhitespaces)),
                WhitespaceString
            )).ThenReturn(BinaryOperator.And)
        );
    private static readonly Parser<char, Func<Expr, Expr, Expr>> _or
        = Binary(
            Try(String("or").Between(SkipWhitespaces)).ThenReturn(BinaryOperator.Or)
        );
    private static readonly Parser<char, TagOperator> _tagOperator
        = OneOf(
            Char('+').ThenReturn(TagOperator.Include),
            Char('-').ThenReturn(TagOperator.Exclude)
        );
    private static readonly Parser<char, string> _attributeValue
        = OneOf(
            LetterOrDigit,
            _dash,
            _colon
        ).ManyString();
    private static readonly Parser<char, Key> _builtInAttribute
        = OneOf(
        Constants.BuiltInKeys.Select(k => String(k))
        )
        .Select(a => new BuiltInAttributeKey(a))
        .Cast<Key>();
    private static readonly Parser<char, Key> _udaAttribute
        = Letter
            .AtLeastOnceString()
            .Select(s => new UserDefinedAttributeKey(s))
            .Cast<Key>();
    internal static readonly Parser<char, Key> _attributePairKey
        = OneOf(
            Try(_builtInAttribute),
            _udaAttribute
        );
    internal static readonly Parser<char, string> _attributePairValue = _string.Or(_attributeValue);
    internal static readonly Parser<char, Expr> _attributePair
        = Map(
            (key, value) => new AttributePair(key, value),
            _attributePairKey,
            _colon.Then(_attributePairValue)
        ).TraceResult().Cast<Expr>();

    internal static readonly Parser<char, Expr> _tagExpression
        = Map(
            (modifier, value) => new Tag(modifier, value),
            _tagOperator,
            LetterOrDigit.AtLeastOnceString()
        ).Cast<Expr>();

    private static readonly Parser<char, Expr> _filtExpr = ExpressionParser.Build(
        expr => (
            OneOf(
                expr.Between(_lParen, _rParen),
                _attributePair,
                _tagExpression
            )
        ), [
            Operator.InfixL(_or),
            Operator.InfixL(_and),
        ]
        );

    public static Result<char, Expr> Parse(string input)
        => _filtExpr.Parse(input);

    public static Expr ParseFilterExpression(string input)
        => _filtExpr.ParseOrThrow(input);
}
public static class Constants
{
    public static readonly string[] BuiltInKeys = ["due", "until", "project", "end", "entry", "estimate", "id", "modified", "parent", "priority", "recur", "scheduled", "start", "status", "wait"];

}
public abstract record Key(string Name);
public record BuiltInAttributeKey(string Name) : Key(Name);
public record UserDefinedAttributeKey(string Name) : Key(Name);
public record AttributePair(Key Key, string Value) : Expr;
public abstract record Expr;
public record BinaryFilter(Expr Left, BinaryOperator Operator, Expr Right) : Expr;
public record Tag(TagOperator Modifier, string Value) : Expr;
public enum TagOperator { Include, Exclude }
public enum BinaryOperator { And, Or }
