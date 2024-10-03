using Sprache;
using ParserCombinatorConsole.SpracheParser;
using TUnit.Assertions.Extensions.Throws;
using ParserCombinatorConsole.PidginParser;
using Pidgin;

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

        var result = FilterGrammar.AttributePairKey.Parse(key);

        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    [Arguments("a b")]
    [Arguments("o ne")]
    [Arguments("tw o")]
    public async Task AnAttributePairKeyDoesNotIncludeSpaces(string key)
    {
        var result = FilterGrammar.AttributePairKey.Parse(key);
        await Assert.That(result.Value).IsNotEqualTo(key);
    }

    [Test]
    [Arguments("1w")]
    [Arguments("2024-01-01T00:00:00")]
    [Arguments("2022-12-12")]
    public async Task AnAttributePairValueCanBeAlphaNumericContainingDashOrColon(string value)
    {
        var result = FilterGrammar.AttributePairValue.Parse(value);
        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    [Arguments("'i am a description'", "i am a description")]
    [Arguments("'2024-01-01T00:00:00'", "2024-01-01T00:00:00")]
    public async Task AnAttributePairValueShouldSupportSingleQuotedStrings(string value, string expected)
    {
        var result = FilterGrammar.AttributePairValue.Parse(value);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Value).IsEqualTo(expected);
    }

    [Test]
    [Arguments("due:tomorrow", "due", "tomorrow")]
    [Arguments("until:2w", "until", "2w")]
    [Arguments("project:home", "project", "home")]
    [Arguments("project:WORK", "project", "WORK")]
    [Arguments("due:2024-01-02T00:00:00", "due", "2024-01-02T00:00:00")]
    [Arguments("due:'i am a quoted string'", "due", "i am a quoted string")]

    public async Task AnAttributePairCanBeParsedFromText(string text, string key, string value)
    {
        var result = FilterGrammar.AttributePair.Parse(text);

        await Assert.That(result.Success).IsTrue();

        await Assert.That(result.Value.Key).IsEqualTo(new BuiltInAttributeKey(key));
    }

    [Test]
    [Arguments("due:tomorrow", typeof(BuiltInAttributeKey))]
    [Arguments("custom:tomorrow", typeof(UserDefinedAttributeKey))]
    public async Task AnAttributePairKeyTypeCanBeParsedFromText(string text, Type keyType)
    {
        var result = FilterGrammar.AttributePair.Parse(text);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Value.Key).IsAssignableTo(keyType);
    }

    // [Test]
    // [Arguments("due:tomorrow and until:1w", typeof(AttributePair), typeof(ExpressionOperator), typeof(AttributePair))]
    // public async Task ASimpleFilterExpressionCanBeParsedFromText(string text, Type attr1, Type op, Type attr2)
    // {

    // }
}

// public class SpracheParserTests
// {
//     [Test]
//     public async Task AnIdentifierIsASequenceOfCharacters()
//     {
//         var input = "name";
//         var id = QuestionnaireGrammar.Identifier.Parse(input);
//         await Assert.That(input).IsEqualTo(id);
//     }

//     [Test]
//     public async Task AnIdentifierDoesNotIncludeSpaces()
//     {
//         var input = "a b";
//         var parsed = QuestionnaireGrammar.Identifier.Parse(input);

//         await Assert.That(parsed).IsEqualTo("a");
//     }

//     [Test]
//     public async Task AnIdentifierCannotStartWithQuote()
//     {
//         var input = "\"name";

//         await Assert.That(() => QuestionnaireGrammar.Identifier.Parse(input)).ThrowsException().OfType<Sprache.ParseException>();
//     }

//     [Test]
//     public async Task QuotedTextReturnsAValueBetweenQuotes()
//     {
//         var input = "\"this is text\"";
//         var content = QuestionnaireGrammar.QuotedText.Parse(input);

//         await Assert.That(content).IsEqualTo("this is text");
//     }

//     [Test]
//     public async Task AQuestionIsAnIdentifierFollowedByAPrompt()
//     {
//         var input = "name \"Full Name\"";
//         var question = QuestionnaireGrammar.Question.Parse(input);

//         await Assert.That(question.Id).IsEqualTo("name");
//         await Assert.That(question.Prompt).IsEqualTo("Full Name");
//     }
// }
