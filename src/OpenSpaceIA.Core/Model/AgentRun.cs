/*
 *	@file	src/OpenSpaceIA.Core/Model/AgentRun.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	One run of a sub-agent, launched by a session
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
/// One run of a sub-agent, launched by a main session
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> an agent type works while one of its runs is in progress, and its periods of work come from its runs
/// (doc/analyse/state-definitions.md, § 2 and § 5).</para>
/// <para><b>Behavior:</b> immutable; two runs with the same values are equal. The last record is never earlier than the
/// start; both are equal for a run with a single record. Whether the run is finished is a fact read from the files
/// (doc/analyse/claude-model-decisions.md, decision 4).</para>
/// </remarks>
public sealed record AgentRun
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
    /// Identifier of the run
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Name of the agent type that runs
    /// </summary>
    public string AgentTypeName { get; }

    /// <summary>
    /// Identifier of the main session that launched the run
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Project of the run
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Time of the first record of the run, in UTC
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Time of the last record of the run, in UTC
    /// </summary>
    public DateTimeOffset LastRecordAtUtc { get; }

    /// <summary>
    /// True when the run is finished
    /// </summary>
    public bool IsFinished { get; }

    /// <summary>
    /// Model really used by the run; null when it is not known
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
    /// <param name="id">[Cannot be null] Identifier of the run</param>
    /// <param name="agentTypeName">[Cannot be null] Name of the agent type that runs</param>
    /// <param name="sessionId">[Cannot be null] Identifier of the main session that launched the run</param>
    /// <param name="project">[Cannot be null] Project of the run</param>
    /// <param name="startedAtUtc">Time of the first record of the run, in UTC</param>
    /// <param name="lastRecordAtUtc">Time of the last record of the run, in UTC</param>
    /// <param name="isFinished">True when the run is finished</param>
    /// <param name="model">[Can be null] Model really used by the run</param>
    /// <exception cref="ArgumentNullException">If an identifier or the name is null, empty or white spaces, or if the project is null</exception>
    /// <exception cref="ArgumentException">If a time is not in UTC</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the last record is earlier than the start</exception>
    public AgentRun(string id, string agentTypeName, string sessionId, Project project, DateTimeOffset startedAtUtc,
        DateTimeOffset lastRecordAtUtc, bool isFinished, string? model)
    {
        // Checks of the two times, which depend on each other
        ModelGuard.RequireUtc(startedAtUtc, nameof(startedAtUtc));
        ModelGuard.RequireUtc(lastRecordAtUtc, nameof(lastRecordAtUtc));
        if (lastRecordAtUtc < startedAtUtc)
            throw new ArgumentOutOfRangeException(nameof(lastRecordAtUtc), lastRecordAtUtc, $"The value of the parameter: {nameof(lastRecordAtUtc)} cannot be earlier than {nameof(startedAtUtc)}");

        // Setting the values
        Id = ModelGuard.RequireText(id, nameof(id));
        AgentTypeName = ModelGuard.RequireText(agentTypeName, nameof(agentTypeName));
        SessionId = ModelGuard.RequireText(sessionId, nameof(sessionId));
        Project = ModelGuard.RequireNotNull(project, nameof(project));
        StartedAtUtc = startedAtUtc;
        LastRecordAtUtc = lastRecordAtUtc;
        IsFinished = isFinished;
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
