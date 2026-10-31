/*
 *	@file	src/OpenSpaceIA.Reader/LiveSessions.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the sessions: the open sessions, and the problems met
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Result of the reading of the sessions: the open sessions, and the problems met
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> gives the open sessions to the reading of the activity, and keeps every session file left out, so
/// that a missing chief is seen instead of hidden.</para>
/// <para><b>Behavior:</b> immutable: the lists are copied when the result is built. Empty lists are valid (no open session,
/// no problem). Value equality compares the lists by reference: two results built separately are never equal.</para>
/// </remarks>
internal sealed record LiveSessions
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
    /// Open sessions, in the order given by the reader
    /// </summary>
    public IReadOnlyList<LiveSession> Sessions { get; }

    /// <summary>
    /// Problems met: session files left out, unknown statuses, processes that cannot be checked
    /// </summary>
    public IReadOnlyList<ReadProblem> Problems { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sessions">[Cannot be null] Open sessions; no null element</param>
    /// <param name="problems">[Cannot be null] Problems met; no null element</param>
    /// <exception cref="ArgumentNullException">If a list is null</exception>
    /// <exception cref="ArgumentException">If a list holds a null element</exception>
    public LiveSessions(IReadOnlyList<LiveSession> sessions, IReadOnlyList<ReadProblem> problems)
    {
        // Copying the lists
        Sessions = ReadOnlyLists.CopyWithoutNull(sessions, nameof(sessions));
        Problems = ReadOnlyLists.CopyWithoutNull(problems, nameof(problems));
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
