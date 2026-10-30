/*
 *	@file	src/OpenSpaceIA.Core/Model/Machine.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A machine where Claude Code runs (a building of the street)
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
/// A machine where Claude Code runs
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> each machine is one building of the street; its activity is read on that machine only.</para>
/// <para><b>Behavior:</b> immutable; two machines with the same name are equal (value equality of the record).</para>
/// </remarks>
public sealed record Machine
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
    /// Name of the machine, as shown on its building
    /// </summary>
    public string Name { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">[Cannot be null] Name of the machine</param>
    /// <exception cref="ArgumentNullException">If the name is null, empty or white spaces</exception>
    public Machine(string name)
    {
        // Setting the name
        Name = ModelGuard.RequireText(name, nameof(name));
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
