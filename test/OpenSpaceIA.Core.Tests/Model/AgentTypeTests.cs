/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/AgentTypeTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the AgentType model
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Tests.Model;

/// <summary>
/// Tests of the AgentType model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class AgentTypeTests
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
    /// An agent type keeps the name, the description and the model it was given
    /// </summary>
    [Fact]
    public void AgentTypeKeepsItsValues()
    {
        // Building the agent type
        AgentType agentType = new AgentType("code-reviewer", "Reviews the code", "model-large");

        // Checking the properties
        agentType.Name.ShouldBe("code-reviewer");
        agentType.Description.ShouldBe("Reviews the code");
        agentType.Model.ShouldBe("model-large");
    }

    /// <summary>
    /// An agent type name cannot be null, empty or white spaces
    /// </summary>
    /// <param name="name">[Can be null] Blank name that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentTypeNameCannotBeBlank(string? name)
    {
        // Building the agent type with a blank name
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new AgentType(name!, "Reviews the code", "model-large"));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("name");
    }

    /// <summary>
    /// A blank description is stored as null
    /// </summary>
    /// <param name="description">[Can be null] Blank description</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentTypeBlankDescriptionIsStoredAsNull(string? description)
    {
        // Building the agent type with a blank description
        AgentType agentType = new AgentType("code-reviewer", description, "model-large");

        // Checking the normalized value
        agentType.Description.ShouldBeNull();
    }

    /// <summary>
    /// A blank model is stored as null
    /// </summary>
    /// <param name="model">[Can be null] Blank model</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentTypeBlankModelIsStoredAsNull(string? model)
    {
        // Building the agent type with a blank model
        AgentType agentType = new AgentType("code-reviewer", "Reviews the code", model);

        // Checking the normalized value
        agentType.Model.ShouldBeNull();
    }

    /// <summary>
    /// Two agent types built with the same arguments are equal
    /// </summary>
    [Fact]
    public void AgentTypesWithTheSameArgumentsAreEqual()
    {
        // Building two agent types with the same arguments
        AgentType first = new AgentType("code-reviewer", "Reviews the code", "model-large");
        AgentType second = new AgentType("code-reviewer", "Reviews the code", "model-large");

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two agent types with different models are not equal
    /// </summary>
    [Fact]
    public void AgentTypesWithDifferentModelsAreNotEqual()
    {
        // Building two agent types with different models
        AgentType first = new AgentType("code-reviewer", "Reviews the code", "model-large");
        AgentType second = new AgentType("code-reviewer", "Reviews the code", "model-small");

        // Checking the value equality
        first.ShouldNotBe(second);
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
