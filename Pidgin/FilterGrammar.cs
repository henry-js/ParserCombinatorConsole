using System.Linq.Expressions;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser<char>;
using static Pidgin.Parser;

namespace ParserCombinatorConsole.PidginParser;

public static class FilterGrammar
{
    private static readonly Parser<char, char> _colon = Token(':');
    private static readonly Parser<char, char> _dash = Token('-');
    private static readonly Parser<char, char> _lParen = Try(Char('('));
    private static readonly Parser<char, char> _rParen = Try(SkipWhitespaces.Then(Char(')')));
    private static Parser<char, Func<Expr, Expr, Expr>> Binary(Parser<char, BinaryOperator> op)
        => op.Select<Func<Expr, Expr, Expr>>(type => (l, r) => new BinaryFilterExpression(l, type, r));
    private static readonly Parser<char, Func<Expr, Expr, Expr>> _and = Binary(
        OneOf(
            Try(String("and").Between(SkipWhitespaces)),
            WhitespaceString
        ).ThenReturn(BinaryOperator.And)
    );
    private static readonly Parser<char, Func<Expr, Expr, Expr>> _or = Binary(
        String("or").Between(SkipWhitespaces).ThenReturn(BinaryOperator.Or)
    );

    private static readonly Parser<char, string> _attributeValue = OneOf(
        LetterOrDigit,
        _dash,
        _colon).ManyString();
    private static readonly Parser<char, Key> _builtInAttribute
        = OneOf(
        BuiltInAttributeKey.keys.Select(k => String(k))
        )
        .Select(a => new BuiltInAttributeKey(a))
        .Cast<Key>();
    private static readonly Parser<char, Key> _udaAttribute
        = Letter
            .AtLeastOnceString()
            .Select(s => new UserDefinedAttributeKey(s))
            .Cast<Key>();
    private static readonly Parser<char, string> _string
    = Token(c => c != '\'')
        .ManyString()
        .Between(Char('\''));

    public static readonly Parser<char, Key> AttributePairKey
        = OneOf(
            Try(_builtInAttribute),
            _udaAttribute
        );
    public static readonly Parser<char, string> AttributePairValue = _string.Or(_attributeValue);
    public static readonly Parser<char, Expr> AttributePair
        = Map(
            (key, value) => new AttributePair(key, value),
            AttributePairKey,
            _colon.Then(AttributePairValue)
        ).TraceResult().Cast<Expr>();

    private static readonly Parser<char, Expr> _expr = ExpressionParser.Build<char, Expr>(
        expr => (
            OneOf(
                expr.Between(_lParen, _rParen),
                AttributePair
            )
        ), [
            Operator.InfixL(_and),
            Operator.InfixL(_or)
        ]
    );

    public static Result<char, Expr> Parse(string input)
        => _expr.Parse(input);

    public static Expr ParseOrThrow(string input)
        => _expr.ParseOrThrow(input);
    // public static readonly Parser<char, Expression> FilterExpression =
    //     Rec(() => OneOf(
    //         BinaryExpression.Trace("trying to parse binary expr").Trace("in one of expression").Cast<Expression>(),
    //         AttributePair.Cast<Expression>()
    //     )).TraceResult();

    // public static readonly Parser<char, Expression> BinaryExpression =
    //     Map(
    //         (left, op, right) => new BinaryFilterExpression(left, op, right),
    //         SkipWhitespaces.Then(FilterExpression).TraceResult(),
    //         OneOf(_or, _and).Between(SkipWhitespaces).TraceResult(),
    //         FilterExpression.TraceResult()
    //     ).TraceResult().Cast<Expression>();
}

public abstract record Expr;
public record TagExpression(Tag Tag) : Expr;
public record BinaryFilterExpression(Expr Left, BinaryOperator Operator, Expr Right) : Expr;

public record BuiltInAttributeKey : Key
{
    public static readonly string[] keys = ["due", "until", "project", "end", "entry", "estimate", "id", "modified", "parent", "priority", "recur", "scheduled", "start", "status", "wait"];
    public BuiltInAttributeKey(string name) : base(name)
    {
    }
}
public record UserDefinedAttributeKey : Key
{
    public UserDefinedAttributeKey(string name) : base(name)
    {
    }
}
public abstract record Key(string Name);
public record AttributePair(Key Key, string Value) : Expr;
public record Tag(TagModifier Modifier, string Value) : Expr;
public enum TagModifier { Include, Exclude }
public enum BinaryOperator { And, Or }
