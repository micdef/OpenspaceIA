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
/// The activity of one machine at a given moment: its sessions, its agent types and the runs of its sub-agents
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

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
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
    {
        // Setting the values
        Machine = ModelGuard.RequireNotNull(machine, nameof(machine));
        TakenAtUtc = ModelGuard.RequireUtc(takenAtUtc, nameof(takenAtUtc));
        Sessions = ModelGuard.CopyWithoutNull(sessions, nameof(sessions));
        AgentTypes = ModelGuard.CopyWithoutNull(agentTypes, nameof(agentTypes));
        Runs = ModelGuard.CopyWithoutNull(runs, nameof(runs));
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
