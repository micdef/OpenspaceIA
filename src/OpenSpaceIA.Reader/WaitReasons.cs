/*
 *	@file	src/OpenSpaceIA.Reader/WaitReasons.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the reasons of the waits: one reason per waiting session, and the problems met
 */

// Using
using System.Collections.ObjectModel;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Result of the reading of the reasons of the waits: one reason per waiting session, and the problems met
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a waiting session of the core always gives its reason; this result gives it for each waiting session,
/// and keeps the doubts so that they are seen.</para>
/// <para><b>Behavior:</b> immutable: the reasons and the problems are copied when the result is built. The reasons are
/// found by the identifier of the session.</para>
/// </remarks>
internal sealed record WaitReasons
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
    /// Reason of the wait of each waiting session, by identifier of the session
    /// </summary>
    public IReadOnlyDictionary<string, WaitReason> Reasons { get; }

    /// <summary>
    /// Problems met: transcripts missing or unreadable, lines that could not be read
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
    /// <param name="reasons">[Cannot be null] Reason of each waiting session, by identifier</param>
    /// <param name="problems">[Cannot be null] Problems met; no null element</param>
    /// <exception cref="ArgumentNullException">If the reasons or the problems are null</exception>
    /// <exception cref="ArgumentException">If the problems hold a null element</exception>
    public WaitReasons(IReadOnlyDictionary<string, WaitReason> reasons, IReadOnlyList<ReadProblem> problems)
    {
        // Check
        if (reasons is null)
            throw new ArgumentNullException(nameof(reasons), $"The value of the parameter: {nameof(reasons)} cannot be null");

        // Copying the reasons and the problems
        Reasons = new ReadOnlyDictionary<string, WaitReason>(new Dictionary<string, WaitReason>(reasons, StringComparer.Ordinal));
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
