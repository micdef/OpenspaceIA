/*
 *	@file	src/OpenSpaceIA.Reader/ReadProblem.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A problem met while the files of Claude Code are read
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
/// A problem met while the files of Claude Code are read: a file left out, or a value that could not be read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the reading is tolerant: a bad file is left out instead of stopping the reading
/// (doc/analyse/roadmap.md, 0.4.0). The problem keeps what was left out and why, so that nothing is lost silently; how it is
/// logged is decided later, with the convention of the logs.</para>
/// <para><b>Behavior:</b> immutable; two problems with the same values are equal. The reason never holds the content of a
/// file, only its path, its names and the reason of the failure.</para>
/// </remarks>
internal sealed record ReadProblem
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
    /// Full path of the file or of the folder concerned
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// What was left out, and why
    /// </summary>
    public string Reason { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file or of the folder concerned</param>
    /// <param name="reason">[Cannot be null] What was left out, and why</param>
    /// <exception cref="ArgumentNullException">If the path or the reason is null, empty or white spaces</exception>
    public ReadProblem(string filePath, string reason)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), $"The value of the parameter: {nameof(filePath)} cannot be null or empty");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason), $"The value of the parameter: {nameof(reason)} cannot be null or empty");

        // Setting the values
        FilePath = filePath;
        Reason = reason;
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
