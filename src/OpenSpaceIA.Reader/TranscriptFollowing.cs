/*
 *	@file	src/OpenSpaceIA.Reader/TranscriptFollowing.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the new lines of a transcript into its cursor, checking that it is still the transcript that was read
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
/// Service used to read the new lines of a transcript into its cursor, checking that it is still the transcript that was read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the transcript of a chief is read once, then only where it grew; a transcript rewritten in its
/// place must never be read from the middle of a line, as if the end of a line were a record.</para>
/// <para><b>Behavior:</b> a transcript shorter than the cursor is read again from its start. Otherwise, after a first
/// reading, the lines are read from the byte before the cursor with <see cref="ClaudeFileReader.ReadCompleteLines"/>: that
/// byte must be the line feed of the last line read, so the first line given must be empty, and it is left out; the others
/// go to the follower. When it is not empty (or no complete line follows the byte), the transcript was rewritten: the
/// cursor starts again, and the transcript is read from its start. A successful reading moves the cursor; a failed one does
/// not, and the caller forgets the cursor, since some lines may have been given before the failure.</para>
/// </remarks>
internal static class TranscriptFollowing
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

    // #N/A

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

    /// <summary>
    /// Method used to read the new complete lines into the follower, from the byte before the cursor
    /// </summary>
    /// <typeparam name="TFollower">Type of what follows the transcript</typeparam>
    /// <param name="fileReader">[Cannot be null] Reader of the files of Claude Code</param>
    /// <param name="path">[Cannot be null] Full path of the transcript</param>
    /// <param name="cursor">[Cannot be null] Cursor of the transcript</param>
    /// <param name="prepare">[Can be null] Action given the follower before it gets lines, again after a restart</param>
    /// <returns>The result of the last reading</returns>
    private static LineReadResult ReadNewLines<TFollower>(ClaudeFileReader fileReader, string path, TranscriptCursor<TFollower> cursor, Action<TFollower>? prepare)
        where TFollower : ITranscriptFollower, new()
    {
        // Preparing the follower
        prepare?.Invoke(cursor.Follower);

        // Reading from the start of a transcript never read
        if (cursor.Offset == 0)
        {
            // Function's return
            return fileReader.ReadCompleteLines(path, 0, cursor.Follower.Add);
        }

        // Reading from the byte before the cursor, which must be the line feed of the last line read
        bool isFirstLine = true;
        bool isSameTranscript = false;
        TFollower follower = cursor.Follower;
        LineReadResult result = fileReader.ReadCompleteLines(path, cursor.Offset - 1, line =>
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                isSameTranscript = line.Length == 0;
            }
            else if (isSameTranscript)
            {
                follower.Add(line);
            }
        });

        // Keeping a reading of the same transcript; reading a rewritten one again from its start (the byte before the cursor
        // is not a line feed, or no complete line follows it)
        if (!result.IsSuccess || isSameTranscript)
        {
            // Function's return
            return result;
        }
        cursor.Restart();
        prepare?.Invoke(cursor.Follower);

        // Function's return
        return fileReader.ReadCompleteLines(path, 0, cursor.Follower.Add);
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to give the follower of a transcript the lines added since the previous reading
    /// </summary>
    /// <typeparam name="TFollower">Type of what follows the transcript</typeparam>
    /// <param name="fileReader">[Cannot be null] Reader of the files of Claude Code</param>
    /// <param name="path">[Cannot be null] Full path of the transcript</param>
    /// <param name="length">Size of the transcript now, in bytes</param>
    /// <param name="cursor">[Cannot be null] Cursor of the transcript</param>
    /// <param name="prepare">[Can be null] Action given the follower before it gets lines, again after a restart (to give it
    /// the latest time accepted, for example)</param>
    /// <returns>The result of the reading; a failure leaves the cursor where it was</returns>
    /// <exception cref="ArgumentNullException">If the reader, the path or the cursor is null</exception>
    public static LineReadResult Follow<TFollower>(ClaudeFileReader fileReader, string path, long length, TranscriptCursor<TFollower> cursor, Action<TFollower>? prepare = null)
        where TFollower : ITranscriptFollower, new()
    {
        // Checks
        if (fileReader is null)
            throw new ArgumentNullException(nameof(fileReader), $"The value of the parameter: {nameof(fileReader)} cannot be null");
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), $"The value of the parameter: {nameof(path)} cannot be null or empty");
        if (cursor is null)
            throw new ArgumentNullException(nameof(cursor), $"The value of the parameter: {nameof(cursor)} cannot be null");

        // Starting again from the start of a shorter transcript, then reading the new lines
        cursor.RestartIfShorter(length);
        LineReadResult result = ReadNewLines(fileReader, path, cursor, prepare);

        // Moving the cursor after a successful reading
        if (result.IsSuccess)
            cursor.MoveTo(result.EndOffset, result.TooLongLineCount);

        // Function's return
        return result;
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
