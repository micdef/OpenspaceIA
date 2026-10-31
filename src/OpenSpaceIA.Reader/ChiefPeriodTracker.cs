/*
 *	@file	src/OpenSpaceIA.Reader/ChiefPeriodTracker.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Periods of work and of waiting of a chief, found in its transcript line by line
 */

// Using
using System.Text.Json;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Periods of work and of waiting of a chief, found in its transcript line by line
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> "overloaded" and "under-used" need the periods of the chief over a window
/// (doc/analyse/state-definitions.md, § 5); they are rebuilt from the transcript, with no storage of our own.</para>
/// <para><b>Behavior:</b> decisions of Michaël, 2026-10-31. A <b>turn</b> starts with a message of the user (a record of the
/// user that is not added by Claude Code, <c>isMeta</c>, nor a summary of a compaction, nor the output of a local command,
/// and that holds no tool result), and ends with a final answer of the assistant (<c>stop_reason</c> <c>end_turn</c>,
/// <c>stop_sequence</c>, <c>refusal</c> or <c>max_tokens</c>), or with an interruption by the user (a message that starts
/// with <c>[Request interrupted by user</c>), at its time; a message of the user inside a turn (typed while Claude works)
/// continues it. A silence of more than 30 minutes inside a turn, without a tool call in progress, ends it at its last
/// record (a session closed in the middle of a turn). A turn not ended yet lasts up to its last record. The records of a
/// sub-agent (<c>isSidechain</c>) are left out. A <b>wait</b> is the time from a tool call to its result when the user
/// decided (the source of the decision is <c>user_temporary</c>, or the refusal is <c>user-rejected</c>); it holds the run of
/// the tool after the decision, and a wait whose result comes after the end of its turn is lost. A turn gives periods of
/// waiting for its waits and periods of work for the rest; a period of zero
/// length is left out. The project of a turn is the last working folder (<c>cwd</c>) given up to its start, or the first one
/// given in it; a turn without any is left out and counted. A record whose time cannot be read (see
/// <see cref="TranscriptTime"/>) is counted and changes nothing; a line that is not a JSON object is counted; a blank line is
/// left out. The periods that end before the start of the window asked are forgotten. A line is never kept.</para>
/// </remarks>
internal sealed class ChiefPeriodTracker : ITranscriptFollower
{
    #region Members

    #region Constants

    /// <summary>
    /// Start of the message that Claude Code writes when the user interrupts a turn
    /// </summary>
    private const string InterruptionMarker = "[Request interrupted by user";

    #endregion

    #region Variables

    /// <summary>
    /// Reasons that end the answer of the assistant, so the turn (every reason but a tool call)
    /// </summary>
    private static readonly HashSet<string> _finalStopReasons = new HashSet<string>(StringComparer.Ordinal) { "end_turn", "stop_sequence", "refusal", "max_tokens" };

    /// <summary>
    /// Starts of the messages that a local command writes, which Claude does not answer
    /// </summary>
    private static readonly string[] _localCommandMarkers = new string[] { "<command-name>", "<command-message>", "<command-args>", "<local-command-" };

    /// <summary>
    /// Longest silence inside a turn without a tool call in progress; a longer one ends the turn at its last record
    /// (decision of Michaël, 2026-10-31; the longest tool call measured in a turn lasted 17 minutes)
    /// </summary>
    private static readonly TimeSpan _silenceLimit = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Periods of the turns already ended, in their order
    /// </summary>
    private readonly List<ActivityPeriod> _endedPeriods = new List<ActivityPeriod>();

