using Pidgin;
using static Pidgin.Parser<char>;
// using static Pidgin.Parser<string>;
using static Pidgin.Parser;

namespace ParserCombinatorConsole.PidginParser;

public static class FilterGrammar
{
    private static readonly Parser<char, char> _colon = Token(':');
    private static readonly Parser<char, char> _dash = Token('-');
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

    public static readonly Parser<char, string> AttributePairKey = OneOf(Letter).ManyString();
    public static readonly Parser<char, string> AttributePairValue = _string.Or(_attributeValue);
    public static readonly Parser<char, AttributePair> AttributePair = Map
    (
        (key, value) => new AttributePair(key, value),
        Try(_builtInAttribute).Or(_udaAttribute),
        _colon.Then(AttributePairValue)
    );
}

public abstract record Expression;
public record AttributeExpression(AttributePair Pair) : Expression;
public record AndExpression(Expression Left, Expression Right) :

public record BuiltInAttributeKey(string Name) : Key
{
    public static readonly string[] keys = ["due", "until", "project", "end", "entry", "estimate", "id", "modified", "parent", "priority", "recur", "scheduled", "start", "status", "wait"];
}
public record UserDefinedAttributeKey(string Name) : Key { }
public abstract record Key;
public record AttributePair(Key Key, string Value);
public record TagFilter(TagModifier Modifier, string Value);
public enum TagModifier { Include, Exclude }
