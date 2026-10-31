/*
 *	@file	test/OpenSpaceIA.Reader.Tests/SessionFileTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the facts kept from a session file
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
/// Tests of the facts kept from a session file
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a value that breaks a rule of the type cannot be built.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class SessionFileTests
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
    /// Invented start of the session, in UTC
    /// </summary>
    private static readonly DateTimeOffset _startedAtUtc = new DateTimeOffset(2026, 10, 31, 9, 0, 0, TimeSpan.Zero);

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
    /// The facts are kept as given; the status may be null
    /// </summary>
    [Fact]
    public void FactsAreKept()
    {
        // Building the facts
        SessionFile file = new SessionFile(1234, "invented-session", _project, "4279", _startedAtUtc, null);

        // Checking the values
        file.ProcessId.ShouldBe(1234);
        file.SessionId.ShouldBe("invented-session");
        file.Project.ShouldBe(_project);
        file.ProcessStart.ShouldBe("4279");
        file.StartedAtUtc.ShouldBe(_startedAtUtc);
        file.Status.ShouldBeNull();
    }

    /// <summary>
    /// A process ID must be greater than 0
    /// </summary>
    /// <param name="processId">Process ID that must be refused</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ProcessIdMustBeGreaterThanZero(int processId)
    {
        // Building the facts with an invalid process ID
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new SessionFile(processId, "invented-session", _project, "4279", _startedAtUtc, null));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("processId");
    }

    /// <summary>
    /// The identifier and the project are required
    /// </summary>
    [Fact]
    public void IdentifierAndProjectAreRequired()
    {
        // Building the facts without identifier, then without project
        ArgumentNullException withoutIdentifier = Should.Throw<ArgumentNullException>(
            () => new SessionFile(1234, "  ", _project, "4279", _startedAtUtc, null));
        ArgumentNullException withoutProject = Should.Throw<ArgumentNullException>(
            () => new SessionFile(1234, "invented-session", null!, "4279", _startedAtUtc, null));

        // Checking the parameters in fault
        withoutIdentifier.ParamName.ShouldBe("sessionId");
        withoutProject.ParamName.ShouldBe("project");
    }

    /// <summary>
    /// The start time of the process must hold digits only
    /// </summary>
    /// <param name="processStart">[Can be null] Start time that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("42x")]
    public void ProcessStartMustHoldDigitsOnly(string? processStart)
    {
        // Building the facts with an invalid start time
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new SessionFile(1234, "invented-session", _project, processStart!, _startedAtUtc, null));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("processStart");
    }

    /// <summary>
    /// The start of the session must be in UTC
    /// </summary>
    [Fact]
    public void StartMustBeInUtc()
    {
        // Building the facts with a start that has an offset
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new SessionFile(1234, "invented-session", _project, "4279", new DateTimeOffset(2026, 10, 31, 10, 0, 0, TimeSpan.FromHours(1)), null));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("startedAtUtc");
    }

    /// <summary>
    /// A status outside the enumeration is refused
    /// </summary>
    [Fact]
    public void UndefinedStatusIsRefused()
    {
        // Building the facts with a status outside the enumeration
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new SessionFile(1234, "invented-session", _project, "4279", _startedAtUtc, (SessionStatus)999));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("status");
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
