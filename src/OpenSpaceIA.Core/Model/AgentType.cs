/*
 *	@file	src/OpenSpaceIA.Core/Model/AgentType.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	An agent type: one character of the office
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
/// An agent type (BB-8, C-3PO, Explore…): one character of the office
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the office shows one character per agent type, not one per run (doc/analyse/state-definitions.md,
/// § 1).</para>
/// <para><b>Behavior:</b> immutable; two agent types with the same values are equal. The description and the model are
/// optional: a blank one is stored as null. The model is the one asked in the definition of the agent; the model really used
/// is the one of each run.</para>
/// </remarks>
public sealed record AgentType
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
    /// Name of the agent type, which identifies it
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Description of the agent type; null when it has none
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Model asked in the definition of the agent type; null when it asks none
    /// </summary>
    public string? Model { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">[Cannot be null] Name of the agent type</param>
    /// <param name="description">[Can be null] Description of the agent type</param>
    /// <param name="model">[Can be null] Model asked in the definition of the agent type</param>
    /// <exception cref="ArgumentNullException">If the name is null, empty or white spaces</exception>
    public AgentType(string name, string? description, string? model)
    {
        // Setting the name, the description and the model
        Name = ModelGuard.RequireText(name, nameof(name));
        Description = ModelGuard.OptionalText(description);
        Model = ModelGuard.OptionalText(model);
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
