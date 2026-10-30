/*
 *	@file	test/OpenSpaceIA.Core.Tests/States/AgentTypeStateTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the state of an agent type
 */

// Using
using OpenSpaceIA.Core.Model;
using OpenSpaceIA.Core.States;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Tests.States;

/// <summary>
/// Tests of the state of an agent type
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class AgentTypeStateTests
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

    /// <summary>
    /// Method used to get the invented agent type shared by the tests
    /// </summary>
    /// <returns>A valid agent type named Explore</returns>
    private static AgentType CreateAgentType()
    {
        // Function's return
        return new AgentType("Explore", null, null);
    }

    /// <summary>
    /// Method used to build a run of an agent type
    /// </summary>
    /// <param name="agentTypeName">Name of the agent type that runs</param>
    /// <param name="isFinished">True when the run is finished</param>
    /// <returns>The built run</returns>
    private static AgentRun CreateRun(string agentTypeName, bool isFinished)
    {
        // Function's return
        return new AgentRun(
            "run-" + agentTypeName + "-" + isFinished, agentTypeName, "session-01", new Project("/home/dev/projects/alpha"),
            new DateTimeOffset(2026, 10, 30, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 30, 8, 5, 0, TimeSpan.Zero),
            isFinished, null);
    }

    #endregion

    #region Publics

    /// <summary>
    /// An agent type with a run in progress is working
    /// </summary>
    [Fact]
    public void AgentTypeWithARunInProgressIsWorking()
    {
        // Getting the state of an agent type with one unfinished run
        AgentState state = AgentStateRules.GetAgentTypeState(CreateAgentType(), new List<AgentRun> { CreateRun("Explore", false) });

        // Checking the state
        state.ShouldBe(AgentState.Working);
    }

    /// <summary>
    /// An agent type without any run is idle
    /// </summary>
    [Fact]
    public void AgentTypeWithoutRunIsIdle()
    {
        // Getting the state of an agent type with no run
        AgentState state = AgentStateRules.GetAgentTypeState(CreateAgentType(), new List<AgentRun>());

        // Checking the state
        state.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// An agent type whose runs are all finished is idle
    /// </summary>
    [Fact]
    public void AgentTypeWithOnlyFinishedRunsIsIdle()
    {
        // Getting the state of an agent type with finished runs only
        AgentState state = AgentStateRules.GetAgentTypeState(
            CreateAgentType(), new List<AgentRun> { CreateRun("Explore", true), CreateRun("Explore", true) });

        // Checking the state
        state.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// One run in progress among finished runs is enough to work, wherever it is in the list
    /// </summary>
    /// <param name="position">Position of the run in progress in the list</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AgentTypeWithOneRunInProgressAmongFinishedRunsIsWorking(int position)
    {
        // Building a list of three runs where only one is in progress
        List<AgentRun> runs = new List<AgentRun> { CreateRun("Explore", true), CreateRun("Explore", true), CreateRun("Explore", true) };
        runs[position] = CreateRun("Explore", false);

        // Getting the state
        AgentState state = AgentStateRules.GetAgentTypeState(CreateAgentType(), runs);

        // Checking the state
        state.ShouldBe(AgentState.Working);
    }

    /// <summary>
    /// The runs of the other agent types are ignored, even when they are in progress
    /// </summary>
    [Fact]
    public void RunsOfOtherAgentTypesAreIgnored()
    {
        // Getting the state of an agent type when only another agent type runs
        AgentState state = AgentStateRules.GetAgentTypeState(
            CreateAgentType(), new List<AgentRun> { CreateRun("C-3PO", false), CreateRun("BB-8", false) });

        // Checking the state
        state.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// Each agent type has its own state, even when the same list of runs is given to both
    /// </summary>
    [Fact]
    public void TwoAgentTypesHaveTheirOwnStateFromTheSameRuns()
    {
        // Building one list of runs where only Explore is in progress
        List<AgentRun> runs = new List<AgentRun> { CreateRun("Explore", false), CreateRun("C-3PO", true) };

        // Getting the state of both agent types
        AgentState explore = AgentStateRules.GetAgentTypeState(new AgentType("Explore", null, null), runs);
        AgentState reviewer = AgentStateRules.GetAgentTypeState(new AgentType("C-3PO", null, null), runs);

        // Checking the states
        explore.ShouldBe(AgentState.Working);
        reviewer.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// The name of the agent type is compared in an ordinal way, case sensitive and without trimming
    /// </summary>
    /// <param name="runName">Name of the run that is not the exact name of the agent type</param>
    [Theory]
    [InlineData("explore")]
    [InlineData("EXPLORE")]
    [InlineData("Explore ")]
    [InlineData("Explor")]
    public void AgentTypeNameIsComparedExactly(string runName)
    {
        // Getting the state of Explore when a run in progress has a name that differs slightly
        AgentState state = AgentStateRules.GetAgentTypeState(CreateAgentType(), new List<AgentRun> { CreateRun(runName, false) });

        // Checking the state
        state.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// An agent type is never waiting for you, whatever its runs: sub-agents do not wait for the user
    /// </summary>
    [Fact]
    public void AgentTypeIsNeverWaitingForYou()
    {
        // Building every combination of runs: none, finished, in progress, other agent type, mixed
        List<List<AgentRun>> cases = new List<List<AgentRun>>
        {
            new List<AgentRun>(),
            new List<AgentRun> { CreateRun("Explore", true) },
            new List<AgentRun> { CreateRun("Explore", false) },
            new List<AgentRun> { CreateRun("BB-8", false) },
            new List<AgentRun> { CreateRun("Explore", true), CreateRun("Explore", false), CreateRun("BB-8", false) },
        };

        // Checking that each case gives Working or Idle, and never anything else
        foreach (List<AgentRun> runs in cases)
        {
            AgentState state = AgentStateRules.GetAgentTypeState(CreateAgentType(), runs);
            state.ShouldBeOneOf(AgentState.Working, AgentState.Idle);
        }
    }

    /// <summary>
    /// The state of an agent type needs an agent type
    /// </summary>
    [Fact]
    public void AgentTypeStateNeedsAnAgentType()
    {
        // Getting the state of no agent type
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.GetAgentTypeState(null!, new List<AgentRun>()));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("agentType");
    }

    /// <summary>
    /// The state of an agent type needs a list of runs (an empty list is allowed, a missing list is not)
    /// </summary>
    [Fact]
    public void AgentTypeStateNeedsAListOfRuns()
    {
        // Getting the state with no list of runs
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.GetAgentTypeState(CreateAgentType(), null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("runs");
    }

    /// <summary>
    /// The list of runs cannot contain a null element
    /// </summary>
    [Fact]
    public void AgentTypeRunsCannotContainNull()
    {
        // Getting the state with a null run in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.GetAgentTypeState(CreateAgentType(), new List<AgentRun> { null! }));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("runs");
    }

    /// <summary>
    /// A null run is refused even when it comes after runs of other agent types
    /// </summary>
    [Fact]
    public void AgentTypeRunsCannotContainNullAfterOtherRuns()
    {
        // Getting the state with a null run after a run of another agent type
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.GetAgentTypeState(CreateAgentType(), new List<AgentRun> { CreateRun("BB-8", false), null! }));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("runs");
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
