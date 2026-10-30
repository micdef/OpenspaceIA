/*
 *	@file	test/OpenSpaceIA.Core.Tests/States/ChiefStateTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the state of the chief
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
/// Tests of the state of the chief
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class ChiefStateTests
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
    /// Method used to build a session of the chief with the given status
    /// </summary>
    /// <param name="status">Status of the session</param>
    /// <param name="waitReason">[Can be null] Reason of the wait; required for a waiting session</param>
    /// <returns>The built session</returns>
    private static Session CreateSession(SessionStatus status, WaitReason? waitReason)
    {
        // Function's return
        return new Session(
            "session-01", new Project("/home/dev/projects/alpha"), status, waitReason,
            new DateTimeOffset(2026, 10, 30, 8, 0, 0, TimeSpan.Zero), "model-large", null);
    }

    #endregion

    #region Publics

    /// <summary>
    /// A busy chief is working
    /// </summary>
    [Fact]
    public void BusyChiefIsWorking()
    {
        // Getting the state of a busy session
        AgentState state = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Busy, null));

        // Checking the state
        state.ShouldBe(AgentState.Working);
    }

    /// <summary>
    /// A chief that waits for the user is waiting for you, whatever the reason (a question or a permission)
    /// </summary>
    /// <param name="waitReason">Reason of the wait</param>
    [Theory]
    [InlineData(WaitReason.Question)]
    [InlineData(WaitReason.Permission)]
    public void WaitingChiefIsWaitingForYou(WaitReason waitReason)
    {
        // Getting the state of a waiting session
        AgentState state = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Waiting, waitReason));

        // Checking the state
        state.ShouldBe(AgentState.WaitingForYou);
    }

    /// <summary>
    /// An idle chief is idle
    /// </summary>
    [Fact]
    public void IdleChiefIsIdle()
    {
        // Getting the state of an idle session
        AgentState state = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Idle, null));

        // Checking the state
        state.ShouldBe(AgentState.Idle);
    }

    /// <summary>
    /// Each status gives exactly one state, and two statuses never give the same state: a chief is never working and idle
    /// at the same time
    /// </summary>
    [Fact]
    public void EachStatusGivesItsOwnSingleState()
    {
        // Getting the state of one session per status
        Dictionary<SessionStatus, AgentState> states = new Dictionary<SessionStatus, AgentState>
        {
            [SessionStatus.Busy] = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Busy, null)),
            [SessionStatus.Waiting] = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Waiting, WaitReason.Question)),
            [SessionStatus.Idle] = AgentStateRules.GetChiefState(CreateSession(SessionStatus.Idle, null)),
        };

        // Checking that every status of the enumeration was covered and that the three states are all different
        states.Count.ShouldBe(Enum.GetValues<SessionStatus>().Length);
        states.Values.Distinct().Count().ShouldBe(3);
    }

    /// <summary>
    /// The state of a chief does not depend on anything but its status: the same session gives the same state every time
    /// </summary>
    [Fact]
    public void ChiefStateIsTheSameEveryTime()
    {
        // Getting the state of the same session twice
        Session session = CreateSession(SessionStatus.Waiting, WaitReason.Permission);
        AgentState first = AgentStateRules.GetChiefState(session);
        AgentState second = AgentStateRules.GetChiefState(session);

        // Checking the states
        first.ShouldBe(AgentState.WaitingForYou);
        second.ShouldBe(first);
    }

    /// <summary>
    /// The state of a chief needs a session
    /// </summary>
    [Fact]
    public void ChiefStateNeedsASession()
    {
        // Getting the state of no session
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => AgentStateRules.GetChiefState(null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("session");
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
