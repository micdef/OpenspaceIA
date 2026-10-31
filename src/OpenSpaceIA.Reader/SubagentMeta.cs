/*
 *	@file	src/OpenSpaceIA.Reader/SubagentMeta.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	What the reader keeps of the description of a sub-agent run
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
/// What the reader keeps of the description of a sub-agent run (<c>agent-&lt;id&gt;.meta.json</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the description gives the agent type of the run, its desk in the office, which the transcript does
/// not give (doc/analyse/spike-claude-files.md, § 3).</para>
/// <para><b>Behavior:</b> immutable; two values with the same fields are equal. A run whose description does not say it is
/// finished is not finished by its description (its transcript may still say so).</para>
/// </remarks>
internal sealed record SubagentMeta
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
    /// Name of the agent type of the run
    /// </summary>
    public string AgentType { get; }

    /// <summary>
    /// True when the description says that the run is finished
    /// </summary>
    public bool IsFinished { get; }

    /// <summary>
    /// Model asked at the launch; null when none was asked
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
    /// <param name="agentType">[Cannot be null] Name of the agent type of the run</param>
    /// <param name="isFinished">True when the description says that the run is finished</param>
    /// <param name="model">[Can be null] Model asked at the launch</param>
    /// <exception cref="ArgumentNullException">If the name of the agent type is null, empty or white spaces</exception>
    public SubagentMeta(string agentType, bool isFinished, string? model)
    {
        // Check
        if (string.IsNullOrWhiteSpace(agentType))
            throw new ArgumentNullException(nameof(agentType), $"The value of the parameter: {nameof(agentType)} cannot be null or empty");

        // Setting the values
        AgentType = agentType;
        IsFinished = isFinished;
        Model = string.IsNullOrWhiteSpace(model) ? null : model;
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
