/*
 *	@file	src/OpenSpaceIA.Reader/FileReadResult.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of a file: its text, or why it failed
 */

// Using
using System.Globalization;
using System.Text;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Result of the reading of a file of Claude Code: its text, or why it could not be read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the expected failures of a reading (missing, locked or forbidden file) are values, not exceptions,
/// so that one bad file never stops the reading of the others, and no failure is lost.</para>
/// <para><b>Behavior:</b> immutable; built only by its two factory methods, so that a result is always either a success
/// with a text, or a failure with its reason and its details. Its text (<c>ToString</c>) shows the length of the content,
/// never the content itself, so that a log never leaks a file of Claude Code.</para>
/// </remarks>
internal sealed record FileReadResult
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
    /// True when the file was read
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Text of the file; null when the reading failed
    /// </summary>
    public string? Content { get; }

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
    /// <param name="content">[Can be null] Text of the file, for a success</param>
    /// <param name="failure">[Can be null] Reason of the failure, for a failure</param>
    /// <param name="detail">[Can be null] Details of the failure, for a failure</param>
    private FileReadResult(string? content, FileReadFailure? failure, string? detail)
    {
        // Setting the values
        IsSuccess = failure is null;
        Content = content;
        Failure = failure;
        Detail = detail;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to write the members of the result into its text
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> replaces the method that the compiler writes for a record, which would print the content of the
    /// file in every log or message of a failed assertion.</para>
    /// <para><b>Behavior:</b> writes whether the reading succeeded, the length of the content, the reason of the failure and
    /// its details; never the content.</para>
    /// </remarks>
    /// <param name="builder">[Cannot be null] Text being built by <c>ToString</c></param>
    /// <returns>True, since members were written</returns>
    private bool PrintMembers(StringBuilder builder)
    {
        // Writing the members, the length of the content in place of the content
        builder.Append(CultureInfo.InvariantCulture, $"IsSuccess = {IsSuccess}, ContentLength = {Content?.Length.ToString(CultureInfo.InvariantCulture) ?? "none"}, ");
        builder.Append(CultureInfo.InvariantCulture, $"Failure = {Failure?.ToString() ?? "none"}, Detail = {Detail ?? "none"}");

        // Function's return
        return true;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to build the result of a file that was read
    /// </summary>
    /// <param name="content">[Cannot be null] Text of the file; it may be empty</param>
    /// <returns>A successful result holding the text</returns>
    /// <exception cref="ArgumentNullException">If the text is null</exception>
    public static FileReadResult Success(string content)
    {
        // Check
        if (content is null)
            throw new ArgumentNullException(nameof(content), $"The value of the parameter: {nameof(content)} cannot be null");

        // Function's return
        return new FileReadResult(content, null, null);
    }

    /// <summary>
    /// Method used to build the result of a file that could not be read
    /// </summary>
    /// <param name="failure">Reason of the failure</param>
    /// <param name="detail">[Cannot be null] Details of the failure, to be reported</param>
    /// <returns>A failed result holding the reason and the details</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the reason is not defined in <see cref="FileReadFailure"/></exception>
    /// <exception cref="ArgumentNullException">If the details are null, empty or white spaces</exception>
    public static FileReadResult Failed(FileReadFailure failure, string detail)
    {
        // Checks
        if (!Enum.IsDefined(failure))
            throw new ArgumentOutOfRangeException(nameof(failure), failure, $"The value of the parameter: {nameof(failure)} is not defined in {nameof(FileReadFailure)}");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentNullException(nameof(detail), $"The value of the parameter: {nameof(detail)} cannot be null or empty");

        // Function's return
        return new FileReadResult(null, failure, detail);
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
