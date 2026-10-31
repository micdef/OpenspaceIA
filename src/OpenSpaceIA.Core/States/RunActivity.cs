/*
 *	@file	src/OpenSpaceIA.Core/States/RunActivity.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	What the sub-agent runs give to the state rules: periods of work and agent types
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.States;

/// <summary>
/// Service used to turn the sub-agent runs into what the state rules need: periods of work and agent types
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the period of work of an agent type is the time between the first and the last record of each of
/// its runs (doc/analyse/state-definitions.md, § 5); and an agent that has no definition of the user (a built-in agent such
/// as Explore, an agent of a plugin) still gets a desk when it runs (decision of Michaël, 2026-10-31).</para>
/// <para><b>Behavior:</b> pure: nothing is read or written. The names of the agent types are compared with their case, as
/// Claude Code does. Overlapping periods are kept as they are: the state rules merge them. In the core since 0.4.0, so that
/// every reader and every screen gets the periods of an agent type from its runs in the same way.</para>
/// </remarks>
public static class RunActivity
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

    // #N/A

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    // #N/A

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
    /// Method used to get the periods of work of an agent type from the runs
    /// </summary>
    /// <param name="agentTypeName">[Cannot be null] Name of the agent type</param>
    /// <param name="runs">[Cannot be null] Runs of every agent type</param>
    /// <returns>One period of work per run of the agent type, on the project of the run, in the order of the runs</returns>
    /// <exception cref="ArgumentNullException">If the name is null, empty or white spaces, or if the runs are null</exception>
    public static IReadOnlyList<ActivityPeriod> GetPeriods(string agentTypeName, IReadOnlyList<AgentRun> runs)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(agentTypeName))
            throw new ArgumentNullException(nameof(agentTypeName), $"The value of the parameter: {nameof(agentTypeName)} cannot be null or empty");
        if (runs is null)
            throw new ArgumentNullException(nameof(runs), $"The value of the parameter: {nameof(runs)} cannot be null");

        // Function's return
        return runs.Where(run => string.Equals(run.AgentTypeName, agentTypeName, StringComparison.Ordinal))
            .Select(run => new ActivityPeriod(run.Project, PeriodKind.Working, run.StartedAtUtc, run.LastRecordAtUtc))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Method used to add to the agent types of the definitions those that are seen only in the runs
    /// </summary>
    /// <param name="definitions">[Cannot be null] Agent types of the definitions of the user</param>
    /// <param name="runs">[Cannot be null] Runs of every agent type</param>
    /// <returns>The agent types of the definitions in their order, then each agent type seen only in the runs, once, with its
    /// name only, in the order of the runs</returns>
    /// <exception cref="ArgumentNullException">If the definitions or the runs are null</exception>
    public static IReadOnlyList<AgentType> AddAgentTypesOfRuns(IReadOnlyList<AgentType> definitions, IReadOnlyList<AgentRun> runs)
    {
        // Checks
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions), $"The value of the parameter: {nameof(definitions)} cannot be null");
        if (runs is null)
            throw new ArgumentNullException(nameof(runs), $"The value of the parameter: {nameof(runs)} cannot be null");

        // Adding each name of a run that is not known yet
        List<AgentType> agentTypes = new List<AgentType>(definitions);
        HashSet<string> names = new HashSet<string>(definitions.Select(agentType => agentType.Name), StringComparer.Ordinal);
        foreach (AgentRun run in runs)
        {
            if (names.Add(run.AgentTypeName))
                agentTypes.Add(new AgentType(run.AgentTypeName, null, null));
        }

        // Function's return
        return agentTypes.AsReadOnly();
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