    /// <summary>
    /// Time of each tool call whose result is not read yet, by identifier of the call
    /// </summary>
    private readonly Dictionary<string, DateTimeOffset> _openCallTimes = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);

    /// <summary>
    /// Waits of the turn in progress
    /// </summary>
    private readonly List<(DateTimeOffset Start, DateTimeOffset End)> _turnWaits = new List<(DateTimeOffset Start, DateTimeOffset End)>();

    /// <summary>
    /// Latest time accepted for a record, in UTC
    /// </summary>
    private DateTimeOffset _latestAcceptedUtc = DateTimeOffset.MaxValue;

    /// <summary>
    /// Start of the turn in progress; null when no turn is in progress
    /// </summary>
    private DateTimeOffset? _turnStart;

    /// <summary>
    /// Last record of the turn in progress
    /// </summary>
    private DateTimeOffset _turnLastRecord;

    /// <summary>
    /// Working folder of the turn in progress; null while none is known
    /// </summary>
    private string? _turnWorkingFolder;

    /// <summary>
    /// Last working folder given by a record
    /// </summary>
    private string? _lastWorkingFolder;

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

    /// <summary>
    /// Latest time accepted for a record (now, with a small margin), in UTC; no limit by default
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If the time is not in UTC</exception>
    public DateTimeOffset LatestAcceptedUtc
    {
        get => _latestAcceptedUtc;
        set
        {
            // Check
            if (value.Offset != TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"The value of the property: {nameof(LatestAcceptedUtc)} must be in UTC");

            // Setting the value
            _latestAcceptedUtc = value;
        }
    }

    /// <summary>
    /// Number of lines that could not be read, or whose time could not be read
    /// </summary>
    public int UnreadableLineCount { get; private set; }

    /// <summary>
    /// Number of turns left out because their project is unknown
    /// </summary>
    public int TurnWithoutProjectCount { get; private set; }

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
    /// Method used to read a text of an element
    /// </summary>
    /// <param name="element">JSON object</param>
    /// <param name="key">[Cannot be null] Key of the text</param>
    /// <returns>[Can be null] The text; null when the key is missing or is not a text</returns>
    /// <exception cref="InvalidOperationException">If the text holds an escaped half of a character (caught by the caller)</exception>
    private static string? ReadText(JsonElement element, string key)
    {
        // Function's return
        return element.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    /// <summary>
    /// Method used to know whether a key of an element is true
    /// </summary>
    /// <param name="element">JSON object</param>
    /// <param name="key">[Cannot be null] Key of the boolean</param>
    /// <returns>
    /// True  ==> The key is the boolean true<br/>
    /// False ==> The key is missing, false, or of another kind
    /// </returns>
    private static bool IsTrue(JsonElement element, string key)
    {
        // Function's return
        return element.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.True;
    }

    /// <summary>
    /// Method used to cut a turn into periods of work and of waiting
    /// </summary>
    /// <param name="project">[Cannot be null] Project of the turn</param>
    /// <param name="start">Start of the turn</param>
    /// <param name="end">End of the turn, not before its start</param>
    /// <param name="waits">[Cannot be null] Waits of the turn</param>
    /// <returns>The periods of the turn, in their order, none of zero length</returns>
    private static List<ActivityPeriod> CutTurn(Project project, DateTimeOffset start, DateTimeOffset end, List<(DateTimeOffset Start, DateTimeOffset End)> waits)
    {
        // Walking the waits in their order, kept inside the turn
        List<ActivityPeriod> periods = new List<ActivityPeriod>();
        DateTimeOffset cursor = start;
        foreach ((DateTimeOffset waitStart, DateTimeOffset waitEnd) in waits.OrderBy(wait => wait.Start))
        {
            DateTimeOffset from = waitStart < cursor ? cursor : waitStart;
            DateTimeOffset to = waitEnd > end ? end : waitEnd;
            if (to <= from)
                continue;

            if (from > cursor)
                periods.Add(new ActivityPeriod(project, PeriodKind.Working, cursor, from));
            periods.Add(new ActivityPeriod(project, PeriodKind.Waiting, from, to));
            cursor = to;
        }

        // Adding the work after the last wait
        if (end > cursor)
            periods.Add(new ActivityPeriod(project, PeriodKind.Working, cursor, end));

        // Function's return
        return periods;
    }

    /// <summary>
    /// Method used to end the turn in progress at a time
    /// </summary>
    /// <param name="end">End of the turn</param>
    private void EndTurn(DateTimeOffset end)
    {
        // Keeping the periods of the turn, or counting a turn without project
        if (_turnStart is DateTimeOffset start)
        {
            if (_turnWorkingFolder is null)
                TurnWithoutProjectCount++;
            else
                _endedPeriods.AddRange(CutTurn(new Project(_turnWorkingFolder), start, end < start ? start : end, _turnWaits));
        }

        // Forgetting the turn
        _turnStart = null;
        _turnWorkingFolder = null;
        _turnWaits.Clear();
        _openCallTimes.Clear();
    }

    /// <summary>
    /// Method used to read the text of a message of the user, when it is not a result of tools
    /// </summary>
    /// <param name="record">Record of the user</param>
    /// <returns>[Can be null] The text (empty when the message has no text item); null when the message holds a tool result</returns>
    /// <exception cref="InvalidOperationException">If a text holds an escaped half of a character (caught by the caller)</exception>
    private static string? ReadMessageText(JsonElement record)
    {
        // Reading a text content, or the items of the content
        if (!record.TryGetProperty("message", out JsonElement message) || message.ValueKind != JsonValueKind.Object
            || !message.TryGetProperty("content", out JsonElement content))
        {
            // Function's return
            return string.Empty;
        }
        if (content.ValueKind == JsonValueKind.String)
        {
            // Function's return
            return content.GetString();
        }

        // Refusing a message that holds a tool result, and keeping the first text
        string text = string.Empty;
        if (content.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object))
            {
                string? type = ReadText(item, "type");
                if (type == "tool_result")
                {
                    // Function's return
                    return null;
                }
                if (type == "text" && text.Length == 0)
                    text = ReadText(item, "text") ?? string.Empty;
            }
        }

        // Function's return
        return text;
    }

    /// <summary>
    /// Method used to apply a record of the user to the turn
    /// </summary>
    /// <param name="record">Record of the user</param>
    /// <param name="time">Time of the record</param>
    /// <exception cref="InvalidOperationException">If a text holds an escaped half of a character (caught by the caller)</exception>
    private void ApplyUserRecord(JsonElement record, DateTimeOffset time)
    {
        // Leaving out the messages that Claude Code adds itself, and the summaries of a compaction
        if (IsTrue(record, "isMeta") || IsTrue(record, "isCompactSummary"))
            return;

        // Closing the calls answered, and keeping the waits decided by the user
        string? text = ReadMessageText(record);
        if (text is null)
        {
            string? source = record.TryGetProperty("permissionDecision", out JsonElement decision) && decision.ValueKind == JsonValueKind.Object ? ReadText(decision, "source") : null;
            bool isDecidedByUser = source == "user_temporary" || ReadText(record, "toolDenialKind") == "user-rejected";
            JsonElement content = record.GetProperty("message").GetProperty("content");
            foreach (JsonElement item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object && ReadText(item, "type") == "tool_result"))
            {
                string? id = ReadText(item, "tool_use_id");
                if (id is not null && _openCallTimes.Remove(id, out DateTimeOffset callTime) && isDecidedByUser && _turnStart is not null)
                    _turnWaits.Add((callTime, time));
            }
            return;
        }

        // Ending the turn at an interruption, leaving out a local command, or starting a turn at a message
        if (text.StartsWith(InterruptionMarker, StringComparison.Ordinal))
        {
            if (_turnStart is not null)
                EndTurn(time);
        }
        else if (_localCommandMarkers.Any(marker => text.StartsWith(marker, StringComparison.Ordinal)))
        {
            return;
        }
        else if (_turnStart is null)
        {
            _turnStart = time;
            _turnLastRecord = time;
            _turnWorkingFolder = _lastWorkingFolder;
        }
    }

    /// <summary>
    /// Method used to apply a record of the assistant to the turn
    /// </summary>
    /// <param name="record">Record of the assistant</param>
    /// <param name="time">Time of the record</param>
    /// <exception cref="InvalidOperationException">If a text holds an escaped half of a character (caught by the caller)</exception>
    private void ApplyAssistantRecord(JsonElement record, DateTimeOffset time)
    {
        // Keeping the time of each tool call of a turn
        if (!record.TryGetProperty("message", out JsonElement message) || message.ValueKind != JsonValueKind.Object)
            return;
        if (_turnStart is not null && message.TryGetProperty("content", out JsonElement content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object && ReadText(item, "type") == "tool_use"))
            {
                if (ReadText(item, "id") is string id)
                    _openCallTimes[id] = time;
            }
        }

        // Ending the turn at the end of the answer
        if (_turnStart is not null && ReadText(message, "stop_reason") is string stopReason && _finalStopReasons.Contains(stopReason))
            EndTurn(time);
    }

    #endregion

    #region Publics

    /// <inheritdoc/>
    public void Add(string line)
    {
        // Check
        if (line is null)
            throw new ArgumentNullException(nameof(line), $"The value of the parameter: {nameof(line)} cannot be null");

        // Leaving out a blank line
        if (string.IsNullOrWhiteSpace(line))
            return;

        try
        {
            // Reading the line as one JSON object of the user or of the assistant
            using JsonDocument document = JsonDocument.Parse(line);
            JsonElement record = document.RootElement;
            if (record.ValueKind != JsonValueKind.Object)
            {
                UnreadableLineCount++;
                return;
            }
            string? type = ReadText(record, "type");
            if ((type != "user" && type != "assistant") || IsTrue(record, "isSidechain"))
                return;

            // Reading the time, without which the record cannot be placed
            if (!TranscriptTime.TryRead(ReadText(record, "timestamp"), _latestAcceptedUtc, out DateTimeOffset time))
            {
                UnreadableLineCount++;
                return;
            }

            // Ending a turn silent for too long without a tool call in progress, at its last record
            if (_turnStart is not null && _openCallTimes.Count == 0 && time - _turnLastRecord > _silenceLimit)
                EndTurn(_turnLastRecord);

            // Keeping the working folder, then applying the record to the turn
            string? workingFolder = ReadText(record, "cwd");
            if (!string.IsNullOrWhiteSpace(workingFolder) && Path.IsPathFullyQualified(workingFolder))
            {
                _lastWorkingFolder = workingFolder;
                if (_turnStart is not null)
                    _turnWorkingFolder ??= workingFolder;
            }
            if (_turnStart is not null && time > _turnLastRecord)
                _turnLastRecord = time;
            if (type == "user")
                ApplyUserRecord(record, time);
            else
                ApplyAssistantRecord(record, time);
        }
        catch (JsonException)
        {
            UnreadableLineCount++;
        }
        catch (InvalidOperationException)
        {
            // A text with an escaped half of a character
            UnreadableLineCount++;
        }
    }

    /// <summary>
    /// Method used to get the periods of the chief that end at or after the start of a window
    /// </summary>
    /// <param name="sinceUtc">Start of the window</param>
    /// <returns>The periods of the turns ended, then those of the turn in progress up to its last record, in their order</returns>
    public IReadOnlyList<ActivityPeriod> GetPeriods(DateTimeOffset sinceUtc)
    {
        // Forgetting the periods that end before the window
        _endedPeriods.RemoveAll(period => period.EndUtc < sinceUtc);

        // Adding the periods of the turn in progress
        List<ActivityPeriod> periods = new List<ActivityPeriod>(_endedPeriods);
        if (_turnStart is DateTimeOffset start && _turnWorkingFolder is not null)
            periods.AddRange(CutTurn(new Project(_turnWorkingFolder), start, _turnLastRecord, _turnWaits).Where(period => period.EndUtc >= sinceUtc));

        // Function's return
        return periods.AsReadOnly();
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
