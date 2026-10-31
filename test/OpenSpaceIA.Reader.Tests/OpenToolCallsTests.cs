/*
 *	@file	test/OpenSpaceIA.Reader.Tests/OpenToolCallsTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the tool calls left open in the transcript of a chief
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
/// Tests of the tool calls left open in the transcript of a chief, and of the reason of a wait that they give
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the reason of a wait decided by Michaël on 2026-10-31: an open question is a question, any
/// other open call is a permission, and no open call is a question (the status comes before the question in the
/// transcript).</para>
/// <para><b>Behavior:</b> invented lines only, shaped like the records measured; no file, no state shared between tests.</para>
/// </remarks>
public sealed class OpenToolCallsTests
{
    #region Members

    #region Constants

    // #N/A

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
    /// Method used to write a record of the assistant that calls tools, as a line
    /// </summary>
    /// <param name="calls">[Cannot be null] Identifier and name of each tool called</param>
    /// <returns>The line</returns>
    private static string Calls(params (string Id, string Name)[] calls)
    {
        // Building the content
        string content = string.Join(",", calls.Select(call => $"{{\"type\":\"tool_use\",\"id\":\"{call.Id}\",\"name\":\"{call.Name}\",\"input\":{{}}}}"));

        // Function's return
        return $"{{\"type\":\"assistant\",\"timestamp\":\"2026-10-31T12:00:00Z\",\"message\":{{\"content\":[{content}]}}}}";
    }

    /// <summary>
    /// Method used to write a record of the user that gives the results of tools, as a line
    /// </summary>
    /// <param name="ids">[Cannot be null] Identifiers of the tool calls answered</param>
    /// <returns>The line</returns>
    private static string Results(params string[] ids)
    {
        // Building the content
        string content = string.Join(",", ids.Select(id => $"{{\"type\":\"tool_result\",\"tool_use_id\":\"{id}\",\"content\":\"invented\"}}"));

        // Function's return
        return $"{{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:00:01Z\",\"message\":{{\"content\":[{content}]}}}}";
    }

    /// <summary>
    /// Method used to follow lines
    /// </summary>
    /// <param name="lines">[Cannot be null] Lines of the invented transcript</param>
    /// <returns>The open tool calls</returns>
    private static OpenToolCalls Follow(params string[] lines)
    {
        // Adding each line
        OpenToolCalls calls = new OpenToolCalls();
        foreach (string line in lines)
            calls.Add(line);

        // Function's return
        return calls;
    }

    #endregion

    #region Publics

    /// <summary>
    /// An open question gives a question
    /// </summary>
    [Fact]
    public void OpenQuestionIsAQuestion()
    {
        // Following a question left open
        OpenToolCalls calls = Follow(Calls(("toolu_1", "AskUserQuestion")));

        // Checking the reason
        calls.GetWaitReason().ShouldBe(WaitReason.Question);
        calls.Count.ShouldBe(1);
    }

    /// <summary>
    /// An open approval of a plan is a question too (decision of Michaël, 2026-10-31), even with another call open
    /// </summary>
    [Fact]
    public void OpenApprovalOfAPlanIsAQuestion()
    {
        // Following an agent and an approval of a plan, both open
        OpenToolCalls calls = Follow(Calls(("toolu_1", "Agent")), Calls(("toolu_2", "ExitPlanMode")));

        // Checking the reason
        calls.GetWaitReason().ShouldBe(WaitReason.Question);
    }

    /// <summary>
    /// Another open tool call gives a permission
    /// </summary>
    [Fact]
    public void OtherOpenCallIsAPermission()
    {
        // Following a read left open
        OpenToolCalls calls = Follow(Calls(("toolu_1", "Read")));

        // Checking the reason
        calls.GetWaitReason().ShouldBe(WaitReason.Permission);
    }

    /// <summary>
    /// With an open question and another open call (an agent running, for example), the question wins
    /// </summary>
    [Fact]
    public void OpenQuestionWinsOverAnotherOpenCall()
    {
        // Following an agent and a question, both open
        OpenToolCalls calls = Follow(Calls(("toolu_1", "Agent")), Calls(("toolu_2", "AskUserQuestion")));

        // Checking the reason
        calls.GetWaitReason().ShouldBe(WaitReason.Question);
        calls.Count.ShouldBe(2);
    }

    /// <summary>
    /// A tool call whose result is written is closed; with no open call, the reason is a question
    /// </summary>
    [Fact]
    public void ClosedCallsGiveAQuestion()
    {
        // Following two calls in one record, then their results
        OpenToolCalls calls = Follow(Calls(("toolu_1", "Read"), ("toolu_2", "Bash")), Results("toolu_1"), Results("toolu_2"));

        // Checking that nothing is open
        calls.Count.ShouldBe(0);
        calls.GetWaitReason().ShouldBe(WaitReason.Question);
    }

    /// <summary>
    /// Only the call answered is closed: the others stay open
    /// </summary>
    [Fact]
    public void OnlyTheCallAnsweredIsClosed()
    {
        // Following a question and a read, then the answer to the question
        OpenToolCalls calls = Follow(Calls(("toolu_1", "AskUserQuestion"), ("toolu_2", "Read")), Results("toolu_1"));

        // Checking that the read is still open
        calls.Count.ShouldBe(1);
        calls.GetWaitReason().ShouldBe(WaitReason.Permission);
    }

    /// <summary>
    /// A call is opened only by a record of the assistant and closed only by a record of the user: the copies of the
    /// requests (api-request-blob) and the other kinds of record change nothing
    /// </summary>
    [Fact]
    public void OnlyTheAssistantOpensAndOnlyTheUserCloses()
    {
        // Following an open read, then records of other kinds that hold calls and results
        OpenToolCalls calls = Follow(
            Calls(("toolu_1", "Read")),
            "{\"type\":\"user\",\"message\":{\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_2\",\"name\":\"Bash\"}]}}",
            "{\"type\":\"assistant\",\"message\":{\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"toolu_1\"}]}}",
            "{\"type\":\"api-request-blob\",\"message\":{\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"toolu_1\"},{\"type\":\"tool_use\",\"id\":\"toolu_3\",\"name\":\"AskUserQuestion\"}]}}");

        // Checking that only the first read is open
        calls.Count.ShouldBe(1);
        calls.GetWaitReason().ShouldBe(WaitReason.Permission);
    }

    /// <summary>
    /// A blank line is left out without being counted
    /// </summary>
    [Fact]
    public void BlankLineIsNotCounted()
    {
        // Following blank lines
        OpenToolCalls calls = Follow(string.Empty, "   ", "\t");

        // Checking the count
        calls.UnreadableLineCount.ShouldBe(0);
    }

    /// <summary>
    /// A line that cannot be read is counted, and changes nothing
    /// </summary>
    [Fact]
    public void UnreadableLineIsCountedAndChangesNothing()
    {
        // Following an open read, then bad lines
        OpenToolCalls calls = Follow(
            Calls(("toolu_1", "Read")),
            "{ not json",
            "[1]",
            "{\"type\":\"user\",\"message\":{\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"\\ud800\"}]}}",
            string.Empty);

        // Checking the count and the open call
        calls.UnreadableLineCount.ShouldBe(3);
        calls.Count.ShouldBe(1);
    }

    /// <summary>
    /// A line is required
    /// </summary>
    [Fact]
    public void LineCannotBeNull()
    {
        // Adding a null line
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new OpenToolCalls().Add(null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("line");
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
