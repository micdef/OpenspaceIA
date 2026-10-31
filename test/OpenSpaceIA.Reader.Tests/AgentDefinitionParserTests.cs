/*
 *	@file	test/OpenSpaceIA.Reader.Tests/AgentDefinitionParserTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the front matter of an agent definition
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the front matter of an agent definition
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves which forms of front matter are read, and that every form that is not read is reported
/// instead of giving a wrong value.</para>
/// <para><b>Behavior:</b> invented texts only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class AgentDefinitionParserTests
{
    #region Members

    #region Constants

    /// <summary>
    /// Invented path of the file, given to the parser for the problems
    /// </summary>
    private const string FilePath = "/invented/.claude/agents/helper.md";

    #endregion

    #region Variables

    // #N/A

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Enumerations

    // #N/A

    #endregion

    #region Services

    // #N/A

    #endregion

    #region Accessors/Properties

    #region Members & Properties

    // #N/A

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    // #N/A

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to join lines with the line feed, whatever the line endings of this source file
    /// </summary>
    /// <param name="lines">[Cannot be null] Lines of the text</param>
    /// <returns>The lines joined with a line feed</returns>
    private static string Lines(params string[] lines)
    {
        // Function's return
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Method used to parse a text with a new list of problems
    /// </summary>
    /// <param name="text">[Cannot be null] Text of the invented file</param>
    /// <param name="problems">The problems reported by the parser</param>
    /// <returns>[Can be null] The agent type read, or null when the file is skipped</returns>
    private static AgentType? Parse(string text, out List<ReadProblem> problems)
    {
        // Parsing the text
        problems = new List<ReadProblem>();

        // Function's return
        return AgentDefinitionParser.Parse(FilePath, text, problems);
    }

    #endregion

    #region Publics

    /// <summary>
    /// The name, the description and the model are read, whatever the order of the keys; the other keys are left out
    /// </summary>
    [Fact]
    public void NameDescriptionAndModelAreReadInAnyOrder()
    {
        // Parsing a definition whose keys are not in the usual order
        AgentType? agentType = Parse(Lines("---", "model: sonnet", "tools: Read, Grep", "name: helper", "description: Helps with tests.", "---", "Body of the prompt."), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", "Helps with tests.", "sonnet"));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Lines that end with a carriage return and a line feed are read like lines that end with a line feed
    /// </summary>
    [Fact]
    public void CarriageReturnsAreLeftOut()
    {
        // Parsing a definition written with Windows line endings
        AgentType? agentType = Parse("---\r\nname: helper\r\ndescription: Helps.\r\nmodel: opus\r\n---\r\n", out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", "Helps.", "opus"));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A definition without description or model is read: both are optional in the model of the office
    /// </summary>
    [Fact]
    public void DescriptionAndModelAreOptional()
    {
        // Parsing a definition with a name only
        AgentType? agentType = Parse(Lines("---", "name: helper", "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", null, null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A value between quotes is read without its quotes; two single quotes inside single quotes stand for one
    /// </summary>
    [Fact]
    public void QuotedValuesAreReadWithoutTheirQuotes()
    {
        // Parsing a definition with quoted values
        AgentType? agentType = Parse(Lines("---", "name: \"helper\"", "description: 'It''s a helper: it helps.'", "model: \"\"", "---"), out List<ReadProblem> problems);

        // Checking the agent type: an empty model is no model
        agentType.ShouldBe(new AgentType("helper", "It's a helper: it helps.", null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A comment after a plain value is left out, but a hash without a space before it is part of the value
    /// </summary>
    [Fact]
    public void CommentAfterAPlainValueIsLeftOut()
    {
        // Parsing a definition with a comment and a hash inside a value
        AgentType? agentType = Parse(Lines("---", "name: helper  # the name", "description: Reviews C# code.", "model: opus #strong", "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", "Reviews C# code.", "opus"));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Comment lines, blank lines and indented lines of the other keys are left out
    /// </summary>
    [Fact]
    public void CommentBlankAndIndentedLinesAreLeftOut()
    {
        // Parsing a definition with a list under another key
        AgentType? agentType = Parse(Lines("---", "# An invented agent", "", "name: helper", "skills:", "  - one", "  - two", "description: Helps.", "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", "Helps.", null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// The text after the front matter is never read, even when it looks like a key
    /// </summary>
    [Fact]
    public void BodyIsNotRead()
    {
        // Parsing a definition whose body holds keys
        AgentType? agentType = Parse(Lines("---", "name: helper", "---", "name: other", "model: haiku"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", null, null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A file that does not start with a front matter is skipped and reported
    /// </summary>
    /// <param name="text">Text without front matter</param>
    [Theory]
    [InlineData("")]
    [InlineData("name: helper")]
    [InlineData("\n---\nname: helper\n---")]
    [InlineData("----\nname: helper\n---")]
    public void FileWithoutFrontMatterIsSkipped(string text)
    {
        // Parsing the text
        AgentType? agentType = Parse(text, out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(FilePath);
        problems[0].Reason.ShouldContain("does not start with a front matter");
    }

    /// <summary>
    /// The delimiters of the front matter may be followed by spaces or a tab
    /// </summary>
    [Fact]
    public void DelimitersMayBeFollowedBySpacesOrATab()
    {
        // Parsing a definition whose delimiters end with spaces and a tab
        AgentType? agentType = Parse(Lines("---  ", "name: helper", "---\t"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", null, null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A front matter that is never closed is skipped and reported
    /// </summary>
    [Fact]
    public void FrontMatterNotClosedIsSkipped()
    {
        // Parsing a front matter without its closing line
        AgentType? agentType = Parse(Lines("---", "name: helper", "description: Helps."), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("never closed");
    }

    /// <summary>
    /// A definition without a name is skipped and reported, as Claude Code skips it
    /// </summary>
    /// <param name="nameLine">Line of the name, or a line of another key</param>
    [Theory]
    [InlineData("description: Helps.")]
    [InlineData("name:")]
    [InlineData("name:    ")]
    [InlineData("name: \"  \"")]
    [InlineData("name: # a comment")]
    [InlineData("name:helper")]
    [InlineData("name: ~")]
    [InlineData("name: null")]
    [InlineData("name: [helper]")]
    [InlineData("name: helper\n  continued")]
    public void DefinitionWithoutANameIsSkipped(string nameLine)
    {
        // Parsing a definition without a usable name
        AgentType? agentType = Parse(Lines("---", nameLine, "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("no name that can be read");
    }

    /// <summary>
    /// A name with a colon is refused by Claude Code: the file is skipped and reported
    /// </summary>
    [Fact]
    public void NameWithAColonIsSkipped()
    {
        // Parsing the definition
        AgentType? agentType = Parse(Lines("---", "name: my-plugin:reviewer", "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("name is refused");
    }

    /// <summary>
    /// A name of more than 256 characters is refused by Claude Code: the file is skipped and reported
    /// </summary>
    [Fact]
    public void NameOfMoreThanTwoHundredFiftySixCharactersIsSkipped()
    {
        // Parsing a definition with a name of 257 characters
        AgentType? agentType = Parse(Lines("---", "name: " + new string('x', 257), "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("name is refused");
    }

    /// <summary>
    /// A name with spaces around it, between quotes, is refused: it would not be the name that Claude Code shows
    /// </summary>
    [Fact]
    public void NameWithSpacesAroundItIsSkipped()
    {
        // Parsing a definition whose quoted name has spaces around it
        AgentType? agentType = Parse(Lines("---", "name: \" helper \"", "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("name is refused");
    }

    /// <summary>
    /// The spaces around a plain value, and before its comment, are not part of it; the first comment wins, whether it
    /// follows a space or a tab
    /// </summary>
    /// <param name="modelLine">Line of the model</param>
    [Theory]
    [InlineData("model:   opus   ")]
    [InlineData("model: opus   # strong")]
    [InlineData("model: opus\t# strong #very")]
    [InlineData("model:\topus")]
    public void SpacesAroundAPlainValueAreLeftOut(string modelLine)
    {
        // Parsing the definition
        AgentType? agentType = Parse(Lines("---", "name: helper", modelLine, "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", null, "opus"));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A name of exactly 256 characters is read
    /// </summary>
    [Fact]
    public void NameOfTwoHundredFiftySixCharactersIsRead()
    {
        // Parsing a definition with the longest name accepted
        string name = new string('x', 256);
        AgentType? agentType = Parse(Lines("---", "name: " + name, "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldNotBeNull();
        agentType.Name.ShouldBe(name);
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A description or a model written in a form the reader does not read (on several lines, or with an escape) is left
    /// empty and reported; the agent is still read
    /// </summary>
    /// <param name="key">Key whose value is not read</param>
    /// <param name="valueLines">Value of the key, and the lines that follow it</param>
    [Theory]
    [InlineData("description", "> Helps\n  with tests.")]
    [InlineData("description", "| Helps\n  with tests.")]
    [InlineData("description", "\n  Helps with tests.")]
    [InlineData("description", "\"Helps with \\\"tests\\\".\"")]
    [InlineData("description", "\"Helps with tests.")]
    [InlineData("description", "Helps with\n  tests.")]
    [InlineData("description", "\"Helps with\n  tests.\"")]
    [InlineData("description", "# a comment")]
    [InlineData("description", "\"C:\\agents\"")]
    [InlineData("model", "'opus")]
    [InlineData("model", "'op'us'")]
    [InlineData("model", "\"")]
    [InlineData("model", "\"opus\" # strong")]
    [InlineData("model", "[opus, sonnet]")]
    [InlineData("model", "{name: opus}")]
    [InlineData("model", "&anchor opus")]
    [InlineData("model", "*anchor")]
    [InlineData("model", "!!str opus")]
    [InlineData("model", "%opus")]
    [InlineData("model", "@opus")]
    [InlineData("model", "`opus`")]
    [InlineData("model", "~")]
    [InlineData("model", "null")]
    public void ValueInAFormNotReadIsLeftEmptyAndReported(string key, string valueLines)
    {
        // Parsing the definition
        AgentType? agentType = Parse(Lines("---", "name: helper", key + ": " + valueLines, "---"), out List<ReadProblem> problems);

        // Checking that the agent is read without the value, and that the value is reported
        agentType.ShouldBe(new AgentType("helper", null, null));
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(FilePath);
        problems[0].Reason.ShouldContain($"key: {key} is written in a form that is not read");
    }

    /// <summary>
    /// The indented lines under a key that is not read do not make the next key a value on several lines
    /// </summary>
    [Fact]
    public void IndentedLinesBelongToTheKeyAboveThem()
    {
        // Parsing a definition where a list follows the description
        AgentType? agentType = Parse(Lines("---", "description: Helps.", "skills:", "  - one", "name: helper", "---"), out List<ReadProblem> problems);

        // Checking the agent type
        agentType.ShouldBe(new AgentType("helper", "Helps.", null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A key that is read and given twice is ambiguous: the file is skipped and reported
    /// </summary>
    /// <param name="key">Key given twice</param>
    [Theory]
    [InlineData("name")]
    [InlineData("description")]
    [InlineData("model")]
    public void KeyGivenTwiceSkipsTheFile(string key)
    {
        // Parsing a definition with a key given twice
        AgentType? agentType = Parse(Lines("---", "name: helper", key + ": first", key + ": second", "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain($"key: {key} is given twice");
    }

    /// <summary>
    /// A key is case-sensitive, as in Claude Code: "Name" is not the name
    /// </summary>
    [Fact]
    public void KeysAreCaseSensitive()
    {
        // Parsing a definition whose key is not in lowercase
        AgentType? agentType = Parse(Lines("---", "Name: helper", "---"), out List<ReadProblem> problems);

        // Checking that the file is skipped for want of a name
        agentType.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("no name that can be read");
    }

    /// <summary>
    /// The arguments of the parser are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Calling the parser without each argument
        ArgumentNullException withoutPath = Should.Throw<ArgumentNullException>(() => AgentDefinitionParser.Parse(null!, "---", new List<ReadProblem>()));
        ArgumentNullException withoutText = Should.Throw<ArgumentNullException>(() => AgentDefinitionParser.Parse(FilePath, null!, new List<ReadProblem>()));
        ArgumentNullException withoutProblems = Should.Throw<ArgumentNullException>(() => AgentDefinitionParser.Parse(FilePath, "---", null!));

        // Checking the parameters in fault
        withoutPath.ParamName.ShouldBe("filePath");
        withoutText.ParamName.ShouldBe("text");
        withoutProblems.ParamName.ShouldBe("problems");
    }

    #endregion

    #endregion

    #region Overrides

    #region Methods

    // #N/A

    #endregion

    #region Operators

    // #N/A

    #endregion

    #endregion
}
