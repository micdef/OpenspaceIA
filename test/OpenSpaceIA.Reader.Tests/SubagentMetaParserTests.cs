/*
 *	@file	test/OpenSpaceIA.Reader.Tests/SubagentMetaParserTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the description of a sub-agent run
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the description of a sub-agent run (<c>agent-&lt;id&gt;.meta.json</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that the agent type is required, and that the optional facts are read only when they are
/// valid.</para>
/// <para><b>Behavior:</b> invented texts only, shaped like the files measured on 2026-10-31; no file, no state shared
/// between tests.</para>
/// </remarks>
public sealed class SubagentMetaParserTests
{
    #region Members

    #region Constants

    /// <summary>
    /// Invented path of the file, for the problems
    /// </summary>
    private const string FilePath = "/invented/.claude/projects/p/s/subagents/agent-a1.meta.json";

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
    /// Method used to parse a text with a new list of problems
    /// </summary>
    /// <param name="text">[Cannot be null] Text of the invented file</param>
    /// <param name="problems">The problems reported by the parser</param>
    /// <returns>[Can be null] The description, or null when the file cannot give one</returns>
    private static SubagentMeta? Parse(string text, out List<ReadProblem> problems)
    {
        // Parsing the text
        problems = new List<ReadProblem>();

        // Function's return
        return SubagentMetaParser.Parse(FilePath, text, problems);
    }

    #endregion

    #region Publics

    /// <summary>
    /// A complete description gives its agent type, whether the run is finished, and the model asked
    /// </summary>
    [Fact]
    public void CompleteDescriptionIsRead()
    {
        // Parsing a description with every key
        SubagentMeta? meta = Parse("{\"agentType\":\"csharp-reviewer\",\"description\":\"invented task\",\"toolUseId\":\"toolu_1\",\"spawnDepth\":1,\"finished\":true,\"model\":\"sonnet\"}", out List<ReadProblem> problems);

        // Checking the description
        meta.ShouldBe(new SubagentMeta("csharp-reviewer", true, "sonnet"));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Without finished and model (a run in progress, or launched with the defaults), the description is still read
    /// </summary>
    [Fact]
    public void FinishedAndModelAreOptional()
    {
        // Parsing a description with the agent type only
        SubagentMeta? meta = Parse("{\"agentType\":\"Explore\"}", out List<ReadProblem> problems);

        // Checking the description
        meta.ShouldBe(new SubagentMeta("Explore", false, null));
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A finished or a model that is not of the expected kind is left out and reported; the description is still read
    /// </summary>
    /// <param name="text">Text of the invented file</param>
    /// <param name="key">Key reported</param>
    [Theory]
    [InlineData("{\"agentType\":\"Explore\",\"finished\":\"yes\"}", "finished")]
    [InlineData("{\"agentType\":\"Explore\",\"model\":12}", "model")]
    public void InvalidOptionalValueIsReported(string text, string key)
    {
        // Parsing the description
        SubagentMeta? meta = Parse(text, out List<ReadProblem> problems);

        // Checking that the description is read without the value, and that the value is reported
        meta.ShouldBe(new SubagentMeta("Explore", false, null));
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain($"key: {key}");
    }

    /// <summary>
    /// A description without a usable agent type cannot place the run at a desk: it is refused and reported
    /// </summary>
    /// <param name="text">Text without a usable agent type</param>
    [Theory]
    [InlineData("{\"description\":\"invented task\"}")]
    [InlineData("{\"agentType\":\"\"}")]
    [InlineData("{\"agentType\":\"   \"}")]
    [InlineData("{\"agentType\":12}")]
    [InlineData("{\"agentType\":\"\\ud800\"}")]
    public void DescriptionWithoutAnAgentTypeIsRefused(string text)
    {
        // Parsing the description
        SubagentMeta? meta = Parse(text, out List<ReadProblem> problems);

        // Checking that it is refused and reported
        meta.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(FilePath);
        problems[0].Reason.ShouldContain("key: agentType");
    }

    /// <summary>
    /// A text that is not a JSON object is refused and reported
    /// </summary>
    /// <param name="text">Text that is not a JSON object</param>
    [Theory]
    [InlineData("")]
    [InlineData("{ \"agentType\": ")]
    [InlineData("[\"Explore\"]")]
    public void TextThatIsNotAJsonObjectIsRefused(string text)
    {
        // Parsing the text
        SubagentMeta? meta = Parse(text, out List<ReadProblem> problems);

        // Checking that it is refused and reported
        meta.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(FilePath);
    }

    /// <summary>
    /// The arguments of the parser are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Calling the parser without each argument
        ArgumentNullException withoutPath = Should.Throw<ArgumentNullException>(() => SubagentMetaParser.Parse(" ", "{}", new List<ReadProblem>()));
        ArgumentNullException withoutText = Should.Throw<ArgumentNullException>(() => SubagentMetaParser.Parse(FilePath, null!, new List<ReadProblem>()));
        ArgumentNullException withoutProblems = Should.Throw<ArgumentNullException>(() => SubagentMetaParser.Parse(FilePath, "{}", null!));

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
