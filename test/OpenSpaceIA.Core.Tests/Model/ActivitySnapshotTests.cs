/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/ActivitySnapshotTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the ActivitySnapshot model
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
/// Tests of the ActivitySnapshot model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class ActivitySnapshotTests
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
    /// Method used to get the invented machine shared by the tests
    /// </summary>
    /// <returns>A valid machine</returns>
    private static Machine CreateMachine()
    {
        // Function's return
        return new Machine("workstation-01");
    }

    /// <summary>
    /// Method used to get an invented session
    /// </summary>
    /// <returns>A valid session</returns>
    private static Session CreateSession()
    {
        // Function's return
        return new Session(
            "session-01", new Project("/home/dev/projects/alpha"), SessionStatus.Busy, null, UtcDate(), "model-large", null);
    }

    /// <summary>
    /// Method used to get an invented agent type
    /// </summary>
    /// <returns>A valid agent type</returns>
    private static AgentType CreateAgentType()
    {
        // Function's return
        return new AgentType("code-reviewer", "Reviews the code", "model-large");
    }

    /// <summary>
    /// Method used to get an invented agent run
    /// </summary>
    /// <returns>A valid agent run</returns>
    private static AgentRun CreateRun()
    {
        // Function's return
        return new AgentRun(
            "run-01", "code-reviewer", "session-01", new Project("/home/dev/projects/alpha"), UtcDate(), UtcDate(), false, null);
    }

    #endregion

    #region Publics

    /// <summary>
    /// A valid snapshot keeps the machine, the date and the content of the three lists
    /// </summary>
    [Fact]
    public void ActivitySnapshotKeepsItsValues()
    {
        // Building the snapshot
        Machine machine = CreateMachine();
        Session session = CreateSession();
        AgentType agentType = CreateAgentType();
        AgentRun run = CreateRun();
        ActivitySnapshot snapshot = new ActivitySnapshot(
            machine,
            UtcDate(),
            new List<Session> { session },
            new List<AgentType> { agentType },
            new List<AgentRun> { run });

        // Checking the properties
        snapshot.Machine.ShouldBe(machine);
        snapshot.TakenAtUtc.ShouldBe(UtcDate());
        snapshot.Sessions.Count.ShouldBe(1);
        snapshot.Sessions[0].ShouldBe(session);
        snapshot.AgentTypes.Count.ShouldBe(1);
        snapshot.AgentTypes[0].ShouldBe(agentType);
        snapshot.Runs.Count.ShouldBe(1);
        snapshot.Runs[0].ShouldBe(run);
    }

    /// <summary>
    /// A snapshot where nobody works has three empty lists, which is allowed
    /// </summary>
    [Fact]
    public void ActivitySnapshotWithEmptyListsIsAllowed()
    {
        // Building the snapshot with empty lists
        ActivitySnapshot snapshot = new ActivitySnapshot(
            CreateMachine(), UtcDate(), new List<Session>(), new List<AgentType>(), new List<AgentRun>());

        // Checking the lists
        snapshot.Sessions.Count.ShouldBe(0);
        snapshot.AgentTypes.Count.ShouldBe(0);
        snapshot.Runs.Count.ShouldBe(0);
    }

    /// <summary>
    /// A snapshot must have a machine
    /// </summary>
    [Fact]
    public void ActivitySnapshotMachineCannotBeNull()
    {
        // Building the snapshot without machine
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new ActivitySnapshot(null!, UtcDate(), new List<Session>(), new List<AgentType>(), new List<AgentRun>()));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("machine");
    }

    /// <summary>
    /// A snapshot date must be expressed in UTC
    /// </summary>
    [Fact]
    public void ActivitySnapshotDateMustBeUtc()
    {
        // Building the snapshot with a date that is not in UTC
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivitySnapshot(
                CreateMachine(), NonUtcDate(), new List<Session>(), new List<AgentType>(), new List<AgentRun>()));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("takenAtUtc");
    }

    /// <summary>
    /// A snapshot must have a list of sessions
    /// </summary>
    [Fact]
    public void ActivitySnapshotSessionsCannotBeNull()
    {
        // Building the snapshot without list of sessions
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new ActivitySnapshot(CreateMachine(), UtcDate(), null!, new List<AgentType>(), new List<AgentRun>()));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("sessions");
    }

    /// <summary>
    /// A snapshot must have a list of agent types
    /// </summary>
    [Fact]
    public void ActivitySnapshotAgentTypesCannotBeNull()
    {
        // Building the snapshot without list of agent types
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new ActivitySnapshot(CreateMachine(), UtcDate(), new List<Session>(), null!, new List<AgentRun>()));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("agentTypes");
    }

    /// <summary>
    /// A snapshot must have a list of runs
    /// </summary>
    [Fact]
    public void ActivitySnapshotRunsCannotBeNull()
    {
        // Building the snapshot without list of runs
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new ActivitySnapshot(CreateMachine(), UtcDate(), new List<Session>(), new List<AgentType>(), null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("runs");
    }

    /// <summary>
    /// The list of sessions cannot contain a null element
    /// </summary>
    [Fact]
    public void ActivitySnapshotSessionsCannotContainNull()
    {
        // Building the snapshot with a null session in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivitySnapshot(
                CreateMachine(), UtcDate(), new List<Session> { null! }, new List<AgentType>(), new List<AgentRun>()));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("sessions");
    }

    /// <summary>
    /// The list of agent types cannot contain a null element
    /// </summary>
    [Fact]
    public void ActivitySnapshotAgentTypesCannotContainNull()
    {
        // Building the snapshot with a null agent type in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivitySnapshot(
                CreateMachine(), UtcDate(), new List<Session>(), new List<AgentType> { null! }, new List<AgentRun>()));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("agentTypes");
    }

    /// <summary>
    /// The list of runs cannot contain a null element
    /// </summary>
    [Fact]
    public void ActivitySnapshotRunsCannotContainNull()
    {
        // Building the snapshot with a null run in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivitySnapshot(
                CreateMachine(), UtcDate(), new List<Session>(), new List<AgentType>(), new List<AgentRun> { null! }));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("runs");
    }

    /// <summary>
    /// The list of sessions is copied: changing the source list afterwards does not change the snapshot
    /// </summary>
    [Fact]
    public void ActivitySnapshotSessionsAreCopied()
    {
        // Building the snapshot from a list that the caller keeps
        List<Session> source = new List<Session> { CreateSession() };
        ActivitySnapshot snapshot = new ActivitySnapshot(
            CreateMachine(), UtcDate(), source, new List<AgentType>(), new List<AgentRun>());

        // Changing the source list after the construction
        source.Add(CreateSession());

        // Checking that the snapshot did not follow
        snapshot.Sessions.Count.ShouldBe(1);
    }

    /// <summary>
    /// The list of agent types is copied: changing the source list afterwards does not change the snapshot
    /// </summary>
    [Fact]
    public void ActivitySnapshotAgentTypesAreCopied()
    {
        // Building the snapshot from a list that the caller keeps
        List<AgentType> source = new List<AgentType> { CreateAgentType() };
        ActivitySnapshot snapshot = new ActivitySnapshot(
            CreateMachine(), UtcDate(), new List<Session>(), source, new List<AgentRun>());

        // Changing the source list after the construction
        source.Add(CreateAgentType());

        // Checking that the snapshot did not follow
        snapshot.AgentTypes.Count.ShouldBe(1);
    }

    /// <summary>
    /// The list of runs is copied: changing the source list afterwards does not change the snapshot
    /// </summary>
    [Fact]
    public void ActivitySnapshotRunsAreCopied()
    {
        // Building the snapshot from a list that the caller keeps
        List<AgentRun> source = new List<AgentRun> { CreateRun() };
        ActivitySnapshot snapshot = new ActivitySnapshot(
            CreateMachine(), UtcDate(), new List<Session>(), new List<AgentType>(), source);

        // Changing the source list after the construction
        source.Add(CreateRun());

        // Checking that the snapshot did not follow
        snapshot.Runs.Count.ShouldBe(1);
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
