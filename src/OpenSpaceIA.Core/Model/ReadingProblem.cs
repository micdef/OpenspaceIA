/*
 *	@file	src/OpenSpaceIA.Core/Model/ReadingProblem.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A problem met while the activity was read: what was left out, and why
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Model;

/// <summary>
/// A problem met while the activity was read: what was left out or may be wrong, and why
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a reader never stops on a bad source, and never hides one: the problems travel with the snapshot,
/// so that the host writes them in its logs and the screens can say that something is unknown (decision of Michaël,
/// 2026-10-31).</para>
/// <para><b>Behavior:</b> immutable; two problems with the same values are equal. The source names where the problem is (a
/// file, a folder); the reason never holds a content of the source.</para>
/// </remarks>
public sealed record ReadingProblem
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
    /// Where the problem is: a file or a folder of the source
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// What was left out or may be wrong, and why
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
    /// <param name="source">[Cannot be null] Where the problem is</param>
    /// <param name="reason">[Cannot be null] What was left out or may be wrong, and why</param>
    /// <exception cref="ArgumentNullException">If the source or the reason is null, empty or white spaces</exception>
    public ReadingProblem(string source, string reason)
    {
        // Setting the values
        Source = ModelGuard.RequireText(source, nameof(source));
        Reason = ModelGuard.RequireText(reason, nameof(reason));
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
