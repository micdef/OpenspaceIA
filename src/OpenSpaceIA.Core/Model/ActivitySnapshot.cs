/*
 *	@file	src/OpenSpaceIA.Core/Model/ActivitySnapshot.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	The activity of one machine at a given moment
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
/// The activity of one machine at a given moment: its sessions, its agent types, the runs of its sub-agents, the periods of
/// its chief, and the problems met while it was read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> what a reader gives to the core; the screens and the state rules work on it.</para>
/// <para><b>Behavior:</b> immutable: the lists are copied when the snapshot is built, and a new snapshot replaces the previous
/// one. Empty lists are valid (nobody works). Value equality compares the lists by reference, and each construction copies
/// them: two snapshots built separately are never equal, so <c>==</c> cannot tell whether the activity has changed.</para>
/// </remarks>
public sealed record ActivitySnapshot
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
    /// Machine whose activity it is
    /// </summary>
    public Machine Machine { get; }

    /// <summary>
    /// Time the activity was read, in UTC
    /// </summary>
    public DateTimeOffset TakenAtUtc { get; }

    /// <summary>
    /// Open main sessions, read only
    /// </summary>
    public IReadOnlyList<Session> Sessions { get; }

    /// <summary>
    /// Known agent types, read only
    /// </summary>
    public IReadOnlyList<AgentType> AgentTypes { get; }

    /// <summary>
    /// Runs of the sub-agents, read only
    /// </summary>
    public IReadOnlyList<AgentRun> Runs { get; }

    /// <summary>
    /// Periods of work and of waiting of the chief over the window of history, read only; those of an agent type come from
    /// its runs
    /// </summary>
    public IReadOnlyList<ActivityPeriod> ChiefPeriods { get; }

    /// <summary>
    /// Problems met while the activity was read, read only
    /// </summary>
    public IReadOnlyList<ReadingProblem> Problems { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor of a snapshot without periods of the chief and without problems
    /// </summary>
    /// <param name="machine">[Cannot be null] Machine whose activity it is</param>
    /// <param name="takenAtUtc">Time the activity was read, in UTC</param>
    /// <param name="sessions">[Cannot be null] Open main sessions</param>
    /// <param name="agentTypes">[Cannot be null] Known agent types</param>
    /// <param name="runs">[Cannot be null] Runs of the sub-agents</param>
    /// <exception cref="ArgumentNullException">If the machine or a list is null</exception>
    /// <exception cref="ArgumentException">If the time is not in UTC, or if a list contains a null element</exception>
    public ActivitySnapshot(Machine machine, DateTimeOffset takenAtUtc, IReadOnlyList<Session> sessions,
        IReadOnlyList<AgentType> agentTypes, IReadOnlyList<AgentRun> runs)
        : this(machine, takenAtUtc, sessions, agentTypes, runs, Array.Empty<ActivityPeriod>(), Array.Empty<ReadingProblem>())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">[Cannot be null] Machine whose activity it is</param>
    /// <param name="takenAtUtc">Time the activity was read, in UTC</param>
    /// <param name="sessions">[Cannot be null] Open main sessions</param>
    /// <param name="agentTypes">[Cannot be null] Known agent types</param>
    /// <param name="runs">[Cannot be null] Runs of the sub-agents</param>
    /// <param name="chiefPeriods">[Cannot be null] Periods of work and of waiting of the chief</param>
    /// <param name="problems">[Cannot be null] Problems met while the activity was read</param>
    /// <exception cref="ArgumentNullException">If the machine or a list is null</exception>
    /// <exception cref="ArgumentException">If the time is not in UTC, or if a list contains a null element</exception>
    public ActivitySnapshot(Machine machine, DateTimeOffset takenAtUtc, IReadOnlyList<Session> sessions,
        IReadOnlyList<AgentType> agentTypes, IReadOnlyList<AgentRun> runs, IReadOnlyList<ActivityPeriod> chiefPeriods,
        IReadOnlyList<ReadingProblem> problems)
    {
        // Setting the values
        Machine = ModelGuard.RequireNotNull(machine, nameof(machine));
        TakenAtUtc = ModelGuard.RequireUtc(takenAtUtc, nameof(takenAtUtc));
        Sessions = ModelGuard.CopyWithoutNull(sessions, nameof(sessions));
        AgentTypes = ModelGuard.CopyWithoutNull(agentTypes, nameof(agentTypes));
        Runs = ModelGuard.CopyWithoutNull(runs, nameof(runs));
        ChiefPeriods = ModelGuard.CopyWithoutNull(chiefPeriods, nameof(chiefPeriods));
        Problems = ModelGuard.CopyWithoutNull(problems, nameof(problems));
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
