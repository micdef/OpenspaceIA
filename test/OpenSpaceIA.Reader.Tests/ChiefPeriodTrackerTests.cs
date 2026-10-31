/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ChiefPeriodTrackerTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the periods of work and of waiting of a chief, found in its transcript
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
/// Tests of the periods of work and of waiting of a chief, found in its transcript line by line
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the decisions of Michaël of 2026-10-31: a period of work is a turn (from a message of the
/// user to the end of the turn) minus the waits, and a wait is the time from a tool call to its result when the user
/// decided.</para>
/// <para><b>Behavior:</b> invented lines only, shaped like the records measured; no file, no state shared between tests.</para>
/// </remarks>
public sealed class ChiefPeriodTrackerTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Invented working folder, full on every system
    /// </summary>
    private static readonly string _workingFolder = Path.Combine(Path.GetTempPath(), "invented", "project");

    /// <summary>
    /// Invented project of the working folder
    /// </summary>
    private static readonly Project _project = new Project(_workingFolder);

    /// <summary>
    /// Start of the window of the tests: before every invented record
    /// </summary>
    private static readonly DateTimeOffset _since = new DateTimeOffset(2026, 10, 31, 0, 0, 0, TimeSpan.Zero);

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
    /// Method used to build a time of the day of the tests
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <returns>The time, in UTC</returns>
    private static DateTimeOffset At(int minutes)
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero).AddMinutes(minutes);
    }

    /// <summary>
    /// Method used to write the text of a time of the day of the tests
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <returns>The time, ISO 8601</returns>
    private static string Time(int minutes)
    {
        // Function's return
        return At(minutes).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Method used to write a message of the user, as a line
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <param name="text">[Cannot be null] Text of the message, escaped for JSON</param>
    /// <returns>The line</returns>
    private static string Prompt(int minutes, string text = "invented prompt")
    {
        // Function's return
        return $"{{\"type\":\"user\",\"timestamp\":\"{Time(minutes)}\",\"cwd\":\"{_workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal)}\",\"message\":{{\"role\":\"user\",\"content\":\"{text}\"}}}}";
    }

    /// <summary>
    /// Method used to write a record of the assistant, as a line
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <param name="stopReason">[Cannot be null] Why the message stops: end_turn or tool_use</param>
    /// <param name="toolId">[Can be null] Identifier of the tool called, or null for none</param>
    /// <param name="toolName">[Cannot be null] Name of the tool called</param>
    /// <returns>The line</returns>
    private static string Assistant(int minutes, string stopReason, string? toolId = null, string toolName = "Read")
    {
        // Building the content: a text, then a tool call when one is given
        string content = toolId is null
            ? "[{\"type\":\"text\",\"text\":\"invented\"}]"
            : $"[{{\"type\":\"tool_use\",\"id\":\"{toolId}\",\"name\":\"{toolName}\",\"input\":{{}}}}]";

        // Function's return
        return $"{{\"type\":\"assistant\",\"timestamp\":\"{Time(minutes)}\",\"message\":{{\"stop_reason\":\"{stopReason}\",\"content\":{content}}}}}";
    }

    /// <summary>
    /// Method used to write the result of a tool, as a line
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <param name="toolId">[Cannot be null] Identifier of the call answered</param>
    /// <param name="source">[Cannot be null] Source of the decision of permission</param>
    /// <param name="denial">[Can be null] Kind of refusal, or null when the call was accepted</param>
    /// <returns>The line</returns>
    private static string Result(int minutes, string toolId, string source = "config", string? denial = null)
    {
        // Building the refusal, when there is one
        string refusal = denial is null ? string.Empty : $",\"toolDenialKind\":\"{denial}\"";

        // Function's return
        return $"{{\"type\":\"user\",\"timestamp\":\"{Time(minutes)}\",\"permissionDecision\":{{\"decision\":\"accept\",\"source\":\"{source}\"}}{refusal},\"message\":{{\"content\":[{{\"type\":\"tool_result\",\"tool_use_id\":\"{toolId}\",\"content\":\"invented\"}}]}}}}";
    }

    /// <summary>
    /// Method used to follow lines
    /// </summary>
    /// <param name="lines">[Cannot be null] Lines of the invented transcript</param>
    /// <returns>The tracker</returns>
    private static ChiefPeriodTracker Follow(params string[] lines)
    {
        // Adding each line
        ChiefPeriodTracker tracker = new ChiefPeriodTracker();
        foreach (string line in lines)
            tracker.Add(line);

        // Function's return
        return tracker;
    }

    /// <summary>
    /// Method used to build a period of the invented project
    /// </summary>
    /// <param name="kind">Kind of the period</param>
    /// <param name="start">Minutes after noon of its start</param>
    /// <param name="end">Minutes after noon of its end</param>
    /// <returns>The period</returns>
    private static ActivityPeriod Period(PeriodKind kind, int start, int end)
    {
        // Function's return
        return new ActivityPeriod(_project, kind, At(start), At(end));
    }

    #endregion

    #region Publics

    /// <summary>
    /// A turn, from a message of the user to the end of the turn, is a period of work on the project of the message
    /// </summary>
    [Fact]
    public void TurnIsAPeriodOfWork()
    {
        // Following a turn with automatic tool calls
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(1, "tool_use", "t1"), Result(2, "t1"), Assistant(5, "end_turn"));

        // Checking the period
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 5) });
    }

    /// <summary>
    /// A tool call decided by the user is a wait inside the turn, from the call to its result
    /// </summary>
    /// <param name="source">Source of the decision</param>
    /// <param name="denial">[Can be null] Kind of refusal</param>
    [Theory]
    [InlineData("user_temporary", null)]
    [InlineData("config", "user-rejected")]
    public void DecisionOfTheUserIsAWait(string source, string? denial)
    {
        // Following a turn with a call that waits for the user from minute 2 to minute 6
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(2, "tool_use", "t1"), Result(6, "t1", source, denial), Assistant(9, "end_turn"));

        // Checking the periods
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>()
        {
            Period(PeriodKind.Working, 0, 2),
            Period(PeriodKind.Waiting, 2, 6),
            Period(PeriodKind.Working, 6, 9),
        });
    }

    /// <summary>
    /// A decision of a saved rule, of the configuration, or another refusal is not a wait
    /// </summary>
    /// <param name="source">Source of the decision</param>
    /// <param name="denial">[Can be null] Kind of refusal</param>
    [Theory]
    [InlineData("user_permanent", null)]
    [InlineData("config", null)]
    [InlineData("config", "permission-rule")]
    [InlineData("config", "interrupted")]
    public void OtherDecisionIsNotAWait(string source, string? denial)
    {
        // Following a turn with a call decided without the user
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(2, "tool_use", "t1"), Result(6, "t1", source, denial), Assistant(9, "end_turn"));

        // Checking the period
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 9) });
    }

    /// <summary>
    /// A wait that starts the turn, or ends it, leaves no period of work of zero length
    /// </summary>
    [Fact]
    public void NoPeriodOfZeroLength()
    {
        // Following a turn whose call waits from its very start to its very end
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(0, "tool_use", "t1"), Result(4, "t1", "user_temporary"), Assistant(4, "end_turn"));

        // Checking the period
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Waiting, 0, 4) });
    }

    /// <summary>
    /// The time between two turns is neither work nor wait
    /// </summary>
    [Fact]
    public void TimeBetweenTurnsIsNotAPeriod()
    {
        // Following two turns, one hour apart
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(3, "end_turn"), Prompt(60), Assistant(62, "end_turn"));

        // Checking the periods
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 3), Period(PeriodKind.Working, 60, 62) });
    }

    /// <summary>
    /// A message of the user inside a turn (typed while Claude works) continues the turn
    /// </summary>
    [Fact]
    public void MessageInsideATurnContinuesIt()
    {
        // Following a turn with a second message in its middle
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(1, "tool_use", "t1"), Prompt(2, "invented follow-up"), Result(3, "t1"), Assistant(5, "end_turn"));

        // Checking the period
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 5) });
    }

    /// <summary>
    /// An interruption by the user ends the turn at its time; the next message starts a new turn
    /// </summary>
    /// <param name="marker">Marker of the interruption written by Claude Code</param>
    [Theory]
    [InlineData("[Request interrupted by user]")]
    [InlineData("[Request interrupted by user for tool use]")]
    public void InterruptionEndsTheTurn(string marker)
    {
        // Following a turn interrupted at minute 4, then a new turn
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(1, "tool_use", "t1"), Prompt(4, marker), Prompt(10), Assistant(12, "end_turn"));

        // Checking the periods
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 4), Period(PeriodKind.Working, 10, 12) });
    }

    /// <summary>
    /// A turn not ended yet is a period up to its last record
    /// </summary>
    [Fact]
    public void TurnNotEndedLastsUpToItsLastRecord()
    {
        // Following a turn still running, with a wait already answered
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(1, "tool_use", "t1"), Result(3, "t1", "user_temporary"), Assistant(7, "tool_use", "t2"));

        // Checking the periods
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>()
        {
            Period(PeriodKind.Working, 0, 1),
            Period(PeriodKind.Waiting, 1, 3),
            Period(PeriodKind.Working, 3, 7),
        });
    }

    /// <summary>
    /// The messages that Claude Code adds itself (isMeta) do not start a turn
    /// </summary>
    [Fact]
    public void MetaMessageDoesNotStartATurn()
    {
        // Following a message of Claude Code alone
        ChiefPeriodTracker tracker = Follow(Prompt(0).Replace("\"type\":\"user\"", "\"type\":\"user\",\"isMeta\":true", StringComparison.Ordinal), Assistant(3, "end_turn"));

        // Checking that there is no period
        tracker.GetPeriods(_since).ShouldBeEmpty();
    }

    /// <summary>
    /// The periods that end before the start of the window are left out, and forgotten
    /// </summary>
    [Fact]
    public void PeriodsBeforeTheWindowAreLeftOut()
    {
        // Following two turns, then asking from minute 30
        ChiefPeriodTracker tracker = Follow(Prompt(0), Assistant(3, "end_turn"), Prompt(60), Assistant(62, "end_turn"));
        IReadOnlyList<ActivityPeriod> periods = tracker.GetPeriods(At(30));

        // Checking that only the second turn is kept, even when asked again from earlier
        periods.ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 60, 62) });
        tracker.GetPeriods(_since).Count.ShouldBe(1);
    }

    /// <summary>
    /// A turn whose project cannot be known (no working folder) is left out and counted
    /// </summary>
    [Fact]
    public void TurnWithoutProjectIsCounted()
    {
        // Following a turn without working folder
        ChiefPeriodTracker tracker = Follow("{\"type\":\"user\",\"timestamp\":\"" + Time(0) + "\",\"message\":{\"content\":\"invented\"}}", Assistant(3, "end_turn"));

        // Checking the result
        tracker.GetPeriods(_since).ShouldBeEmpty();
        tracker.TurnWithoutProjectCount.ShouldBe(1);
    }

    /// <summary>
    /// A record whose time cannot be read, or is in the future, is counted and changes nothing
    /// </summary>
    [Fact]
    public void RecordWithoutAUsableTimeIsCounted()
    {
        // Following a turn whose end has no offset, then an end in the future, then the true end
        ChiefPeriodTracker tracker = new ChiefPeriodTracker() { LatestAcceptedUtc = At(30) };
        tracker.Add(Prompt(0));
        tracker.Add(Assistant(5, "end_turn").Replace("Z\"", "\"", StringComparison.Ordinal));
        tracker.Add(Assistant(31, "end_turn"));
        tracker.Add(Assistant(8, "end_turn"));

        // Checking the period and the count
        tracker.GetPeriods(_since).ShouldBe(new List<ActivityPeriod>() { Period(PeriodKind.Working, 0, 8) });
        tracker.UnreadableLineCount.ShouldBe(2);
    }

    /// <summary>
    /// A line that is not a JSON object is counted; a blank line is not
    /// </summary>
    [Fact]
    public void BadLinesAreCounted()
    {
        // Following bad and blank lines
        ChiefPeriodTracker tracker = Follow("{ bad", "[1]", string.Empty, "  ");

        // Checking the count
        tracker.UnreadableLineCount.ShouldBe(2);
    }

    /// <summary>
    /// The latest time accepted must be in UTC, and a line is required
    /// </summary>
    [Fact]
    public void ArgumentsAreChecked()
    {
        // Setting a time with an offset, then adding a null line
        ArgumentOutOfRangeException offset = Should.Throw<ArgumentOutOfRangeException>(
            () => new ChiefPeriodTracker() { LatestAcceptedUtc = new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.FromHours(1)) });
        ArgumentNullException line = Should.Throw<ArgumentNullException>(() => new ChiefPeriodTracker().Add(null!));

        // Checking the parameters in fault
        offset.ParamName.ShouldBe("value");
        line.ParamName.ShouldBe("line");
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
