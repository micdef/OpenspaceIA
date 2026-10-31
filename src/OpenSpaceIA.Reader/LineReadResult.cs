/*
 *	@file	src/OpenSpaceIA.Reader/LineReadResult.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the lines of a file: its counts, or why it failed
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
/// Result of the reading of the lines of a file of Claude Code: how many lines were given and skipped, or why it failed
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the lines themselves are given one by one while the file is read, so that a large transcript is
/// never held in memory; the result only says how the reading went.</para>
/// <para><b>Behavior:</b> immutable; built only by its two factory methods, so that a result is always either a success
/// with its counts, or a failure with its reason and its details. A failure may come after some lines were given.</para>
/// </remarks>
internal sealed record LineReadResult
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
    /// True when the whole file was read
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Number of lines given; 0 for a failure
    /// </summary>
    public int LineCount { get; }

    /// <summary>
    /// Number of lines skipped because they were longer than the size limit; 0 for a failure
    /// </summary>
    public int TooLongLineCount { get; }

    /// <summary>
    /// Position just after the last byte consumed, in bytes from the start of the file, where a next reading can start; 0
    /// for a failure
    /// </summary>
    public long EndOffset { get; }

    /// <summary>
    /// Why the reading failed; null when it succeeded
    /// </summary>
    public FileReadFailure? Failure { get; }

    /// <summary>
    /// Details of the failure, to be reported; null when the reading succeeded
    /// </summary>
    public string? Detail { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor, used by the factory methods only
    /// </summary>
    /// <param name="lineCount">Number of lines given</param>
    /// <param name="tooLongLineCount">Number of lines skipped</param>
    /// <param name="endOffset">Position just after the last byte consumed</param>
    /// <param name="failure">[Can be null] Reason of the failure, for a failure</param>
    /// <param name="detail">[Can be null] Details of the failure, for a failure</param>
    private LineReadResult(int lineCount, int tooLongLineCount, long endOffset, FileReadFailure? failure, string? detail)
    {
        // Setting the values
        IsSuccess = failure is null;
        LineCount = lineCount;
        TooLongLineCount = tooLongLineCount;
        EndOffset = endOffset;
        Failure = failure;
        Detail = detail;
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
    /// Method used to build the result of a file whose lines were all read
    /// </summary>
    /// <param name="lineCount">Number of lines given, 0 or more</param>
    /// <param name="tooLongLineCount">Number of lines skipped because they were too long, 0 or more</param>
    /// <param name="endOffset">Position just after the last byte consumed, 0 or more</param>
    /// <returns>A successful result holding the counts and the end position</returns>
    /// <exception cref="ArgumentOutOfRangeException">If a count or the position is negative</exception>
    public static LineReadResult Success(int lineCount, int tooLongLineCount, long endOffset)
    {
        // Checks
        if (lineCount < 0)
            throw new ArgumentOutOfRangeException(nameof(lineCount), lineCount, $"The value of the parameter: {nameof(lineCount)} cannot be negative");
        if (tooLongLineCount < 0)
            throw new ArgumentOutOfRangeException(nameof(tooLongLineCount), tooLongLineCount, $"The value of the parameter: {nameof(tooLongLineCount)} cannot be negative");
        if (endOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(endOffset), endOffset, $"The value of the parameter: {nameof(endOffset)} cannot be negative");

        // Function's return
        return new LineReadResult(lineCount, tooLongLineCount, endOffset, null, null);
    }

    /// <summary>
    /// Method used to build the result of a file whose lines could not be read
    /// </summary>
    /// <param name="failure">Reason of the failure</param>
    /// <param name="detail">[Cannot be null] Details of the failure, to be reported</param>
    /// <returns>A failed result holding the reason and the details</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the reason is not defined in <see cref="FileReadFailure"/></exception>
    /// <exception cref="ArgumentNullException">If the details are null, empty or white spaces</exception>
    public static LineReadResult Failed(FileReadFailure failure, string detail)
    {
        // Checks
        if (!Enum.IsDefined(failure))
            throw new ArgumentOutOfRangeException(nameof(failure), failure, $"The value of the parameter: {nameof(failure)} is not defined in {nameof(FileReadFailure)}");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentNullException(nameof(detail), $"The value of the parameter: {nameof(detail)} cannot be null or empty");

        // Function's return
        return new LineReadResult(0, 0, 0, failure, detail);
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
