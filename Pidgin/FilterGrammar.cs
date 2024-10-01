using Pidgin;
using static Pidgin.Parser<char>;
using static Pidgin.Parser;

namespace ParserCombinatorConsole.Pidgin;

public static class FilterGrammar
{
    public static readonly Parser<string, string> AttributePairKey = String()
}

public record AttributePair(string Key, string Value);
public record TagFilter(TagModifier Modifier, string Value);

public enum TagModifier { Include, Exclude }
