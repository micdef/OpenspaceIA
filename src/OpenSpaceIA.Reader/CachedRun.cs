/*
 *	@file	src/OpenSpaceIA.Reader/CachedRun.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A sub-agent run already read, kept in memory while its files do not change
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
/// A sub-agent run already read, kept in memory while its files do not change
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> reading every transcript of the week at each snapshot would be too heavy (72 MB measured on
/// 2026-10-31); a run whose files have not changed is not read again. Nothing is written on the disk (decision 6 of
/// doc/analyse/claude-model-decisions.md).</para>
/// <para><b>Behavior:</b> immutable. The files are known by their size and their last write, in UTC; a description that
/// does not exist has a size of -1. The problems of the reading are kept with the run, so that they are reported again.</para>
/// </remarks>
internal sealed record CachedRun
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
    /// Size of the transcript when it was read, in bytes
    /// </summary>
    public long TranscriptLength { get; }

    /// <summary>
    /// Last write of the transcript when it was read, in UTC
    /// </summary>
    public DateTime TranscriptWriteUtc { get; }

    /// <summary>
    /// Size of the description when it was read, in bytes; -1 when it did not exist
    /// </summary>
    public long MetaLength { get; }

    /// <summary>
    /// Last write of the description when it was read, in UTC; the smallest date when it did not exist
    /// </summary>
    public DateTime MetaWriteUtc { get; }

    /// <summary>
    /// Run read; null when the run was left out
    /// </summary>
    public AgentRun? Run { get; }

    /// <summary>
    /// Problems met while the run was read
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
    /// <param name="transcriptLength">Size of the transcript when it was read</param>
    /// <param name="transcriptWriteUtc">Last write of the transcript when it was read, in UTC</param>
    /// <param name="metaLength">Size of the description when it was read; -1 when it did not exist</param>
    /// <param name="metaWriteUtc">Last write of the description when it was read, in UTC</param>
    /// <param name="run">[Can be null] Run read; null when the run was left out</param>
    /// <param name="problems">[Cannot be null] Problems met while the run was read; no null element</param>
    /// <exception cref="ArgumentNullException">If the problems are null</exception>
    /// <exception cref="ArgumentException">If the problems hold a null element</exception>
    public CachedRun(long transcriptLength, DateTime transcriptWriteUtc, long metaLength, DateTime metaWriteUtc, AgentRun? run, IReadOnlyList<ReadProblem> problems)
    {
        // Setting the values
        TranscriptLength = transcriptLength;
        TranscriptWriteUtc = transcriptWriteUtc;
        MetaLength = metaLength;
        MetaWriteUtc = metaWriteUtc;
        Run = run;
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

    /// <summary>
    /// Method used to know whether the files of the run are still those that were read
    /// </summary>
    /// <param name="transcriptLength">Size of the transcript now</param>
    /// <param name="transcriptWriteUtc">Last write of the transcript now, in UTC</param>
    /// <param name="metaLength">Size of the description now; -1 when it does not exist</param>
    /// <param name="metaWriteUtc">Last write of the description now, in UTC</param>
    /// <returns>
    /// True  ==> Both files have the same size and the same last write: the run can be taken from the cache<br/>
    /// False ==> A file has changed: the run must be read again
    /// </returns>
    public bool IsSameAs(long transcriptLength, DateTime transcriptWriteUtc, long metaLength, DateTime metaWriteUtc)
    {
        // Function's return
        return TranscriptLength == transcriptLength && TranscriptWriteUtc == transcriptWriteUtc && MetaLength == metaLength && MetaWriteUtc == metaWriteUtc;
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
