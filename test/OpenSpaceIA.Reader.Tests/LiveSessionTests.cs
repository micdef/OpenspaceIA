/*
 *	@file	test/OpenSpaceIA.Reader.Tests/LiveSessionTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of an open session, and of the result of the reading of the sessions
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
/// Tests of an open session, and of the result of the reading of the sessions
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a closed session can never be kept, and that the result cannot be changed after it is
/// built.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class LiveSessionTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Invented facts of a session file
    /// </summary>
    private static readonly SessionFile _file = new SessionFile(1234, "invented-session",
        new Project(Path.Combine(Path.GetTempPath(), "invented", "project")), "4279", new DateTimeOffset(2026, 10, 31, 9, 0, 0, TimeSpan.Zero), SessionStatus.Idle);

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
    /// An alive session and a session whose process cannot be checked are kept
    /// </summary>
    /// <param name="livenessValue">Liveness accepted (an integer, because the enumeration is internal)</param>
    [Theory]
    [InlineData((int)ProcessLiveness.Alive)]
    [InlineData((int)ProcessLiveness.Unknown)]
    public void AliveOrUnknownSessionIsKept(int livenessValue)
    {
        // Building the session
        ProcessLiveness liveness = (ProcessLiveness)livenessValue;
        LiveSession session = new LiveSession(_file, liveness);

        // Checking the values
        session.File.ShouldBe(_file);
        session.Liveness.ShouldBe(liveness);
    }

    /// <summary>
    /// A closed session, or a liveness outside the enumeration, is refused
    /// </summary>
    /// <param name="livenessValue">Liveness refused (an integer, because the enumeration is internal)</param>
    [Theory]
    [InlineData((int)ProcessLiveness.Closed)]
    [InlineData(999)]
    public void ClosedSessionIsRefused(int livenessValue)
    {
        // Building a closed session
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new LiveSession(_file, (ProcessLiveness)livenessValue));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("liveness");
    }

    /// <summary>
    /// The facts of the session are required
    /// </summary>
    [Fact]
    public void FileCannotBeNull()
    {
        // Building a session without facts
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new LiveSession(null!, ProcessLiveness.Alive));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("file");
    }

    /// <summary>
    /// The lists of the result are copied, cannot be changed, and cannot hold null
    /// </summary>
    [Fact]
    public void ResultListsAreCopiedAndReadOnly()
    {
        // Building the result from lists that are changed afterwards
        List<LiveSession> sessions = new List<LiveSession>() { new LiveSession(_file, ProcessLiveness.Alive) };
        List<ReadProblem> problems = new List<ReadProblem>() { new ReadProblem("/invented/sessions/1.json", "invented reason") };
        LiveSessions result = new LiveSessions(sessions, problems);
        sessions.Clear();
        problems.Clear();

        // Checking the copy, the read-only lists and the refused values
        result.Sessions.Count.ShouldBe(1);
        result.Problems.Count.ShouldBe(1);
        Should.Throw<NotSupportedException>(() => ((IList<LiveSession>)result.Sessions).Clear());
        Should.Throw<ArgumentNullException>(() => new LiveSessions(null!, problems)).ParamName.ShouldBe("sessions");
        Should.Throw<ArgumentException>(() => new LiveSessions(new List<LiveSession>() { null! }, problems)).ParamName.ShouldBe("sessions");
        Should.Throw<ArgumentNullException>(() => new LiveSessions(sessions, null!)).ParamName.ShouldBe("problems");
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
