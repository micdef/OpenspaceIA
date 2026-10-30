/*
 *	@file	src/OpenSpaceIA.Core/Model/PlanTask.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A task of the task list of a session
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
/// A task of the task list of a session
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the task cards show what the chief plans to do and where it stands.</para>
/// <para><b>Behavior:</b> immutable; two tasks with the same text and the same status are equal. The text comes from a
/// transcript: it is not trusted, and the screens show it as plain text only.</para>
/// </remarks>
public sealed record PlanTask
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
    /// Text of the task
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Status of the task
    /// </summary>
    public PlanTaskStatus Status { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="content">[Cannot be null] Text of the task</param>
    /// <param name="status">Status of the task</param>
    /// <exception cref="ArgumentNullException">If the text is null, empty or white spaces</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the status is not defined in <see cref="PlanTaskStatus"/></exception>
    public PlanTask(string content, PlanTaskStatus status)
    {
        // Setting the text and the status
        Content = ModelGuard.RequireText(content, nameof(content));
        Status = ModelGuard.RequireDefined(status, nameof(status));
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
