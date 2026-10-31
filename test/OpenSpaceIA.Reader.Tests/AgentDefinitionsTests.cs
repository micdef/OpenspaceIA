/*
 *	@file	test/OpenSpaceIA.Reader.Tests/AgentDefinitionsTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the result of the reading of the agent definitions
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
/// Tests of the result of the reading of the agent definitions
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that the result is complete and cannot be changed after it is built.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class AgentDefinitionsTests
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
    /// The lists are copied: changing the given lists afterwards does not change the result
    /// </summary>
    [Fact]
    public void ListsAreCopied()
    {
        // Building the result from lists that are changed afterwards
        List<AgentType> agentTypes = new List<AgentType>() { new AgentType("helper", null, null) };
        List<ReadProblem> problems = new List<ReadProblem>() { new ReadProblem("/invented/agents/b.md", "invented reason") };
        AgentDefinitions definitions = new AgentDefinitions(agentTypes, problems);
        agentTypes.Clear();
        problems.Clear();

        // Checking that the result kept its values
        definitions.AgentTypes.Count.ShouldBe(1);
        definitions.Problems.Count.ShouldBe(1);
    }

    /// <summary>
    /// The lists of the result cannot be changed through a cast
    /// </summary>
    [Fact]
    public void ListsCannotBeChanged()
    {
        // Building the result
        AgentDefinitions definitions = new AgentDefinitions(new List<AgentType>() { new AgentType("helper", null, null) }, new List<ReadProblem>());

        // Checking that the lists are read-only
        Should.Throw<NotSupportedException>(() => ((IList<AgentType>)definitions.AgentTypes).Clear());
        Should.Throw<NotSupportedException>(() => ((IList<ReadProblem>)definitions.Problems).Clear());
    }

    /// <summary>
    /// The lists are required and cannot hold a null element
    /// </summary>
    [Fact]
    public void ListsCannotBeNullNorHoldNull()
    {
        // Building the result with null lists, and with lists that hold null
        ArgumentNullException withoutAgentTypes = Should.Throw<ArgumentNullException>(() => new AgentDefinitions(null!, new List<ReadProblem>()));
        ArgumentNullException withoutProblems = Should.Throw<ArgumentNullException>(() => new AgentDefinitions(new List<AgentType>(), null!));
        ArgumentException nullAgentType = Should.Throw<ArgumentException>(() => new AgentDefinitions(new List<AgentType>() { null! }, new List<ReadProblem>()));
        ArgumentException nullProblem = Should.Throw<ArgumentException>(() => new AgentDefinitions(new List<AgentType>(), new List<ReadProblem>() { null! }));

        // Checking the exact types and the parameters in fault
        withoutAgentTypes.ParamName.ShouldBe("agentTypes");
        withoutProblems.ParamName.ShouldBe("problems");
        nullAgentType.ShouldBeOfType<ArgumentException>();
        nullAgentType.ParamName.ShouldBe("agentTypes");
        nullProblem.ShouldBeOfType<ArgumentException>();
        nullProblem.ParamName.ShouldBe("problems");
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
