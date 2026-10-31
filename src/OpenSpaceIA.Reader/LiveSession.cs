/*
 *	@file	src/OpenSpaceIA.Reader/LiveSession.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A session of Claude Code whose process still runs, or may still run
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
/// A session of Claude Code whose process still runs, or may still run
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the open sessions are the chiefs of the office; a closed session is never kept.</para>
/// <para><b>Behavior:</b> immutable; two values with the same fields are equal. The liveness is alive, or unknown when the
/// system cannot say: a closed session is refused.</para>
/// </remarks>
internal sealed record LiveSession
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
    /// Facts read from the session file
    /// </summary>
    public SessionFile File { get; }

    /// <summary>
    /// Whether the process is known to run (alive), or cannot be checked (unknown)
    /// </summary>
    public ProcessLiveness Liveness { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="file">[Cannot be null] Facts read from the session file</param>
    /// <param name="liveness">Alive or unknown</param>
    /// <exception cref="ArgumentNullException">If the facts are null</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the liveness is closed, or not defined in its enumeration</exception>
    public LiveSession(SessionFile file, ProcessLiveness liveness)
    {
        // Checks
        if (file is null)
            throw new ArgumentNullException(nameof(file), $"The value of the parameter: {nameof(file)} cannot be null");
        if (liveness != ProcessLiveness.Alive && liveness != ProcessLiveness.Unknown)
            throw new ArgumentOutOfRangeException(nameof(liveness), liveness, $"The value of the parameter: {nameof(liveness)} must be alive or unknown");

        // Setting the values
        File = file;
        Liveness = liveness;
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
