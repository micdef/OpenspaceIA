/*
 *	@file	src/OpenSpaceIA.Reader/SessionFile.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	What the reader keeps of a session file of Claude Code
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// What the reader keeps of a session file of Claude Code (<c>sessions/&lt;pid&gt;.json</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the facts of the session file, before they are joined with its transcript (its model, its task
/// list, the reason of a wait) into a <see cref="Session"/> of the core.</para>
/// <para><b>Behavior:</b> immutable; two values with the same fields are equal. The status is null when the file does not
/// give one yet (Claude Code writes it about one second after the file) or gives one that is not known.</para>
/// </remarks>
internal sealed record SessionFile
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
    /// ID of the process of the session
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Identifier of the session, which names its transcript
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Project the session works on: its working folder
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Start time of the process, as Claude Code writes it: digits only, whose meaning depends on the system
    /// </summary>
    public string ProcessStart { get; }

    /// <summary>
    /// When the session started, in UTC
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Status of the session; null when it is not given yet, or not known
    /// </summary>
    public SessionStatus? Status { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processId">ID of the process, greater than 0</param>
    /// <param name="sessionId">[Cannot be null] Identifier of the session</param>
    /// <param name="project">[Cannot be null] Project the session works on</param>
    /// <param name="processStart">[Cannot be null] Start time of the process: digits only</param>
    /// <param name="startedAtUtc">When the session started; its offset must be zero</param>
    /// <param name="status">[Can be null] Status of the session</param>
    /// <exception cref="ArgumentOutOfRangeException">If the ID is not greater than 0, if the start is not in UTC, or if the
    /// status is not defined in its enumeration</exception>
    /// <exception cref="ArgumentNullException">If the identifier is null, empty or white spaces, or if the project is null</exception>
    /// <exception cref="ArgumentException">If the start time of the process is empty or holds something else than digits</exception>
    public SessionFile(int processId, string sessionId, Project project, string processStart, DateTimeOffset startedAtUtc, SessionStatus? status)
    {
        // Checks
        if (processId <= 0)
            throw new ArgumentOutOfRangeException(nameof(processId), processId, $"The value of the parameter: {nameof(processId)} must be greater than 0");
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentNullException(nameof(sessionId), $"The value of the parameter: {nameof(sessionId)} cannot be null or empty");
        if (project is null)
            throw new ArgumentNullException(nameof(project), $"The value of the parameter: {nameof(project)} cannot be null");
        if (string.IsNullOrEmpty(processStart) || !processStart.All(char.IsAsciiDigit))
            throw new ArgumentException($"The value of the parameter: {nameof(processStart)} must hold digits only", nameof(processStart));
        if (startedAtUtc.Offset != TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(startedAtUtc), startedAtUtc, $"The value of the parameter: {nameof(startedAtUtc)} must be in UTC");
        if (status is not null && !Enum.IsDefined(status.Value))
            throw new ArgumentOutOfRangeException(nameof(status), status, $"The value of the parameter: {nameof(status)} is not a known status");

        // Setting the values
        ProcessId = processId;
        SessionId = sessionId;
        Project = project;
        ProcessStart = processStart;
        StartedAtUtc = startedAtUtc;
        Status = status;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    // #N/A

    #endregion

    #region Publics

    // #N/A

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
