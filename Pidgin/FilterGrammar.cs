using System.Linq.Expressions;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace ParserCombinatorConsole.PidginParser;

public static class FilterGrammar
{
    private static readonly Parser<char, char> _colon = Token(':');
    private static readonly Parser<char, char> _dash = Token('-');
    private static readonly Parser<char, char> _lParen = Try(Char('('));
    private static readonly Parser<char, char> _rParen = Try(SkipWhitespaces.Then(Char(')')));
    private static readonly Parser<char, ExpressionOperator> _and = OneOf(
        Try(SkipWhitespaces.Then(String("and"))),
        WhitespaceString
        ).ThenReturn(ExpressionOperator.And);
    private static readonly Parser<char, ExpressionOperator> _or = SkipWhitespaces.Then(String("or")).ThenReturn(ExpressionOperator.Or);
    private static readonly Parser<char, string> _attributeValue = OneOf(LetterOrDigit, _dash, _colon).ManyString();
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
    private static readonly Parser<char, string> _string =
        Token(c => c != '\'')
        .ManyString()
        .Between(Char('\''));

    public static readonly Parser<char, Key> AttributePairKey = OneOf(
        Try(_builtInAttribute),
        _udaAttribute
    );
    public static readonly Parser<char, string> AttributePairValue = _string.Or(_attributeValue);
    public static readonly Parser<char, ValueExpression> AttributePair = Map
    (
        (key, value) => new AttributePair(key, value),
        AttributePairKey,
        _colon.Then(AttributePairValue)
    ).TraceResult().Cast<ValueExpression>();

    public static readonly Parser<char, Expression> FilterExpression =
        Rec(() => OneOf(
            AttributePair.Cast<Expression>(),
            BinaryExpression.Trace("trying to parse binary expr").Trace("in one of expression").Cast<Expression>()
        )).TraceResult();

    public static readonly Parser<char, Expression> BinaryExpression =
        Map(
            (left, op, right) => new BinaryFilterExpression(left, op, right),
            SkipWhitespaces.Then(FilterExpression).TraceResult(),
            OneOf(_or, _and).Between(SkipWhitespaces).TraceResult(),
            FilterExpression.TraceResult()
        ).TraceResult().Cast<Expression>();
}

public abstract record Expression;
public abstract record ValueExpression : Expression;
public record AttributeExpression(AttributePair Pair) : ValueExpression;
public record TagExpression(Tag Tag) : ValueExpression;
public record BinaryFilterExpression(Expression Left, ExpressionOperator Operator, Expression Right) : Expression;

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
public record AttributePair(Key Key, string Value) : ValueExpression;
public record Tag(TagModifier Modifier, string Value) : ValueExpression;
public enum TagModifier { Include, Exclude }
public enum ExpressionOperator { And, Or }
