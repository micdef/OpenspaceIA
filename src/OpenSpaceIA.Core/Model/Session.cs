/*
 *	@file	src/OpenSpaceIA.Core/Model/Session.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A main session of Claude Code: the chief
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Model;

/// <summary>
/// A main session of Claude Code: the chief
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> each open main session is one chief of the office, with its status, its project and its task list
/// (doc/analyse/state-definitions.md, § 1 and § 2).</para>
/// <para><b>Behavior:</b> immutable: the task list is copied when the session is built. A waiting session always gives its
/// reason, and only a waiting session has one. Value equality compares the task list by reference: two sessions built
/// separately with the same values are equal only when both have no task list (each construction copies the list), so
/// <c>==</c> cannot tell whether a task list has changed.</para>
/// </remarks>
public sealed record Session
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
    /// Identifier of the session
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Project of the session
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Status of the session, as Claude Code gives it
    /// </summary>
    public SessionStatus Status { get; }

    /// <summary>
    /// Why the session waits for the user; null when it does not wait
    /// </summary>
    public WaitReason? WaitReason { get; }

    /// <summary>
    /// Start of the session, in UTC
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Model really used by the session; null when it is not known yet
    /// </summary>
    public string? Model { get; }

    /// <summary>
    /// Task list of the session, read only; null when no task list is known (an empty list is a known list without task)
    /// </summary>
    public IReadOnlyList<PlanTask>? Tasks { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">[Cannot be null] Identifier of the session</param>
    /// <param name="project">[Cannot be null] Project of the session</param>
    /// <param name="status">Status of the session</param>
    /// <param name="waitReason">[Can be null] Why the session waits; required when the status is waiting, null otherwise</param>
    /// <param name="startedAtUtc">Start of the session, in UTC</param>
    /// <param name="model">[Can be null] Model really used by the session</param>
    /// <param name="tasks">[Can be null] Task list of the session; null when no task list is known</param>
    /// <exception cref="ArgumentNullException">If the identifier is null, empty or white spaces, or if the project is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the status or the wait reason is not defined in its enumeration</exception>
    /// <exception cref="ArgumentException">If a waiting session has no reason, if a session that does not wait has one, if the
    /// start is not in UTC, or if the task list contains a null element</exception>
    public Session(string id, Project project, SessionStatus status, WaitReason? waitReason, DateTimeOffset startedAtUtc,
        string? model, IReadOnlyList<PlanTask>? tasks)
    {
        // Checks of the wait reason, which depends on the status
        ModelGuard.RequireDefined(status, nameof(status));
        if (status == SessionStatus.Waiting && waitReason is null)
            throw new ArgumentException($"The value of the parameter: {nameof(waitReason)} is required when the session is waiting", nameof(waitReason));
        if (status != SessionStatus.Waiting && waitReason is not null)
            throw new ArgumentException($"The value of the parameter: {nameof(waitReason)} must be null when the session is not waiting", nameof(waitReason));
        if (waitReason is not null)
            ModelGuard.RequireDefined(waitReason.Value, nameof(waitReason));

        // Setting the values
        Id = ModelGuard.RequireText(id, nameof(id));
        Project = ModelGuard.RequireNotNull(project, nameof(project));
        Status = status;
        WaitReason = waitReason;
        StartedAtUtc = ModelGuard.RequireUtc(startedAtUtc, nameof(startedAtUtc));
        Model = ModelGuard.OptionalText(model);
        Tasks = tasks is null ? null : ModelGuard.CopyWithoutNull(tasks, nameof(tasks));
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
