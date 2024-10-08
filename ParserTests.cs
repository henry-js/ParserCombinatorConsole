using Sprache;
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

        var result = FilterGrammar.AttributePairKey.ParseOrThrow(key);

        await Assert.That(result).IsNotNull();
    }

    [Test]
    [Arguments("a b")]
    [Arguments("o ne")]
    [Arguments("tw o")]
    public async Task AnAttributePairKeyDoesNotIncludeSpaces(string key)
    {
        var result = FilterGrammar.AttributePairKey.ParseOrThrow(key);

        await Assert.That(result.Name).IsNotEqualTo(key);
    }

    [Test]
    [Arguments("due", typeof(BuiltInAttributeKey))]
    [Arguments("custom", typeof(UserDefinedAttributeKey))]
    public async Task AnAttributePairKeyTypeCanBeParsedFromText(string text, Type keyType)
    {
        var result = FilterGrammar.AttributePairKey.ParseOrThrow(text);

        await Assert.That(result).IsAssignableTo(keyType);
    }

    [Test]
    [Arguments("1w")]
    [Arguments("2024-01-01T00:00:00")]
    [Arguments("2022-12-12")]
    public async Task AnAttributePairValueCanBeAlphaNumericContainingDashOrColon(string value)
    {
        var result = FilterGrammar.AttributePairValue.ParseOrThrow(value);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments("'i am a description'", "i am a description")]
    [Arguments("'2024-01-01T00:00:00'", "2024-01-01T00:00:00")]
    public async Task AnAttributePairValueShouldSupportSingleQuotedStrings(string value, string expected)
    {
        var result = FilterGrammar.AttributePairValue.ParseOrThrow(value);

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
        var result = FilterGrammar.AttributePair.ParseOrThrow(text);

        await Assert.That((result as AttributePair).Key)
        .IsEqualTo(new BuiltInAttributeKey(key))
        .Or
        .IsEqualTo(new UserDefinedAttributeKey(key));
    }


    [Test]
    [Arguments("due:8w and until:7w", ExpressionOperator.And)]
    [Arguments("due:9w until:8w", ExpressionOperator.And)]
    [Arguments("due:10w or until:9w", ExpressionOperator.Or)]
    [Arguments("project:work or project:notWork", ExpressionOperator.Or)]
    public async Task ABinaryExpressionCanBeParsedFromText(string text, ExpressionOperator @operator)
    {
        var result = FilterGrammar.FilterExpression.ParseOrThrow(text);
        await Assert.That(result).IsAssignableTo(typeof(BinaryFilterExpression));

        var resultVal = result as BinaryFilterExpression;
        await Assert.That(resultVal.Operator).IsEqualTo(@operator);
    }

    // [Test]
    // [Arguments("due:tomorrow", typeof(AttributePair))]
    // [Arguments("due:tomorrow or project:home", typeof(BinaryFilterExpression))]
    // public async Task AParenthesizedExpressionCanBeParsedFromText(string text, Type t)
    // {
    //     var result = FilterGrammar.FilterExpression.ParseOrThrow(text);

    //     await Assert.That(result).IsAssignableTo(t);
    // }

    [Test]
    [Arguments("-3*(()370+9)*37")]
    public async Task ExprParserShouldParseEquation(string text)
    {
        var result = ExprParser.ParseOrThrow(text);
        await Assert.That(result.Success).IsTrue();
    }
}

// public class SpracheParserTests
// {
//     [Test]
//     public async Task AnIdentifierIsASequenceOfCharacters()
//     {
//         var input = "name";
//         var id = QuestionnaireGrammar.Identifier.ParseOrThrow(input);
//         await Assert.That(input).IsEqualTo(id);
//     }

//     [Test]
//     public async Task AnIdentifierDoesNotIncludeSpaces()
//     {
//         var input = "a b";
//         var parsed = QuestionnaireGrammar.Identifier.ParseOrThrow(input);

//         await Assert.That(parsed).IsEqualTo("a");
//     }

//     [Test]
//     public async Task AnIdentifierCannotStartWithQuote()
//     {
//         var input = "\"name";

//         await Assert.That(() => QuestionnaireGrammar.Identifier.ParseOrThrow(input)).ThrowsException().OfType<Sprache.ParseException>();
//     }

//     [Test]
//     public async Task QuotedTextReturnsAValueBetweenQuotes()
//     {
//         var input = "\"this is text\"";
//         var content = QuestionnaireGrammar.QuotedText.ParseOrThrow(input);

//         await Assert.That(content).IsEqualTo("this is text");
//     }

//     [Test]
//     public async Task AQuestionIsAnIdentifierFollowedByAPrompt()
//     {
//         var input = "name \"Full Name\"";
//         var question = QuestionnaireGrammar.Question.ParseOrThrow(input);

//         await Assert.That(question.Id).IsEqualTo("name");
//         await Assert.That(question.Prompt).IsEqualTo("Full Name");
//     }
// }
