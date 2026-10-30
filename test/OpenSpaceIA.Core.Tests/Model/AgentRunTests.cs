/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/AgentRunTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the AgentRun model
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
/// Tests of the AgentRun model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class AgentRunTests
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
    /// Method used to get the fixed UTC date shared by the tests
    /// </summary>
    /// <returns>A date whose offset is zero</returns>
    private static DateTimeOffset UtcDate()
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);
    }

    /// <summary>
    /// Method used to get a fixed date that is not expressed in UTC
    /// </summary>
    /// <returns>A date whose offset is two hours</returns>
    private static DateTimeOffset NonUtcDate()
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 30, 10, 0, 0, TimeSpan.FromHours(2));
    }

    /// <summary>
    /// Method used to get the invented project shared by the tests
    /// </summary>
    /// <returns>A valid project</returns>
    private static Project CreateProject()
    {
        // Function's return
        return new Project("/home/dev/projects/alpha");
    }

    #endregion

    #region Publics

    /// <summary>
    /// A valid run keeps every value it was given
    /// </summary>
    [Fact]
    public void AgentRunKeepsItsValues()
    {
        // Building the run
        Project project = CreateProject();
        DateTimeOffset lastRecord = UtcDate().AddMinutes(5);
        AgentRun run = new AgentRun("run-01", "code-reviewer", "session-01", project, UtcDate(), lastRecord, true, "model-large");

        // Checking the properties
        run.Id.ShouldBe("run-01");
        run.AgentTypeName.ShouldBe("code-reviewer");
        run.SessionId.ShouldBe("session-01");
        run.Project.ShouldBe(project);
        run.StartedAtUtc.ShouldBe(UtcDate());
        run.LastRecordAtUtc.ShouldBe(lastRecord);
        run.IsFinished.ShouldBeTrue();
        run.Model.ShouldBe("model-large");
    }

    /// <summary>
    /// The finished flag is stored as given
    /// </summary>
    /// <param name="isFinished">Finished flag given to the run</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AgentRunKeepsItsFinishedFlag(bool isFinished)
    {
        // Building the run
        AgentRun run = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), isFinished, null);

        // Checking the flag
        run.IsFinished.ShouldBe(isFinished);
    }

    /// <summary>
    /// A run id cannot be null, empty or white spaces
    /// </summary>
    /// <param name="id">[Can be null] Blank id that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentRunIdCannotBeBlank(string? id)
    {
        // Building the run with a blank id
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new AgentRun(id!, "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("id");
    }

    /// <summary>
    /// A run agent type name cannot be null, empty or white spaces
    /// </summary>
    /// <param name="agentTypeName">[Can be null] Blank agent type name that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentRunAgentTypeNameCannotBeBlank(string? agentTypeName)
    {
        // Building the run with a blank agent type name
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new AgentRun("run-01", agentTypeName!, "session-01", CreateProject(), UtcDate(), UtcDate(), false, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("agentTypeName");
    }

    /// <summary>
    /// A run session id cannot be null, empty or white spaces
    /// </summary>
    /// <param name="sessionId">[Can be null] Blank session id that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentRunSessionIdCannotBeBlank(string? sessionId)
    {
        // Building the run with a blank session id
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new AgentRun("run-01", "code-reviewer", sessionId!, CreateProject(), UtcDate(), UtcDate(), false, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("sessionId");
    }

    /// <summary>
    /// A run must have a project
    /// </summary>
    [Fact]
    public void AgentRunProjectCannotBeNull()
    {
        // Building the run without project
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new AgentRun("run-01", "code-reviewer", "session-01", null!, UtcDate(), UtcDate(), false, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("project");
    }

    /// <summary>
    /// A run start date must be expressed in UTC
    /// </summary>
    [Fact]
    public void AgentRunStartDateMustBeUtc()
    {
        // Building the run with a start date that is not in UTC
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), NonUtcDate(), UtcDate(), false, null));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("startedAtUtc");
    }

    /// <summary>
    /// A run last record date must be expressed in UTC
    /// </summary>
    [Fact]
    public void AgentRunLastRecordDateMustBeUtc()
    {
        // Building the run with a last record date that is not in UTC
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), NonUtcDate(), false, null));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("lastRecordAtUtc");
    }

    /// <summary>
    /// A run cannot have its last record before its start
    /// </summary>
    [Fact]
    public void AgentRunLastRecordCannotBeBeforeItsStart()
    {
        // Building the run with a last record one second before the start
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new AgentRun(
                "run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate().AddSeconds(-1), false, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("lastRecordAtUtc");
    }

    /// <summary>
    /// A run with a single record has its last record equal to its start, which is allowed
    /// </summary>
    [Fact]
    public void AgentRunWithASingleRecordIsAllowed()
    {
        // Building the run with a last record equal to the start
        AgentRun run = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, null);

        // Checking the dates
        run.LastRecordAtUtc.ShouldBe(run.StartedAtUtc);
    }

    /// <summary>
    /// A blank model is stored as null
    /// </summary>
    /// <param name="model">[Can be null] Blank model</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgentRunBlankModelIsStoredAsNull(string? model)
    {
        // Building the run with a blank model
        AgentRun run = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, model);

        // Checking the normalized value
        run.Model.ShouldBeNull();
    }

    /// <summary>
    /// Two runs built with the same arguments are equal
    /// </summary>
    [Fact]
    public void AgentRunsWithTheSameArgumentsAreEqual()
    {
        // Building two runs with the same arguments
        AgentRun first = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, "model-large");
        AgentRun second = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, "model-large");

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two runs with a different finished flag are not equal
    /// </summary>
    [Fact]
    public void AgentRunsWithDifferentFinishedFlagsAreNotEqual()
    {
        // Building two runs that differ by the finished flag
        AgentRun first = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), false, null);
        AgentRun second = new AgentRun("run-01", "code-reviewer", "session-01", CreateProject(), UtcDate(), UtcDate(), true, null);

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
