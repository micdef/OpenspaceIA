/*
 *	@file	test/OpenSpaceIA.Reader.Tests/RunActivityTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of what the runs give to the state rules: periods of work and agent types
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of what the runs give to the state rules: periods of work and agent types
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that each run gives one period of work of its agent type, and that an agent type seen only
/// in runs (a built-in agent, an agent of a plugin) gets a desk.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class RunActivityTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Invented project, full on every system
    /// </summary>
    private static readonly Project _project = new Project(Path.Combine(Path.GetTempPath(), "invented", "project"));

    /// <summary>
    /// Invented start of the runs
    /// </summary>
    private static readonly DateTimeOffset _start = new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero);

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

    /// <summary>
    /// Method used to build an invented run
    /// </summary>
    /// <param name="id">[Cannot be null] Identifier of the run</param>
    /// <param name="agentTypeName">[Cannot be null] Name of its agent type</param>
    /// <param name="minutes">Duration of the run, in minutes</param>
    /// <returns>The run</returns>
    private static AgentRun Run(string id, string agentTypeName, int minutes)
    {
        // Function's return
        return new AgentRun(id, agentTypeName, "invented-session", _project, _start, _start.AddMinutes(minutes), true, null);
    }

    #endregion

    #region Publics

    /// <summary>
    /// The periods of an agent type are its runs, from their first to their last record, as work, on their project
    /// </summary>
    [Fact]
    public void PeriodsOfAnAgentTypeAreItsRuns()
    {
        // Building runs of two agent types
        List<AgentRun> runs = new List<AgentRun>() { Run("a1", "Explore", 5), Run("a2", "csharp-reviewer", 7), Run("a3", "Explore", 0) };

        // Getting the periods of one agent type
        IReadOnlyList<ActivityPeriod> periods = RunActivity.GetPeriods("Explore", runs);

        // Checking the periods: a run of a single record gives a period of a single instant
        periods.ShouldBe(new List<ActivityPeriod>()
        {
            new ActivityPeriod(_project, PeriodKind.Working, _start, _start.AddMinutes(5)),
            new ActivityPeriod(_project, PeriodKind.Working, _start, _start),
        });
    }

    /// <summary>
    /// An agent type without run has no period, and the names are compared with their case
    /// </summary>
    [Fact]
    public void AgentTypeWithoutRunHasNoPeriod()
    {
        // Building a run of another agent type, whose name differs only by its case
        List<AgentRun> runs = new List<AgentRun>() { Run("a1", "explore", 5) };

        // Getting the periods
        RunActivity.GetPeriods("Explore", runs).ShouldBeEmpty();
    }

    /// <summary>
    /// The agent types of the definitions are kept in their order, and an agent type seen only in runs is added after them,
    /// with its name only, once, in the order of the runs
    /// </summary>
    [Fact]
    public void AgentTypesSeenOnlyInRunsAreAdded()
    {
        // Building two definitions and runs of a defined type and of two built-in types
        List<AgentType> definitions = new List<AgentType>() { new AgentType("csharp-reviewer", "Reviews C#.", "sonnet"), new AgentType("planner", null, "sonnet") };
        List<AgentRun> runs = new List<AgentRun>() { Run("a1", "Explore", 1), Run("a2", "csharp-reviewer", 1), Run("a3", "general-purpose", 1), Run("a4", "Explore", 1) };

        // Adding the agent types of the runs
        IReadOnlyList<AgentType> agentTypes = RunActivity.AddAgentTypesOfRuns(definitions, runs);

        // Checking the agent types
        agentTypes.ShouldBe(new List<AgentType>()
        {
            new AgentType("csharp-reviewer", "Reviews C#.", "sonnet"),
            new AgentType("planner", null, "sonnet"),
            new AgentType("Explore", null, null),
            new AgentType("general-purpose", null, null),
        });
    }

    /// <summary>
    /// The arguments are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Calling each method without each argument
        List<AgentRun> runs = new List<AgentRun>();
        Should.Throw<ArgumentNullException>(() => RunActivity.GetPeriods(" ", runs)).ParamName.ShouldBe("agentTypeName");
        Should.Throw<ArgumentNullException>(() => RunActivity.GetPeriods("Explore", null!)).ParamName.ShouldBe("runs");
        Should.Throw<ArgumentNullException>(() => RunActivity.AddAgentTypesOfRuns(null!, runs)).ParamName.ShouldBe("definitions");
        Should.Throw<ArgumentNullException>(() => RunActivity.AddAgentTypesOfRuns(new List<AgentType>(), null!)).ParamName.ShouldBe("runs");
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
