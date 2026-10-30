/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/SessionTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the Session model
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
/// Tests of the Session model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class SessionTests
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

    /// <summary>
    /// Method used to build a session with the valid shared values, except the ones under test
    /// </summary>
    /// <param name="status">Status of the session</param>
    /// <param name="waitReason">[Can be null] Wait reason of the session</param>
    /// <param name="tasks">[Can be null] Task list of the session</param>
    /// <returns>The built session</returns>
    private static Session CreateSession(SessionStatus status, WaitReason? waitReason, IReadOnlyList<PlanTask>? tasks)
    {
        // Function's return
        return new Session("session-01", CreateProject(), status, waitReason, UtcDate(), "model-large", tasks);
    }

    #endregion

    #region Publics

    /// <summary>
    /// A valid session keeps every value it was given
    /// </summary>
    [Fact]
    public void SessionKeepsItsValues()
    {
        // Building the session
        Project project = CreateProject();
        PlanTask task = new PlanTask("Write the unit tests", PlanTaskStatus.Pending);
        Session session = new Session(
            "session-01", project, SessionStatus.Waiting, WaitReason.Question, UtcDate(), "model-large",
            new List<PlanTask> { task });

        // Checking the properties
        session.Id.ShouldBe("session-01");
        session.Project.ShouldBe(project);
        session.Status.ShouldBe(SessionStatus.Waiting);
        session.WaitReason.ShouldBe(WaitReason.Question);
        session.StartedAtUtc.ShouldBe(UtcDate());
        session.Model.ShouldBe("model-large");
        session.Tasks.ShouldNotBeNull();
        session.Tasks.Count.ShouldBe(1);
        session.Tasks[0].ShouldBe(task);
    }

    /// <summary>
    /// A session id cannot be null, empty or white spaces
    /// </summary>
    /// <param name="id">[Can be null] Blank id that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SessionIdCannotBeBlank(string? id)
    {
        // Building the session with a blank id
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new Session(id!, CreateProject(), SessionStatus.Busy, null, UtcDate(), "model-large", null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("id");
    }

    /// <summary>
    /// A session must have a project
    /// </summary>
    [Fact]
    public void SessionProjectCannotBeNull()
    {
        // Building the session without project
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new Session("session-01", null!, SessionStatus.Busy, null, UtcDate(), "model-large", null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("project");
    }

    /// <summary>
    /// A session status must be a value defined in the enumeration
    /// </summary>
    [Fact]
    public void SessionStatusMustBeDefined()
    {
        // Building the session with an undefined status
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new Session("session-01", CreateProject(), (SessionStatus)42, null, UtcDate(), "model-large", null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("status");
    }

    /// <summary>
    /// A session start date must be expressed in UTC
    /// </summary>
    [Fact]
    public void SessionStartDateMustBeUtc()
    {
        // Building the session with a start date that is not in UTC
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new Session("session-01", CreateProject(), SessionStatus.Busy, null, NonUtcDate(), "model-large", null));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("startedAtUtc");
    }

    /// <summary>
    /// A blank model is stored as null
    /// </summary>
    /// <param name="model">[Can be null] Blank model</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SessionBlankModelIsStoredAsNull(string? model)
    {
        // Building the session with a blank model
        Session session = new Session("session-01", CreateProject(), SessionStatus.Busy, null, UtcDate(), model, null);

        // Checking the normalized value
        session.Model.ShouldBeNull();
    }

    /// <summary>
    /// A waiting session must give the reason why it waits
    /// </summary>
    [Fact]
    public void WaitingSessionWithoutReasonIsRefused()
    {
        // Building a waiting session without reason
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => CreateSession(SessionStatus.Waiting, null, null));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("waitReason");
    }

    /// <summary>
    /// A session that is not waiting cannot have a wait reason
    /// </summary>
    /// <param name="status">Status other than waiting</param>
    /// <param name="waitReason">Reason that must be refused</param>
    [Theory]
    [InlineData(SessionStatus.Busy, WaitReason.Question)]
    [InlineData(SessionStatus.Busy, WaitReason.Permission)]
    [InlineData(SessionStatus.Idle, WaitReason.Question)]
    [InlineData(SessionStatus.Idle, WaitReason.Permission)]
    public void SessionThatIsNotWaitingWithReasonIsRefused(SessionStatus status, WaitReason waitReason)
    {
        // Building a session that is not waiting, with a reason
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => CreateSession(status, waitReason, null));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("waitReason");
    }

    /// <summary>
    /// A waiting session keeps its wait reason
    /// </summary>
    /// <param name="waitReason">Reason given to the waiting session</param>
    [Theory]
    [InlineData(WaitReason.Question)]
    [InlineData(WaitReason.Permission)]
    public void WaitingSessionKeepsItsReason(WaitReason waitReason)
    {
        // Building a waiting session with a reason
        Session session = CreateSession(SessionStatus.Waiting, waitReason, null);

        // Checking the properties
        session.Status.ShouldBe(SessionStatus.Waiting);
        session.WaitReason.ShouldBe(waitReason);
    }

    /// <summary>
    /// A busy or idle session without reason is valid and has no wait reason
    /// </summary>
    /// <param name="status">Status other than waiting</param>
    [Theory]
    [InlineData(SessionStatus.Busy)]
    [InlineData(SessionStatus.Idle)]
    public void SessionThatIsNotWaitingHasNoReason(SessionStatus status)
    {
        // Building a session that is not waiting, without reason
        Session session = CreateSession(status, null, null);

        // Checking the properties
        session.Status.ShouldBe(status);
        session.WaitReason.ShouldBeNull();
    }

    /// <summary>
    /// A wait reason must be a value defined in the enumeration
    /// </summary>
    [Fact]
    public void SessionWaitReasonMustBeDefined()
    {
        // Building a waiting session with an undefined reason
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => CreateSession(SessionStatus.Waiting, (WaitReason)42, null));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("waitReason");
    }

    /// <summary>
    /// A null task list means that no list is known and stays null
    /// </summary>
    [Fact]
    public void SessionNullTasksStayNull()
    {
        // Building the session without task list
        Session session = CreateSession(SessionStatus.Busy, null, null);

        // Checking that nothing was invented
        session.Tasks.ShouldBeNull();
    }

    /// <summary>
    /// An empty task list stays an empty list and is not turned into null
    /// </summary>
    [Fact]
    public void SessionEmptyTasksStayEmpty()
    {
        // Building the session with an empty task list
        Session session = CreateSession(SessionStatus.Busy, null, new List<PlanTask>());

        // Checking the list
        session.Tasks.ShouldNotBeNull();
        session.Tasks.Count.ShouldBe(0);
    }

    /// <summary>
    /// The task list is copied: changing the source list afterwards does not change the session
    /// </summary>
    [Fact]
    public void SessionTasksAreCopied()
    {
        // Building the session from a list that the caller keeps
        List<PlanTask> source = new List<PlanTask> { new PlanTask("Write the unit tests", PlanTaskStatus.Pending) };
        Session session = CreateSession(SessionStatus.Busy, null, source);

        // Changing the source list after the construction
        source.Add(new PlanTask("Write the code", PlanTaskStatus.Pending));

        // Checking that the session did not follow
        session.Tasks.ShouldNotBeNull();
        session.Tasks.Count.ShouldBe(1);
        session.Tasks[0].Content.ShouldBe("Write the unit tests");
    }

    /// <summary>
    /// A task list cannot contain a null element
    /// </summary>
    [Fact]
    public void SessionTasksCannotContainNull()
    {
        // Building the session with a null task in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => CreateSession(SessionStatus.Busy, null, new List<PlanTask> { null! }));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("tasks");
    }

    /// <summary>
    /// Two sessions built with the same arguments and no task list are equal
    /// </summary>
    [Fact]
    public void SessionsWithTheSameArgumentsAndNoTasksAreEqual()
    {
        // Building two sessions with the same arguments
        Session first = CreateSession(SessionStatus.Busy, null, null);
        Session second = CreateSession(SessionStatus.Busy, null, null);

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two sessions with different statuses are not equal
    /// </summary>
    [Fact]
    public void SessionsWithDifferentStatusesAreNotEqual()
    {
        // Building two sessions with different statuses
        Session first = CreateSession(SessionStatus.Busy, null, null);
        Session second = CreateSession(SessionStatus.Idle, null, null);

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
