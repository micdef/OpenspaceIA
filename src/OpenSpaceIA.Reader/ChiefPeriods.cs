/*
 *	@file	src/OpenSpaceIA.Reader/ChiefPeriods.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the periods of the chief: the periods, and the problems met
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
/// Result of the reading of the periods of the chief: the periods of work and of waiting, and the problems met
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> gives the state rules the periods of the chief over the window, and keeps the doubts so that a
/// missing activity is seen.</para>
/// <para><b>Behavior:</b> immutable: the lists are copied when the result is built. The periods of several sessions may
/// overlap: the state rules merge them. Value equality compares the lists by reference.</para>
/// </remarks>
internal sealed record ChiefPeriods
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
    /// Periods of work and of waiting of the chief, transcript after transcript
    /// </summary>
    public IReadOnlyList<ActivityPeriod> Periods { get; }

    /// <summary>
    /// Problems met: transcripts unreadable, lines that could not be read, turns without project, folders not followed
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
    /// <param name="periods">[Cannot be null] Periods of the chief; no null element</param>
    /// <param name="problems">[Cannot be null] Problems met; no null element</param>
    /// <exception cref="ArgumentNullException">If a list is null</exception>
    /// <exception cref="ArgumentException">If a list holds a null element</exception>
    public ChiefPeriods(IReadOnlyList<ActivityPeriod> periods, IReadOnlyList<ReadProblem> problems)
    {
        // Copying the lists
        Periods = ReadOnlyLists.CopyWithoutNull(periods, nameof(periods));
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
