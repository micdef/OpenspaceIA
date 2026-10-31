/*
 *	@file	src/OpenSpaceIA.Reader/SessionFileParser.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of a session file of Claude Code
 */

// Using
using System.Globalization;
using System.Text.Json;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read a session file of Claude Code (<c>sessions/&lt;pid&gt;.json</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> keeps the facts the office needs from a session file (doc/analyse/spike-claude-files.md, § 2), and
/// reports every file that cannot give them instead of guessing.</para>
/// <para><b>Behavior:</b> the text must be one JSON object. The keys <c>pid</c> (a whole number greater than 0, the same as
/// the name of the file), <c>sessionId</c> (a text), <c>cwd</c> (a full path on this machine), <c>procStart</c> (a text of
/// digits) and <c>startedAt</c> (a whole number of milliseconds since 1970, UTC) are required: when one is missing or
/// invalid, the file is skipped and reported. The key <c>status</c> is optional: <c>busy</c>, <c>waiting</c> or
/// <c>idle</c>; a missing status is not a problem (Claude Code writes it about one second after the file), another value
/// leaves the status unknown and is reported. The other keys are left out. A problem holds the path and the name of the key,
/// never a value of the file.</para>
/// </remarks>
internal static class SessionFileParser
{
    #region Members

    #region Constants

    /// <summary>
    /// Extension of a session file
    /// </summary>
    private const string SessionExtension = ".json";

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
    /// Method used to read a required text value
    /// </summary>
    /// <param name="root">Object of the file</param>
    /// <param name="key">[Cannot be null] Key of the value</param>
    /// <returns>[Can be null] The text; null when the key is missing, is not a text, or is blank</returns>
    private static string? ReadText(JsonElement root, string key)
    {
        // Function's return
        return root.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
    }

    /// <summary>
    /// Method used to read the status of the session
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problem</param>
    /// <param name="root">Object of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where a status that is not known is added</param>
    /// <returns>[Can be null] The status; null when it is missing or not known</returns>
    private static SessionStatus? ReadStatus(string filePath, JsonElement root, ICollection<ReadProblem> problems)
    {
        // Leaving out a status that is not given yet
        if (!root.TryGetProperty("status", out JsonElement value))
        {
            // Function's return
            return null;
        }

        // Turning the known values into a status
        SessionStatus? status = value.ValueKind != JsonValueKind.String ? null : value.GetString() switch
        {
            "busy" => SessionStatus.Busy,
            "waiting" => SessionStatus.Waiting,
            "idle" => SessionStatus.Idle,
            _ => null,
        };
        if (status is null)
            problems.Add(new ReadProblem(filePath, "The value of the key: status is not a known status (busy, waiting, idle): the status of the session is unknown"));

        // Function's return
        return status;
    }

    /// <summary>
    /// Method used to read the facts of a session from the object of its file
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problems</param>
    /// <param name="root">Object of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where a file skipped or a status not known is added</param>
    /// <returns>[Can be null] The facts of the session; null when the file is skipped</returns>
    private static SessionFile? ReadSession(string filePath, JsonElement root, ICollection<ReadProblem> problems)
    {
        // Reading the process ID, which must be the name of the file
        if (!root.TryGetProperty("pid", out JsonElement pidValue) || pidValue.ValueKind != JsonValueKind.Number
            || !pidValue.TryGetInt32(out int processId) || processId <= 0)
        {
            problems.Add(new ReadProblem(filePath, "The key: pid is missing or is not a whole number greater than 0: the file is skipped"));

            // Function's return
            return null;
        }
        if (!string.Equals(Path.GetFileName(filePath), processId.ToString(CultureInfo.InvariantCulture) + SessionExtension, StringComparison.Ordinal))
        {
            problems.Add(new ReadProblem(filePath, "The key: pid is not the name of the file: the file is skipped"));

            // Function's return
            return null;
        }

        // Reading the identifier of the session and its working folder
        string? sessionId = ReadText(root, "sessionId");
        if (sessionId is null)
        {
            problems.Add(new ReadProblem(filePath, "The key: sessionId is missing or is not a text: the file is skipped"));

            // Function's return
            return null;
        }
        string? workingFolder = ReadText(root, "cwd");
        if (workingFolder is null || !Path.IsPathFullyQualified(workingFolder))
        {
            problems.Add(new ReadProblem(filePath, "The key: cwd is missing or is not a full path on this machine: the file is skipped"));

            // Function's return
            return null;
        }

        // Reading the start time of the process, which must hold digits only
        string? processStart = ReadText(root, "procStart");
        if (processStart is null || !processStart.All(char.IsAsciiDigit))
        {
            problems.Add(new ReadProblem(filePath, "The key: procStart is missing or is not a text of digits: the file is skipped"));

            // Function's return
            return null;
        }

        // Reading the start of the session, in milliseconds since 1970, within the range of the dates
        if (!root.TryGetProperty("startedAt", out JsonElement startedValue) || startedValue.ValueKind != JsonValueKind.Number
            || !startedValue.TryGetInt64(out long milliseconds)
            || milliseconds < DateTimeOffset.MinValue.ToUnixTimeMilliseconds() || milliseconds > DateTimeOffset.MaxValue.ToUnixTimeMilliseconds())
        {
            problems.Add(new ReadProblem(filePath, "The key: startedAt is missing or is not a whole number of milliseconds within the range of the dates: the file is skipped"));

            // Function's return
            return null;
        }

        // Function's return
        return new SessionFile(processId, sessionId, new Project(workingFolder), processStart, DateTimeOffset.FromUnixTimeMilliseconds(milliseconds),
            ReadStatus(filePath, root, problems));
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the facts of a session from the text of its file
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, whose name must be the process ID</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where a file skipped or a status not known is added</param>
    /// <returns>[Can be null] The facts of the session; null when the file is skipped</returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces, or if the text or the problems are null</exception>
    public static SessionFile? Parse(string filePath, string text, ICollection<ReadProblem> problems)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), $"The value of the parameter: {nameof(filePath)} cannot be null or empty");
        if (text is null)
            throw new ArgumentNullException(nameof(text), $"The value of the parameter: {nameof(text)} cannot be null");
        if (problems is null)
            throw new ArgumentNullException(nameof(problems), $"The value of the parameter: {nameof(problems)} cannot be null");

        try
        {
            // Reading the text as one JSON object
            using JsonDocument document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                problems.Add(new ReadProblem(filePath, "The file is not a JSON object: it is skipped"));

                // Function's return
                return null;
            }

            // Function's return
            return ReadSession(filePath, document.RootElement, problems);
        }
        catch (JsonException exception)
        {
            problems.Add(new ReadProblem(filePath, $"The file is not valid JSON (line {exception.LineNumber}, byte {exception.BytePositionInLine}): it is skipped"));

            // Function's return
            return null;
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
