/*
 *	@file	src/OpenSpaceIA.Reader/TranscriptCursor.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Where the reading of the transcript of a chief stopped, and what was found so far
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
/// Where the reading of the transcript of a chief stopped, and what its follower found so far
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the transcript of a chief can hold hundreds of megabytes (305 MB measured on 2026-10-31); it is read
/// once, then only where it grew. Nothing is written on the disk (decision 6 of doc/analyse/claude-model-decisions.md).</para>
/// <para><b>Behavior:</b> holds the position just after the last complete line read, the follower that got the lines up to
/// it, and the number of lines skipped up to it. A transcript shorter than the position, or whose byte just before the
/// position is not a line feed (checked by <see cref="TranscriptFollowing"/>), is not the one that was read: the cursor starts
/// again from the start, with a new follower.</para>
/// </remarks>
/// <typeparam name="TFollower">Type of what follows the transcript</typeparam>
internal sealed class TranscriptCursor<TFollower>
    where TFollower : ITranscriptFollower, new()
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
    /// Position just after the last complete line read, in bytes
    /// </summary>
    public long Offset { get; private set; }

    /// <summary>
    /// What follows the transcript, with what it found up to the position
    /// </summary>
    public TFollower Follower { get; private set; } = new TFollower();

    /// <summary>
    /// Number of lines skipped up to the position because they were longer than the limit
    /// </summary>
    public int TooLongLineCount { get; private set; }

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

    // #N/A

    #endregion

    #region Publics

    /// <summary>
    /// Method used to start again from the start of a transcript that became shorter than the position
    /// </summary>
    /// <param name="length">Size of the transcript now, in bytes</param>
    /// <returns>
    /// True  ==> The transcript was shorter: the cursor starts again from the start, with a new follower<br/>
    /// False ==> The transcript is not shorter: the cursor is kept
    /// </returns>
    public bool RestartIfShorter(long length)
    {
        // Keeping a cursor whose transcript only grew
        if (length >= Offset)
        {
            // Function's return
            return false;
        }

        // Starting again from the start
        Restart();

        // Function's return
        return true;
    }

    /// <summary>
    /// Method used to start again from the start of the transcript, with a new follower
    /// </summary>
    public void Restart()
    {
        // Forgetting the position and what was found
        Offset = 0;
        Follower = new TFollower();
        TooLongLineCount = 0;
    }

    /// <summary>
    /// Method used to move the cursor to the end of a reading
    /// </summary>
    /// <param name="offset">End position of the reading, not before the current position</param>
    /// <param name="tooLongLineCount">Number of lines skipped by the reading because they were too long, 0 or more</param>
    /// <exception cref="ArgumentOutOfRangeException">If the position is before the current one, or if the number is negative</exception>
    public void MoveTo(long offset, int tooLongLineCount)
    {
        // Checks
        if (offset < Offset)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, $"The value of the parameter: {nameof(offset)} cannot be before the current position");
        if (tooLongLineCount < 0)
            throw new ArgumentOutOfRangeException(nameof(tooLongLineCount), tooLongLineCount, $"The value of the parameter: {nameof(tooLongLineCount)} cannot be negative");

        // Setting the position, and adding the lines skipped
        Offset = offset;
        TooLongLineCount += tooLongLineCount;
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
