/*
 *	@file	src/OpenSpaceIA.Reader/SubagentRuns.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the sub-agent runs: the runs, and the problems met
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
/// Result of the reading of the sub-agent runs: the runs, and the problems met
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> gives the runs to the core, and keeps every run left out, so that a missing activity is seen
/// instead of hidden.</para>
/// <para><b>Behavior:</b> immutable: the lists are copied when the result is built. Empty lists are valid. Value equality
/// compares the lists by reference: two results built separately are never equal.</para>
/// </remarks>
internal sealed record SubagentRuns
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
    /// Runs read, in the order given by the reader
    /// </summary>
    public IReadOnlyList<AgentRun> Runs { get; }

    /// <summary>
    /// Problems met: runs left out, lines that could not be read, folders not followed
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
    /// <param name="runs">[Cannot be null] Runs read; no null element</param>
    /// <param name="problems">[Cannot be null] Problems met; no null element</param>
    /// <exception cref="ArgumentNullException">If a list is null</exception>
    /// <exception cref="ArgumentException">If a list holds a null element</exception>
    public SubagentRuns(IReadOnlyList<AgentRun> runs, IReadOnlyList<ReadProblem> problems)
    {
        // Copying the lists
        Runs = ReadOnlyLists.CopyWithoutNull(runs, nameof(runs));
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
