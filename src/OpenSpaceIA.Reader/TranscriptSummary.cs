/*
 *	@file	src/OpenSpaceIA.Reader/TranscriptSummary.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Summary of a transcript of Claude Code, built line by line
 */

// Using
using System.Globalization;
using System.Text.Json;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Summary of a transcript of Claude Code, built line by line: its times, its working folder, its model, its last tool
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a run needs only a few facts of its transcript (doc/analyse/spike-claude-files.md, § 3); the
/// summary keeps them while the lines go by, so that the transcript is never held in memory.</para>
/// <para><b>Behavior:</b> each line is one JSON record. The first and the last times are the earliest and the latest
/// <c>timestamp</c> of the records, in UTC; a time must be a full ISO 8601 date and time with its offset (or <c>Z</c>), and
/// not later than the latest time accepted (a time in the future is not true). The working folder is the first <c>cwd</c>
/// that is a full path for the system that reads it (Claude Code writes the folders of its own machine). The model is the
/// last <c>message.model</c> of a record of the assistant, except the placeholder <c>&lt;synthetic&gt;</c> of the messages
/// that Claude Code makes itself; the last tool is the name of the last item <c>tool_use</c> of a record of the assistant.
/// Each fact of a line is read on its own: a fact that cannot be read is lost and the line is counted, but the other facts
/// of the line are kept, so that a bad time never hides the hand back of a run. A line that is not a JSON object is counted
/// and gives no fact; a blank line is left out. A line is never kept.</para>
/// </remarks>
internal sealed class TranscriptSummary
{
    #region Members

    #region Constants

    /// <summary>
    /// Model that Claude Code writes on the messages it makes itself, which is not a model
    /// </summary>
    private const string SyntheticModel = "<synthetic>";

    #endregion

    #region Variables

    /// <summary>
    /// Formats of a time accepted: a full date and time, with fractions of a second or not, then <c>Z</c> or an offset
    /// </summary>
    private static readonly string[] _timeFormats = new string[]
    {
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz",
    };

    /// <summary>
    /// Latest time accepted, in UTC
    /// </summary>
    private readonly DateTimeOffset _latestAcceptedUtc;

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
    /// Earliest time of a record, in UTC; null while no record gave a time
    /// </summary>
    public DateTimeOffset? FirstRecordAtUtc { get; private set; }

    /// <summary>
    /// Latest time of a record, in UTC; null while no record gave a time
    /// </summary>
    public DateTimeOffset? LastRecordAtUtc { get; private set; }

    /// <summary>
    /// First working folder that is a full path; null while none was given
    /// </summary>
    public string? WorkingFolder { get; private set; }

    /// <summary>
    /// Model of the last message of the assistant; null while none was given
    /// </summary>
    public string? Model { get; private set; }

    /// <summary>
    /// Name of the last tool called by the assistant; null while none was called
    /// </summary>
    public string? LastToolName { get; private set; }

