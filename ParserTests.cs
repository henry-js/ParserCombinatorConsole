using Sprache;
using ParserCombinatorConsole.Sprache;
using TUnit.Assertions.Extensions.Throws;
using ParserCombinatorConsole.Pidgin;

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
        var result = FilterGrammar.AttributePairKey.Parse();
    }
}
public class SpracheParserTests
{
    [Test]
    public async Task AnIdentifierIsASequenceOfCharacters()
    {
        var input = "name";
        var id = QuestionnaireGrammar.Identifier.Parse(input);
        await Assert.That(input).IsEqualTo(id);
    }

    [Test]
    public async Task AnIdentifierDoesNotIncludeSpaces()
    {
        var input = "a b";
        var parsed = QuestionnaireGrammar.Identifier.Parse(input);

        await Assert.That(parsed).IsEqualTo("a");
    }

    [Test]
    public async Task AnIdentifierCannotStartWithQuote()
    {
        var input = "\"name";

        await Assert.That(() => QuestionnaireGrammar.Identifier.Parse(input)).ThrowsException().OfType<ParseException>();
    }

    [Test]
    public async Task QuotedTextReturnsAValueBetweenQuotes()
    {
        var input = "\"this is text\"";
        var content = QuestionnaireGrammar.QuotedText.Parse(input);

        await Assert.That(content).IsEqualTo("this is text");
    }

    [Test]
    public async Task AQuestionIsAnIdentifierFollowedByAPrompt()
    {
        var input = "name \"Full Name\"";
        var question = QuestionnaireGrammar.Question.Parse(input);

        await Assert.That(question.Id).IsEqualTo("name");
        await Assert.That(question.Prompt).IsEqualTo("Full Name");
    }

}
