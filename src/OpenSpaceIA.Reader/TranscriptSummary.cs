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
/// <c>timestamp</c> of the records, in UTC; a time must give its offset (or <c>Z</c>). The working folder is the first
/// <c>cwd</c> that is a full path. The model is the last <c>message.model</c> of a record of the assistant, and the last tool
/// is the name of the last <c>tool_use</c> of a record of the assistant. A line that is not a JSON object, or whose time or
/// texts cannot be read, is counted and gives no fact at all; a blank line is left out. A line is never kept.</para>
/// </remarks>
internal sealed class TranscriptSummary
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
    /// Number of lines that could not be read
    /// </summary>
    public int UnreadableLineCount { get; private set; }

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
    /// Method used to read the time of a record, which must give its offset
    /// </summary>
    /// <param name="value">Value of the key <c>timestamp</c></param>
    /// <param name="time">The time, in UTC</param>
    /// <returns>
    /// True  ==> The value is a text of a time with its offset<br/>
    /// False ==> The value is of another kind, or is a time without offset (it would depend on the time zone of the machine)
    /// </returns>
    /// <exception cref="InvalidOperationException">If the text holds an escaped half of a character (caught by the caller)</exception>
    private static bool TryReadTime(JsonElement value, out DateTimeOffset time)
    {
        // Refusing a value that is not a text, or a time without offset
        time = default;
        if (value.ValueKind != JsonValueKind.String)
        {
            // Function's return
            return false;
        }
        string text = value.GetString()!;
        if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed) || parsed.Kind == DateTimeKind.Unspecified
            || !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset withOffset))
        {
            // Function's return
            return false;
        }

        // Turning the time into UTC
        time = withOffset.ToUniversalTime();

        // Function's return
        return true;
    }

    /// <summary>
    /// Method used to read the name of the last tool called in the content of a message
    /// </summary>
    /// <param name="message">Message of a record of the assistant</param>
    /// <returns>[Can be null] The name of the last tool called; null when the message calls none</returns>
    /// <exception cref="InvalidOperationException">If a name holds an escaped half of a character (caught by the caller)</exception>
    private static string? ReadLastToolName(JsonElement message)
    {
        // Looking at each item of the content
        string? toolName = null;
        if (message.TryGetProperty("content", out JsonElement content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in content.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("type", out JsonElement type) && type.ValueKind == JsonValueKind.String
                    && type.ValueEquals("tool_use") && item.TryGetProperty("name", out JsonElement name) && name.ValueKind == JsonValueKind.String)
                {
                    toolName = name.GetString();
                }
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

        try
        {
            // Reading the line as one JSON object
            using JsonDocument document = JsonDocument.Parse(line);
            JsonElement record = document.RootElement;
            if (record.ValueKind != JsonValueKind.Object)
            {
                UnreadableLineCount++;
                return;
            }

            // Reading every fact of the record first, so that a line that fails gives no fact at all
            DateTimeOffset? time = null;
            if (record.TryGetProperty("timestamp", out JsonElement timestamp))
            {
                if (!TryReadTime(timestamp, out DateTimeOffset readTime))
                {
                    UnreadableLineCount++;
                    return;
                }
                time = readTime;
            }
            string? workingFolder = record.TryGetProperty("cwd", out JsonElement cwd) && cwd.ValueKind == JsonValueKind.String ? cwd.GetString() : null;
            string? model = null;
            string? toolName = null;
            if (record.TryGetProperty("type", out JsonElement type) && type.ValueKind == JsonValueKind.String && type.ValueEquals("assistant")
                && record.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.Object)
            {
                model = message.TryGetProperty("model", out JsonElement modelValue) && modelValue.ValueKind == JsonValueKind.String ? modelValue.GetString() : null;
                toolName = ReadLastToolName(message);
            }

            // Keeping the facts of the record
            if (time is not null)
            {
                FirstRecordAtUtc = FirstRecordAtUtc is null || time < FirstRecordAtUtc ? time : FirstRecordAtUtc;
                LastRecordAtUtc = LastRecordAtUtc is null || time > LastRecordAtUtc ? time : LastRecordAtUtc;
            }
            if (WorkingFolder is null && !string.IsNullOrWhiteSpace(workingFolder) && Path.IsPathFullyQualified(workingFolder))
                WorkingFolder = workingFolder;
            if (!string.IsNullOrWhiteSpace(model))
                Model = model;
            if (toolName is not null)
                LastToolName = toolName;
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