    /// <summary>
    /// Number of lines that could not be read, or one of whose facts could not be read
    /// </summary>
    public int UnreadableLineCount { get; private set; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="latestAcceptedUtc">Latest time accepted for a record (now, with a small margin); its offset must be zero</param>
    /// <exception cref="ArgumentOutOfRangeException">If the latest time accepted is not in UTC</exception>
    public TranscriptSummary(DateTimeOffset latestAcceptedUtc)
    {
        // Check
        if (latestAcceptedUtc.Offset != TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(latestAcceptedUtc), latestAcceptedUtc, $"The value of the parameter: {nameof(latestAcceptedUtc)} must be in UTC");

        // Setting the value
        _latestAcceptedUtc = latestAcceptedUtc;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to read the time of a record
    /// </summary>
    /// <param name="record">Record of the transcript</param>
    /// <param name="isRead">False when the record gives a time that cannot be read</param>
    /// <returns>[Can be null] The time, in UTC; null when the record gives none, or gives one that cannot be read</returns>
    private DateTimeOffset? ReadTime(JsonElement record, out bool isRead)
    {
        // Leaving out a record without time
        isRead = true;
        if (!record.TryGetProperty("timestamp", out JsonElement value))
        {
            // Function's return
            return null;
        }

        // Reading a full ISO time with its offset, not later than the latest time accepted
        string? text = value.ValueKind == JsonValueKind.String ? ReadString(value) : null;
        if (text is null
            || !DateTimeOffset.TryParseExact(text, _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset time)
            || time > _latestAcceptedUtc)
        {
            isRead = false;

            // Function's return
            return null;
        }

        // Function's return
        return time.ToUniversalTime();
    }

    /// <summary>
    /// Method used to read a JSON text, which fails when it holds an escaped half of a character
    /// </summary>
    /// <param name="value">JSON text</param>
    /// <returns>[Can be null] The text; null when it holds an escaped half of a character</returns>
    private static string? ReadString(JsonElement value)
    {
        try
        {
            // Function's return
            return value.GetString();
        }
        catch (InvalidOperationException)
        {
            // Function's return: an escaped half of a character is not a text
            return null;
        }
    }

    /// <summary>
    /// Method used to read the name of the last tool called in the content of a message
    /// </summary>
    /// <param name="message">Message of a record of the assistant</param>
    /// <param name="isRead">False when a name of tool cannot be read</param>
    /// <returns>[Can be null] The name of the last tool called; null when the message calls none</returns>
    private static string? ReadLastToolName(JsonElement message, out bool isRead)
    {
        // Looking at each item of the content
        isRead = true;
        string? toolName = null;
        if (message.TryGetProperty("content", out JsonElement content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in content.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("type", out JsonElement type) || type.ValueKind != JsonValueKind.String
                    || ReadString(type) != "tool_use" || !item.TryGetProperty("name", out JsonElement name) || name.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                string? readName = ReadString(name);
                if (readName is null)
                    isRead = false;
                else
                    toolName = readName;
            }
        }

        // Function's return
        return toolName;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to add a line of the transcript to the summary
    /// </summary>
    /// <param name="line">[Cannot be null] Line of the transcript</param>
    /// <exception cref="ArgumentNullException">If the line is null</exception>
    public void Add(string line)
    {
        // Check
        if (line is null)
            throw new ArgumentNullException(nameof(line), $"The value of the parameter: {nameof(line)} cannot be null");

        // Leaving out a blank line
        if (string.IsNullOrWhiteSpace(line))
            return;

        // Reading the line as one JSON object
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line);
        }
        catch (JsonException)
        {
            UnreadableLineCount++;
            return;
        }

        using (document)
        {
            JsonElement record = document.RootElement;
            if (record.ValueKind != JsonValueKind.Object)
            {
                UnreadableLineCount++;
                return;
            }

            // Reading each fact on its own, so that a fact that cannot be read never hides the others
            bool isLineRead = true;
            DateTimeOffset? time = ReadTime(record, out bool isTimeRead);
            isLineRead &= isTimeRead;
            if (time is not null)
            {
                FirstRecordAtUtc = FirstRecordAtUtc is null || time < FirstRecordAtUtc ? time : FirstRecordAtUtc;
                LastRecordAtUtc = LastRecordAtUtc is null || time > LastRecordAtUtc ? time : LastRecordAtUtc;
            }

            // Keeping the first working folder that is a full path
            if (record.TryGetProperty("cwd", out JsonElement cwd) && cwd.ValueKind == JsonValueKind.String)
            {
                string? workingFolder = ReadString(cwd);
                isLineRead &= workingFolder is not null;
                if (WorkingFolder is null && !string.IsNullOrWhiteSpace(workingFolder) && Path.IsPathFullyQualified(workingFolder))
                    WorkingFolder = workingFolder;
            }

            // Keeping the model and the last tool of a record of the assistant
            if (record.TryGetProperty("type", out JsonElement type) && type.ValueKind == JsonValueKind.String && ReadString(type) == "assistant"
                && record.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.Object)
            {
                if (message.TryGetProperty("model", out JsonElement modelValue) && modelValue.ValueKind == JsonValueKind.String)
                {
                    string? model = ReadString(modelValue);
                    isLineRead &= model is not null;
                    if (!string.IsNullOrWhiteSpace(model) && model != SyntheticModel)
                        Model = model;
                }

                string? toolName = ReadLastToolName(message, out bool isToolRead);
                isLineRead &= isToolRead;
                if (toolName is not null)
                    LastToolName = toolName;
            }

            // Counting a line one of whose facts could not be read
            if (!isLineRead)
                UnreadableLineCount++;
        }
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
