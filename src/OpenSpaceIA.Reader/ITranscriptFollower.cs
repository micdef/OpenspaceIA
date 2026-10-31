/*
 *	@file	src/OpenSpaceIA.Reader/ITranscriptFollower.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Something that follows a transcript line by line, and keeps what it found between the readings
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
/// Something that follows a transcript line by line, and keeps what it found between the readings
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a transcript of a chief is read once, then only where it grew (<see cref="TranscriptFollowing"/>);
/// each thing that follows it (the open tool calls, the periods of the chief) gets its new lines in their order.</para>
/// <para><b>Behavior:</b> a line that cannot be read is counted and never stops the following; a line is never kept.</para>
/// </remarks>
internal interface ITranscriptFollower
{
    #region Enumerations

    // #N/A

    #endregion

    #region Accessors/Properties

    #region Members

    /// <summary>
    /// Number of lines that could not be read, or one of whose facts could not be read
    /// </summary>
    public int UnreadableLineCount { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Method used to add the next line of the transcript
    /// </summary>
    /// <param name="line">[Cannot be null] Line of the transcript, without its line feed</param>
    /// <exception cref="ArgumentNullException">If the line is null</exception>
    public void Add(string line);

    #endregion
}
