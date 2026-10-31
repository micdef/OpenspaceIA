/*
 *	@file	src/OpenSpaceIA.Reader/TranscriptCursor.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Where the reading of the transcript of a chief stopped, and what it found so far
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
/// Where the reading of the transcript of a chief stopped, and what it found so far
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the transcript of a chief can hold hundreds of megabytes (305 MB measured on 2026-10-31); it is read
/// once, then only where it grew. Nothing is written on the disk (decision 6 of doc/analyse/claude-model-decisions.md).</para>
/// <para><b>Behavior:</b> holds the position just after the last complete line read, and the open tool calls found up to
/// it. A transcript shorter than the position was rewritten: the cursor starts again from the start, with nothing found.</para>
/// </remarks>
internal sealed class TranscriptCursor
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
    /// Tool calls left open up to the position
    /// </summary>
    public OpenToolCalls Calls { get; private set; } = new OpenToolCalls();

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
    /// True  ==> The transcript was shorter: the cursor starts again from the start, with nothing found<br/>
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
        Offset = 0;
        Calls = new OpenToolCalls();

        // Function's return
        return true;
    }

    /// <summary>
    /// Method used to move the cursor to the end of a reading
    /// </summary>
    /// <param name="offset">End position of the reading, not before the current position</param>
    /// <exception cref="ArgumentOutOfRangeException">If the position is before the current one</exception>
    public void MoveTo(long offset)
    {
        // Check
        if (offset < Offset)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, $"The value of the parameter: {nameof(offset)} cannot be before the current position");

        // Setting the position
        Offset = offset;
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
