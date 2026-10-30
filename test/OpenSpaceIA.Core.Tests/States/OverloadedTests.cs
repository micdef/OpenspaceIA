/*
 *	@file	test/OpenSpaceIA.Core.Tests/States/OverloadedTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the overloaded rule
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
/// Tests of the overloaded rule
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class OverloadedTests
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
    /// Method used to get the fixed "now" shared by the tests
    /// </summary>
    /// <returns>A date whose offset is zero</returns>
    private static DateTimeOffset Now()
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    }

    /// <summary>
    /// Method used to express a duration before "now"
    /// </summary>
    /// <param name="minutes">Minutes before now (negative for the future)</param>
    /// <param name="seconds">Seconds added to the minutes</param>
    /// <returns>The duration</returns>
    private static TimeSpan Ago(int minutes, int seconds = 0)
    {
        // Function's return
        return TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Method used to build a period relative to "now"
    /// </summary>
    /// <param name="project">Project of the period</param>
    /// <param name="kind">Kind of the period</param>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period</returns>
    private static ActivityPeriod Period(Project project, PeriodKind kind, TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return new ActivityPeriod(project, kind, Now() - startAgo, Now() - endAgo);
    }

    /// <summary>
    /// Method used to build a period of work relative to "now"
    /// </summary>
    /// <param name="project">Project of the period</param>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period</returns>
    private static ActivityPeriod Work(Project project, TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return Period(project, PeriodKind.Working, startAgo, endAgo);
    }

    /// <summary>
    /// Method used to build a period of waiting relative to "now"
    /// </summary>
    /// <param name="project">Project of the period</param>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period</returns>
    private static ActivityPeriod Wait(Project project, TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return Period(project, PeriodKind.Waiting, startAgo, endAgo);
    }

    /// <summary>
    /// Method used to get the first invented project
    /// </summary>
    /// <returns>A valid project</returns>
    private static Project Alpha()
    {
        // Function's return
        return new Project("/home/dev/projects/alpha");
    }

    /// <summary>
    /// Method used to get the second invented project
    /// </summary>
    /// <returns>A valid project</returns>
    private static Project Beta()
    {
        // Function's return
        return new Project("/home/dev/projects/beta");
    }

    /// <summary>
    /// Method used to get the third invented project
    /// </summary>
    /// <returns>A valid project</returns>
    private static Project Gamma()
    {
        // Function's return
        return new Project("/home/dev/projects/gamma");
    }

    /// <summary>
    /// Method used to build settings where only the three values of "overloaded" are chosen
    /// </summary>
    /// <param name="window">Window of "overloaded"</param>
    /// <param name="minimumProjects">Minimum number of projects</param>
    /// <param name="maximumBreak">Break that ends "never stops"</param>
    /// <returns>The built settings</returns>
    private static StateSettings Settings(TimeSpan window, int minimumProjects, TimeSpan maximumBreak)
    {
        // Function's return
        return new StateSettings(window, minimumProjects, maximumBreak, TimeSpan.FromDays(7), 0.9);
    }

    #endregion

    #region Publics

    /// <summary>
    /// At the desk: one project, busy for the whole window, is not overloaded (busy, but not spread over projects)
    /// </summary>
    [Fact]
    public void AgentAtTheDeskOnOneProjectIsNotOverloaded()
    {
        // Building two hours of work on a single project
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(Alpha(), Ago(120), Ago(0)) };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Chaining without a break: two projects over the whole window, with breaks shorter than 10 minutes, is overloaded
    /// </summary>
    [Fact]
    public void AgentChainingTwoProjectsWithoutBreakIsOverloaded()
    {
        // Building two projects one after the other with a 5 minutes break
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(65)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Waiting: a waiting period of 15 minutes in the middle of a chain is a break, so the agent is not overloaded
    /// </summary>
    [Fact]
    public void WaitingPeriodOfFifteenMinutesInTheMiddleOfAChainIsNotOverloaded()
    {
        // Building two projects with 15 minutes of waiting in between
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(75)),
            Wait(Alpha(), Ago(75), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A short waiting period is a short break: it does not stop the chain by itself
    /// </summary>
    [Fact]
    public void WaitingPeriodOfFiveMinutesInTheMiddleOfAChainIsOverloaded()
    {
        // Building two projects with 5 minutes of waiting in between
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(65)),
            Wait(Alpha(), Ago(65), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A waiting period at the end of the window is a break between the last work and now
    /// </summary>
    [Fact]
    public void WaitingPeriodUntilNowIsABreakAtTheEnd()
    {
        // Building two projects, then 20 minutes of waiting until now
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(20)),
            Wait(Beta(), Ago(20), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A waiting period that overlaps work does not remove the work: the time stays worked
    /// </summary>
    [Fact]
    public void WaitingPeriodOverlappingWorkDoesNotCreateABreak()
    {
        // Building a chain of two projects with a waiting period inside the first one
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Wait(Alpha(), Ago(100), Ago(80)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A project that is only waited on is not a project worked on
    /// </summary>
    [Fact]
    public void WaitingPeriodOfAnotherProjectIsNotAProjectWorkedOn()
    {
        // Building work on one project and waiting on another, over the whole window
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(0)),
            Wait(Beta(), Ago(100), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Waiting periods only: nothing was worked, so the agent is not overloaded
    /// </summary>
    [Fact]
    public void OnlyWaitingPeriodsAreNotOverloaded()
    {
        // Building waiting periods on two projects
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Wait(Alpha(), Ago(120), Ago(60)),
            Wait(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Resting: an agent without any period is not overloaded
    /// </summary>
    [Fact]
    public void RestingAgentWithoutPeriodIsNotOverloaded()
    {
        // Checking the rule on an empty list
        AgentStateRules.IsOverloaded(new List<ActivityPeriod>(), Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Resting: an agent whose periods are all older than the window is not overloaded
    /// </summary>
    [Fact]
    public void RestingAgentWithOnlyOldPeriodsIsNotOverloaded()
    {
        // Building two projects worked on a day ago
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(1500), Ago(1450)),
            Work(Beta(), Ago(1450), Ago(1400)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A break between two periods of work ends "never stops" when it lasts 10 minutes or more (strictly shorter is overloaded)
    /// </summary>
    /// <param name="breakSeconds">Length of the break between the two projects, in seconds</param>
    /// <param name="expected">Expected result of the rule</param>
    [Theory]
    [InlineData(1, true)]
    [InlineData(300, true)]
    [InlineData(599, true)]
    [InlineData(600, false)]
    [InlineData(601, false)]
    [InlineData(3600, false)]
    public void BreakBetweenTwoPeriodsIsComparedStrictlyToTenMinutes(int breakSeconds, bool expected)
    {
        // Building two projects separated by the break (the first ends 65 minutes before now)
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(65)),
            Work(Beta(), Ago(65) - TimeSpan.FromSeconds(breakSeconds), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBe(expected);
    }

    /// <summary>
    /// The time between the start of the window and the first period is a break too
    /// </summary>
    /// <param name="breakSeconds">Length of the break at the start of the window, in seconds</param>
    /// <param name="expected">Expected result of the rule</param>
    [Theory]
    [InlineData(0, true)]
    [InlineData(599, true)]
    [InlineData(600, false)]
    [InlineData(601, false)]
    [InlineData(3600, false)]
    public void BreakAtTheStartOfTheWindowCounts(int breakSeconds, bool expected)
    {
        // Building two projects without break between them, the first one starting after the start of the window
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120) - TimeSpan.FromSeconds(breakSeconds), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBe(expected);
    }

    /// <summary>
    /// The time between the last period and now is a break too
    /// </summary>
    /// <param name="breakSeconds">Length of the break before now, in seconds</param>
    /// <param name="expected">Expected result of the rule</param>
    [Theory]
    [InlineData(0, true)]
    [InlineData(599, true)]
    [InlineData(600, false)]
    [InlineData(601, false)]
    [InlineData(3600, false)]
    public void BreakAtTheEndBeforeNowCounts(int breakSeconds, bool expected)
    {
        // Building two projects without break between them, the last one ending before now
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), TimeSpan.FromSeconds(breakSeconds)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBe(expected);
    }

    /// <summary>
    /// The longest break decides, not the sum of the breaks: two breaks of 9 minutes are less than 10 minutes each
    /// </summary>
    [Fact]
    public void SeveralShortBreaksWhoseSumIsLongAreOverloaded()
    {
        // Building a chain with two breaks of 9 minutes (18 minutes in total)
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(90)),
            Work(Beta(), Ago(81), Ago(50)),
            Work(Alpha(), Ago(41), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// One long break is enough to stop, even when the other breaks are short
    /// </summary>
    [Fact]
    public void OneLongBreakAmongShortBreaksIsNotOverloaded()
    {
        // Building a chain with a break of 5 minutes and a break of 12 minutes
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(90)),
            Work(Beta(), Ago(85), Ago(50)),
            Work(Alpha(), Ago(38), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Two periods that touch (the second starts when the first ends) have no break between them
    /// </summary>
    [Fact]
    public void TouchingPeriodsHaveNoBreak()
    {
        // Building two projects one right after the other
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Periods that overlap are merged: there is no break between them
    /// </summary>
    [Fact]
    public void OverlappingPeriodsAreMerged()
    {
        // Building two projects in parallel for part of the window
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(30)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period inside a longer one does not hide the overlap of the next one: the merge uses the latest end seen so far
    /// </summary>
    [Fact]
    public void PeriodNestedInAnotherOneDoesNotCreateABreak()
    {
        // Building a long period, a short one inside it, and a third one that starts before the end of the long one
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(50)),
            Work(Beta(), Ago(100), Ago(90)),
            Work(Gamma(), Ago(55), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The order of the list does not matter
    /// </summary>
    /// <param name="breakSeconds">Length of the break between the two projects, in seconds</param>
    /// <param name="expected">Expected result of the rule</param>
    [Theory]
    [InlineData(599, true)]
    [InlineData(600, false)]
    public void OrderOfThePeriodsDoesNotMatter(int breakSeconds, bool expected)
    {
        // Building two projects separated by the break, the latest period first
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Beta(), Ago(65) - TimeSpan.FromSeconds(breakSeconds), Ago(0)),
            Work(Alpha(), Ago(120), Ago(65)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBe(expected);
    }

    /// <summary>
    /// Exactly two projects are enough, because the rule is "at least two"
    /// </summary>
    [Fact]
    public void ExactlyTwoProjectsAreEnough()
    {
        // Building two projects without break
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule with the default minimum of two
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Several periods on the same project, with short breaks, are still one project
    /// </summary>
    [Fact]
    public void SeveralPeriodsOnTheSameProjectAreOneProject()
    {
        // Building three periods on the same project with short breaks
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(90)),
            Work(Alpha(), Ago(85), Ago(40)),
            Work(Alpha(), Ago(35), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Two project objects with the same path are the same project
    /// </summary>
    [Fact]
    public void ProjectsWithTheSamePathAreCountedOnce()
    {
        // Building two periods whose projects are two objects with the same path
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(new Project("/home/dev/projects/alpha"), Ago(120), Ago(60)),
            Work(new Project("/home/dev/projects/alpha/"), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A period that started before the window counts for its part inside the window: its project is counted and it covers the start
    /// </summary>
    [Fact]
    public void PeriodStartedBeforeTheWindowIsCutAtTheStartOfTheWindow()
    {
        // Building a first period that started three hours ago, followed by a second project
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(180), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period entirely before the window is ignored: its project is not counted
    /// </summary>
    [Fact]
    public void PeriodEntirelyBeforeTheWindowIsIgnored()
    {
        // Building an old period on a first project, then two hours of work on a second one
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(180), Ago(150)),
            Work(Beta(), Ago(120), Ago(0)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A period that ends after now counts for its part inside the window: its project is counted and it covers the end
    /// </summary>
    [Fact]
    public void PeriodEndingAfterNowIsCutAtNow()
    {
        // Building a second period that ends in thirty minutes
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(-30)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period that starts after now is ignored: its project is not counted
    /// </summary>
    [Fact]
    public void PeriodStartingAfterNowIsIgnored()
    {
        // Building two hours of work on a first project and a period in the future on a second one
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(0)),
            Work(Beta(), Ago(-10), Ago(-60)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A period in the future does not count as a project, even when the minimum is three
    /// </summary>
    [Fact]
    public void PeriodInTheFutureIsNotCountedAsAProject()
    {
        // Building two projects now and a third one in the future, with a minimum of three projects
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
            Work(Gamma(), Ago(-5), Ago(-30)),
        };

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10))).ShouldBeFalse();
    }

    /// <summary>
    /// A period that ends exactly at the start of the window only touches it: its project is not counted
    /// </summary>
    [Fact]
    public void PeriodEndingExactlyAtTheStartOfTheWindowIsNotAProject()
    {
        // Building two projects in the window and a third one that ends exactly where the window starts
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Gamma(), Ago(150), Ago(120)),
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule with a minimum of three projects
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10))).ShouldBeFalse();
    }

    /// <summary>
    /// A period that starts exactly now only touches the window: its project is not counted
    /// </summary>
    [Fact]
    public void PeriodStartingExactlyNowIsNotAProject()
    {
        // Building two projects in the window and a third one that starts exactly now
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
            Work(Gamma(), Ago(0), Ago(-30)),
        };

        // Checking the rule with a minimum of three projects
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10))).ShouldBeFalse();
    }

    /// <summary>
    /// A period that lasts zero inside the window is not a project
    /// </summary>
    [Fact]
    public void PeriodOfZeroDurationIsNotAProject()
    {
        // Building two projects in the window and a third one of a single instant in the middle
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
            Work(Gamma(), Ago(30), Ago(30)),
        };

        // Checking the rule with a minimum of three projects
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10))).ShouldBeFalse();
    }

    /// <summary>
    /// Positive control of the three tests above: one second inside the window is enough to count a project
    /// </summary>
    [Fact]
    public void PeriodWithOneSecondInsideTheWindowIsAProject()
    {
        // Building two projects in the window and a third one that ends one second after the start of the window
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Gamma(), Ago(150), Ago(119, 59)),
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule with a minimum of three projects
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10))).ShouldBeTrue();
    }

    /// <summary>
    /// The result depends on "now": the same periods are overloading at one time and old at another
    /// </summary>
    [Fact]
    public void NowIsAParameterOfTheRule()
    {
        // Building a chain that ends at the fixed now
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };

        // Checking the rule at the fixed now, then three hours later
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
        AgentStateRules.IsOverloaded(periods, Now().AddHours(3), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A very long list of tiny periods that chain two projects without a break is overloaded
    /// </summary>
    [Fact]
    public void TenThousandTouchingPeriodsAreOverloaded()
    {
        // Building 10000 touching periods that cover the window exactly, alternating two projects
        TimeSpan window = TimeSpan.FromHours(2);
        long step = window.Ticks / 10000;
        List<ActivityPeriod> periods = new List<ActivityPeriod>();
        for (int index = 0; index < 10000; index++)
        {
            DateTimeOffset start = Now() - window + TimeSpan.FromTicks(step * index);
            periods.Add(new ActivityPeriod(index % 2 == 0 ? Alpha() : Beta(), PeriodKind.Working, start, start + TimeSpan.FromTicks(step)));
        }

        // Checking the rule
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The window of the settings is used: with a window of one hour, work older than one hour is ignored
    /// </summary>
    [Fact]
    public void CustomWindowChangesTheResult()
    {
        // Building a chain whose first project ended seventy minutes ago
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(70)),
            Work(Beta(), Ago(70), Ago(0)),
        };

        // Checking the rule with the default window of two hours, then with a window of one hour
        AgentStateRules.IsOverloaded(periods, Now(), StateSettings.Default).ShouldBeTrue();
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(1), 2, TimeSpan.FromMinutes(10))).ShouldBeFalse();
    }

    /// <summary>
    /// The minimum number of projects of the settings is used
    /// </summary>
    [Fact]
    public void CustomMinimumProjectsChangesTheResult()
    {
        // Building a chain of two projects, then a chain of three projects
        List<ActivityPeriod> twoProjects = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(60)),
            Work(Beta(), Ago(60), Ago(0)),
        };
        List<ActivityPeriod> threeProjects = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(80)),
            Work(Beta(), Ago(80), Ago(40)),
            Work(Gamma(), Ago(40), Ago(0)),
        };
        StateSettings three = Settings(TimeSpan.FromHours(2), 3, TimeSpan.FromMinutes(10));

        // Checking the rule with a minimum of three projects
        AgentStateRules.IsOverloaded(twoProjects, Now(), three).ShouldBeFalse();
        AgentStateRules.IsOverloaded(threeProjects, Now(), three).ShouldBeTrue();
    }

    /// <summary>
    /// A minimum of one project makes one project enough
    /// </summary>
    [Fact]
    public void MinimumOfOneProjectMakesAnAgentAtTheDeskOverloaded()
    {
        // Building two hours of work on a single project
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(Alpha(), Ago(120), Ago(0)) };

        // Checking the rule with a minimum of one project
        AgentStateRules.IsOverloaded(periods, Now(), Settings(TimeSpan.FromHours(2), 1, TimeSpan.FromMinutes(10))).ShouldBeTrue();
    }

    /// <summary>
    /// The maximum break of the settings is used, in both directions
    /// </summary>
    [Fact]
    public void CustomMaximumBreakChangesTheResult()
    {
        // Building two projects separated by a break of nine minutes, then ten minutes
        List<ActivityPeriod> nineMinutes = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(65)),
            Work(Beta(), Ago(56), Ago(0)),
        };
        List<ActivityPeriod> tenMinutes = new List<ActivityPeriod>
        {
            Work(Alpha(), Ago(120), Ago(65)),
            Work(Beta(), Ago(55), Ago(0)),
        };

        // Checking the rule with a limit of five minutes, then of fifteen minutes
        StateSettings five = Settings(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(5));
        StateSettings fifteen = Settings(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(15));
        AgentStateRules.IsOverloaded(nineMinutes, Now(), five).ShouldBeFalse();
        AgentStateRules.IsOverloaded(nineMinutes, Now(), fifteen).ShouldBeTrue();
        AgentStateRules.IsOverloaded(tenMinutes, Now(), fifteen).ShouldBeTrue();
    }

    /// <summary>
    /// The list of periods is required (an empty list is allowed, a missing list is not)
    /// </summary>
    [Fact]
    public void OverloadedNeedsAListOfPeriods()
    {
        // Checking the rule without list
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.IsOverloaded(null!, Now(), StateSettings.Default));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// The list of periods cannot contain a null element
    /// </summary>
    [Fact]
    public void OverloadedPeriodsCannotContainNull()
    {
        // Checking the rule with a null period in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsOverloaded(new List<ActivityPeriod> { null! }, Now(), StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// A null period is refused even when it comes after valid periods
    /// </summary>
    [Fact]
    public void OverloadedPeriodsCannotContainNullAfterValidPeriods()
    {
        // Checking the rule with a null period after a valid one
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsOverloaded(
                new List<ActivityPeriod> { Work(Alpha(), Ago(120), Ago(0)), null! }, Now(), StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// The "now" of the rule must be expressed in UTC
    /// </summary>
    [Fact]
    public void OverloadedNowMustBeUtc()
    {
        // Checking the rule with a "now" that is not in UTC
        DateTimeOffset nonUtcNow = new DateTimeOffset(2026, 10, 30, 14, 0, 0, TimeSpan.FromHours(2));
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsOverloaded(new List<ActivityPeriod>(), nonUtcNow, StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("nowUtc");
    }

    /// <summary>
    /// The settings of the rule are required
    /// </summary>
    [Fact]
    public void OverloadedNeedsSettings()
    {
        // Checking the rule without settings
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.IsOverloaded(new List<ActivityPeriod>(), Now(), null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("settings");
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
