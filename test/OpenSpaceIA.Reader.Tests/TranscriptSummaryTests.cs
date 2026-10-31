/*
 *	@file	test/OpenSpaceIA.Reader.Tests/TranscriptSummaryTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the summary of a transcript, built line by line
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
/// Tests of the summary of a transcript, built line by line
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves which facts a transcript gives (times, working folder, model, last tool), and that a line
/// that cannot be read is counted instead of stopping the reading.</para>
/// <para><b>Behavior:</b> invented lines only, shaped like the records measured on 2026-10-31; no file, no state shared
/// between tests.</para>
/// </remarks>
public sealed class TranscriptSummaryTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Invented working folder, full on every system, escaped for JSON
    /// </summary>
    private static readonly string _workingFolder = Path.Combine(Path.GetTempPath(), "invented", "project");

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
    /// Method used to write a record of a user, as a line
    /// </summary>
    /// <param name="timestamp">[Cannot be null] Time of the record, ISO 8601</param>
    /// <returns>The line</returns>
    private static string User(string timestamp)
    {
        // Function's return
        return $"{{\"type\":\"user\",\"timestamp\":\"{timestamp}\",\"cwd\":\"{_workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal)}\",\"isSidechain\":true,\"message\":{{\"role\":\"user\",\"content\":\"invented task\"}}}}";
    }

    /// <summary>
    /// Method used to write a record of the assistant, as a line
    /// </summary>
    /// <param name="timestamp">[Cannot be null] Time of the record, ISO 8601</param>
    /// <param name="model">[Cannot be null] Model of the message</param>
    /// <param name="toolName">[Can be null] Name of the tool called, or null for a text only</param>
    /// <returns>The line</returns>
    private static string Assistant(string timestamp, string model, string? toolName)
    {
        // Building the content: a text, then a tool call when one is given
        string content = toolName is null
            ? "[{\"type\":\"text\",\"text\":\"invented\"}]"
            : $"[{{\"type\":\"text\",\"text\":\"invented\"}},{{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"{toolName}\",\"input\":{{}}}}]";

        // Function's return
        return $"{{\"type\":\"assistant\",\"timestamp\":\"{timestamp}\",\"message\":{{\"model\":\"{model}\",\"content\":{content}}}}}";
    }

    /// <summary>
    /// Method used to summarize lines
    /// </summary>
    /// <param name="lines">[Cannot be null] Lines of the invented transcript</param>
    /// <returns>The summary</returns>
    private static TranscriptSummary Summarize(params string[] lines)
    {
        // Adding each line
        TranscriptSummary summary = new TranscriptSummary();
        foreach (string line in lines)
            summary.Add(line);

        // Function's return
        return summary;
    }

    #endregion

    #region Publics

    /// <summary>
    /// The first and the last times, the working folder, the last model and the last tool are kept
    /// </summary>
    [Fact]
    public void FactsOfTheTranscriptAreKept()
    {
        // Summarizing a run that ends with its hand back
        TranscriptSummary summary = Summarize(
            User("2026-10-31T12:14:34.000Z"),
            Assistant("2026-10-31T12:15:00.000Z", "claude-haiku-5-5", "Read"),
            "{\"type\":\"api-request\"}",
            Assistant("2026-10-31T12:17:59.500Z", "claude-sonnet-5-5", "SubagentHandback"),
            User("2026-10-31T12:17:59.600Z"));

        // Checking the facts
        summary.FirstRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 12, 14, 34, TimeSpan.Zero));
        summary.LastRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 12, 17, 59, 600, TimeSpan.Zero));
        summary.WorkingFolder.ShouldBe(_workingFolder);
        summary.Model.ShouldBe("claude-sonnet-5-5");
        summary.LastToolName.ShouldBe("SubagentHandback");
        summary.UnreadableLineCount.ShouldBe(0);
    }

    /// <summary>
    /// The times are the earliest and the latest, even when the records are not in order, and an offset is turned into UTC
    /// </summary>
    [Fact]
    public void TimesAreTheEarliestAndTheLatestInUtc()
    {
        // Summarizing records out of order, one with an offset of two hours
        TranscriptSummary summary = Summarize(
            User("2026-10-31T12:00:00.000Z"),
            User("2026-10-31T11:00:00.000Z"),
            User("2026-10-31T15:30:00.000+02:00"));

        // Checking the times
        summary.FirstRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 11, 0, 0, TimeSpan.Zero));
        summary.LastRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 13, 30, 0, TimeSpan.Zero));
        summary.FirstRecordAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
        summary.LastRecordAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// The last tool is the one of the last record of the assistant that calls a tool; a text after it does not clear it
    /// </summary>
    [Fact]
    public void LastToolIsTheLastToolCalled()
    {
        // Summarizing two tool calls, then a text
        TranscriptSummary summary = Summarize(
            Assistant("2026-10-31T12:00:00.000Z", "claude-sonnet-5-5", "SubagentHandback"),
            Assistant("2026-10-31T12:01:00.000Z", "claude-sonnet-5-5", "Read"),
            Assistant("2026-10-31T12:02:00.000Z", "claude-sonnet-5-5", null));

        // Checking the last tool
        summary.LastToolName.ShouldBe("Read");
    }

    /// <summary>
    /// A transcript without records gives no fact
    /// </summary>
    [Fact]
    public void EmptyTranscriptGivesNoFact()
    {
        // Summarizing nothing
        TranscriptSummary summary = Summarize();

        // Checking that nothing is known
        summary.FirstRecordAtUtc.ShouldBeNull();
        summary.LastRecordAtUtc.ShouldBeNull();
        summary.WorkingFolder.ShouldBeNull();
        summary.Model.ShouldBeNull();
        summary.LastToolName.ShouldBeNull();
        summary.UnreadableLineCount.ShouldBe(0);
    }

    /// <summary>
    /// A line that is not a JSON object, or whose time or text cannot be read, is counted and never stops the reading;
    /// a blank line is not counted
    /// </summary>
    [Fact]
    public void UnreadableLinesAreCountedAndSkipped()
    {
        // Summarizing good lines around bad ones
        TranscriptSummary summary = Summarize(
            User("2026-10-31T12:00:00.000Z"),
            "{ not json",
            "[1, 2]",
            "{\"type\":\"user\",\"timestamp\":\"not a time\"}",
            "{\"type\":\"user\",\"timestamp\":12}",
            "{\"type\":\"assistant\",\"timestamp\":\"2026-10-31T12:30:00Z\",\"message\":{\"model\":\"\\ud800\"}}",
            "{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:40:00\"}",
            string.Empty,
            "   ",
            User("2026-10-31T13:00:00.000Z"));

        // Checking the count and the facts of the good lines
        summary.UnreadableLineCount.ShouldBe(6);
        summary.FirstRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero));
        summary.LastRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 13, 0, 0, TimeSpan.Zero));
        summary.Model.ShouldBeNull();
    }

    /// <summary>
    /// A working folder that is not a full path is not kept; the first full one is
    /// </summary>
    [Fact]
    public void OnlyAFullWorkingFolderIsKept()
    {
        // Summarizing a relative folder, then a full one, then another full one
        TranscriptSummary summary = Summarize(
            "{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:00:00Z\",\"cwd\":\"relative\"}",
            User("2026-10-31T12:01:00.000Z"),
            "{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:02:00Z\",\"cwd\":\"" + Path.Combine(Path.GetTempPath(), "other").Replace("\\", "\\\\", StringComparison.Ordinal) + "\"}");

        // Checking the folder
        summary.WorkingFolder.ShouldBe(_workingFolder);
    }

    /// <summary>
    /// A line is required
    /// </summary>
    [Fact]
    public void LineCannotBeNull()
    {
        // Adding a null line
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new TranscriptSummary().Add(null!));

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
