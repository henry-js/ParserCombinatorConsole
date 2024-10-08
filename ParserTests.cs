using Sprache;
using ParserCombinatorConsole.PidginParser;
using Pidgin;
using ParserCombinatorConsole.Pidgin;
using System.Net.Http.Headers;
using TUnit.Assertions.Extensions.Generic;

namespace ParserCombinatorConsole;

public class PidginParserTests
{
    [Test]
    [Arguments("one")]
    [Arguments("two")]
    [Arguments("three")]
    [Arguments("project")]
    [Arguments("due")]
    [Arguments("until")]
    public async Task AnAttributePairKeyIsASequenceOfLetters(string key)
    {

        var result = FilterGrammar._attributePairKey.ParseOrThrow(key);

        await Assert.That(result).IsNotNull();
    }

    [Test]
    [Arguments("a b")]
    [Arguments("o ne")]
    [Arguments("tw o")]
    public async Task AnAttributePairKeyDoesNotIncludeSpaces(string key)
    {
        var result = FilterGrammar._attributePairKey.ParseOrThrow(key);

        await Assert.That(result.Name).IsNotEqualTo(key);
    }

    [Test]
    [Arguments("due", typeof(BuiltInAttributeKey))]
    [Arguments("custom", typeof(UserDefinedAttributeKey))]
    public async Task AnAttributePairKeyTypeCanBeParsedFromText(string text, Type keyType)
    {
        var result = FilterGrammar._attributePairKey.ParseOrThrow(text);

        await Assert.That(result).IsAssignableTo(keyType);
    }

    [Test]
    [Arguments("1w")]
    [Arguments("2024-01-01T00:00:00")]
    [Arguments("2022-12-12")]
    public async Task AnAttributePairValueCanBeAlphaNumericContainingDashOrColon(string value)
    {
        var result = FilterGrammar._attributePairValue.ParseOrThrow(value);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments("'i am a description'", "i am a description")]
    [Arguments("'2024-01-01T00:00:00'", "2024-01-01T00:00:00")]
    public async Task AnAttributePairValueShouldSupportSingleQuotedStrings(string value, string expected)
    {
        var result = FilterGrammar._attributePairValue.ParseOrThrow(value);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("due:tomorrow", "due", "tomorrow")]
    [Arguments("until:2w", "until", "2w")]
    [Arguments("project:home", "project", "home")]
    [Arguments("project:WORK", "project", "WORK")]
    [Arguments("due:2024-01-02T00:00:00", "due", "2024-01-02T00:00:00")]
    [Arguments("due:'i am a quoted string'", "due", "i am a quoted string")]
    [Arguments("uda:'i am a UDA'", "uda", "i am a UDA")]

    public async Task AnAttributePairCanBeParsedFromText(string text, string key, string value)
    {
        var result = FilterGrammar.ParseExpression(text);

        await Assert.That((result as AttributePair)?.Key)
        .IsEquivalentTo(new BuiltInAttributeKey(key))
        .Or
        .IsEquivalentTo(new UserDefinedAttributeKey(key));
    }


    [Test]
    [Arguments("due:8w and until:7w", BinaryOperator.And)]
    [Arguments("due:9w until:8w", BinaryOperator.And)]
    [Arguments("due:10w or until:9w", BinaryOperator.Or)]
    [Arguments("project:work or project:notWork", BinaryOperator.Or)]
    public async Task ABinaryExpressionCanBeParsedFromText(string text, BinaryOperator @operator)
    {
        var result = FilterGrammar.ParseExpression(text);

        // await Assert.That(result.Success).IsTrue();
        await Assert.That(result).IsAssignableTo(typeof(BinaryFilter));

        var resultVal = result as BinaryFilter;
        await Assert.That(resultVal?.Operator).IsEquivalentTo(@operator);
    }

    [Test]
    [Arguments("+test", TagModifier.Include)]
    [Arguments("-test", TagModifier.Exclude)]
    public async Task ATagExpressionCanBeParsedFromText(string tagText, TagModifier modifier)
    {
        var result = FilterGrammar.ParseExpression(tagText);

        await Assert.That(result).IsAssignableTo(typeof(Tag));
        var tag = result as Tag;
        await Assert.That(tag.Modifier).IsEqualTo(modifier);

    }

    [Test]
    [Arguments("due:tomorrow", typeof(AttributePair))]
    [Arguments("+test or due:tomorrow", typeof(BinaryFilter))]
    [Arguments("due:tomorrow or project:home", typeof(BinaryFilter))]
    public async Task DifferentExpressionsCanBeParsedFromText(string text, Type t)
    {
        var result = FilterGrammar.ParseExpression(text);

        await Assert.That(result).IsAssignableTo(t);
    }
}
