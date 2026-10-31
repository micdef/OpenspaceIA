/*
 *	@file	src/OpenSpaceIA.Reader/OpenToolCalls.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tool calls left open in the transcript of a chief, followed line by line
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
/// Tool calls left open in the transcript of a chief, followed line by line, and the reason of a wait that they give
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> while the status of a session is <c>waiting</c>, the reason is the tool call left open
/// (doc/analyse/spike-waiting-signal.md; doc/analyse/claude-model-decisions.md, decision 2).</para>
/// <para><b>Behavior:</b> a call is open from its item <c>tool_use</c> in a record of the assistant until an item
/// <c>tool_result</c> with its identifier in a record of the user. The state is kept between the lines, so that a
/// transcript that grows is followed from where it was. The reason of a wait is a question when an open call is
/// <c>AskUserQuestion</c>, a permission when another call is open, and a question when none is open (the status of a
/// question can come before its record; decision of Michaël, 2026-10-31). A line that is not a JSON object, or whose
/// identifiers cannot be read, is counted and changes nothing; a blank line is left out. A line is never kept.</para>
/// </remarks>
internal sealed class OpenToolCalls
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the tool that asks the user a question
    /// </summary>
    private const string QuestionToolName = "AskUserQuestion";

    #endregion

    #region Variables

    /// <summary>
    /// Name of each open tool call, by its identifier
    /// </summary>
    private readonly Dictionary<string, string> _openCalls = new Dictionary<string, string>(StringComparer.Ordinal);

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
    /// Number of tool calls open
    /// </summary>
    public int Count => _openCalls.Count;

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
    /// Method used to read the items of the content of the message of a record
    /// </summary>
    /// <param name="record">Record of the transcript</param>
    /// <returns>The items of the content; empty when the record has no content of items</returns>
    private static IEnumerable<JsonElement> ReadItems(JsonElement record)
    {
        // Function's return
        return record.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.Object
            && message.TryGetProperty("content", out JsonElement content) && content.ValueKind == JsonValueKind.Array
            ? content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).ToList()
            : Enumerable.Empty<JsonElement>();
    }

    /// <summary>
    /// Method used to read a text of an item
    /// </summary>
    /// <param name="item">Item of a content</param>
    /// <param name="key">[Cannot be null] Key of the text</param>
    /// <returns>[Can be null] The text; null when the key is missing or is not a text</returns>
    /// <exception cref="InvalidOperationException">If the text holds an escaped half of a character (caught by the caller)</exception>
    private static string? ReadText(JsonElement item, string key)
    {
        // Function's return
        return item.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to add a line of the transcript
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

            // Reading the calls opened and closed by the record, before changing anything
            string? type = ReadText(record, "type");
            List<(string Id, string Name)> opened = new List<(string Id, string Name)>();
            List<string> closed = new List<string>();
            foreach (JsonElement item in ReadItems(record))
            {
                string? itemType = ReadText(item, "type");
                if (type == "assistant" && itemType == "tool_use" && ReadText(item, "id") is string id && ReadText(item, "name") is string name)
                    opened.Add((id, name));
                else if (type == "user" && itemType == "tool_result" && ReadText(item, "tool_use_id") is string answeredId)
                    closed.Add(answeredId);
            }

            // Opening and closing the calls
            foreach ((string id, string name) in opened)
                _openCalls[id] = name;
            foreach (string id in closed)
                _openCalls.Remove(id);
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
    /// Method used to get the reason of a wait from the open tool calls
    /// </summary>
    /// <returns>
    /// Question   ==> An open call asks a question, or no call is open<br/>
    /// Permission ==> Calls are open, and none asks a question
    /// </returns>
    public WaitReason GetWaitReason()
    {
        // Function's return
        return _openCalls.Count == 0 || _openCalls.ContainsValue(QuestionToolName) ? WaitReason.Question : WaitReason.Permission;
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
