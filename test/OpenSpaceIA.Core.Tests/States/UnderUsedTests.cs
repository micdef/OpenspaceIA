/*
 *	@file	test/OpenSpaceIA.Core.Tests/States/UnderUsedTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the under-used rule
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
/// Tests of the under-used rule
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class UnderUsedTests
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
    /// Method used to get the duration of the busy time that makes exactly 10 % of the default window of seven days
    /// </summary>
    /// <returns>16 hours and 48 minutes (1008 minutes of the 10080 minutes of the window)</returns>
    private static TimeSpan TenPercentOfTheWeek()
    {
        // Function's return
        return TimeSpan.FromMinutes(1008);
    }

    /// <summary>
    /// Method used to build a period relative to "now"
    /// </summary>
    /// <param name="kind">Kind of the period</param>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period, on the first invented project</returns>
    private static ActivityPeriod Period(PeriodKind kind, TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return new ActivityPeriod(new Project("/home/dev/projects/alpha"), kind, Now() - startAgo, Now() - endAgo);
    }

    /// <summary>
    /// Method used to build a period of work relative to "now"
    /// </summary>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period</returns>
    private static ActivityPeriod Work(TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return Period(PeriodKind.Working, startAgo, endAgo);
    }

    /// <summary>
    /// Method used to build a period of waiting relative to "now"
    /// </summary>
    /// <param name="startAgo">How long before now the period starts</param>
    /// <param name="endAgo">How long before now the period ends (shorter than the start)</param>
    /// <returns>The built period</returns>
    private static ActivityPeriod Wait(TimeSpan startAgo, TimeSpan endAgo)
    {
        // Function's return
        return Period(PeriodKind.Waiting, startAgo, endAgo);
    }

    /// <summary>
    /// Method used to build settings where only the window and the idle share of "under-used" are chosen
    /// </summary>
    /// <param name="window">Window of "under-used"</param>
    /// <param name="idleShare">Idle share of "under-used"</param>
    /// <returns>The built settings</returns>
    private static StateSettings Settings(TimeSpan window, double idleShare)
    {
        // Function's return
        return new StateSettings(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), window, idleShare);
    }

    #endregion

    #region Publics

    /// <summary>
    /// Resting: an agent never called in the window is under-used
    /// </summary>
    [Fact]
    public void RestingAgentWithoutPeriodIsUnderUsed()
    {
        // Checking the rule on an empty list
        AgentStateRules.IsUnderUsed(new List<ActivityPeriod>(), Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A window longer than the time since the smallest date starts at that date instead of failing with an overflow
    /// </summary>
    [Fact]
    public void WindowLongerThanTheCalendarStartsAtTheSmallestDate()
    {
        // Building settings with the longest window, and one week of work
        StateSettings settings = Settings(TimeSpan.MaxValue, 0.5);
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromDays(7), TimeSpan.Zero) };

        // Checking that the rule answers: one week of work is nothing against two thousand years
        AgentStateRules.IsUnderUsed(periods, Now(), settings).ShouldBeTrue();
    }

    /// <summary>
    /// Resting: an agent whose periods are all older than the window is under-used
    /// </summary>
    [Fact]
    public void RestingAgentWithOnlyOldPeriodsIsUnderUsed()
    {
        // Building a long period that ended eight days ago
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromDays(9), TimeSpan.FromDays(8)) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// An agent that works all the window is not under-used
    /// </summary>
    [Fact]
    public void AgentBusyAllTheWindowIsNotUnderUsed()
    {
        // Building a period that covers the whole week
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromDays(7), TimeSpan.Zero) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// At the desk for two hours in a week is mostly idle, so under-used
    /// </summary>
    [Fact]
    public void AgentAtTheDeskTwoHoursInAWeekIsUnderUsed()
    {
        // Building two hours of work
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromHours(2), TimeSpan.Zero) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Exactly 90 % idle is under-used (the limit is inclusive), for work as well as for waiting
    /// </summary>
    /// <param name="kind">Kind of the only period</param>
    [Theory]
    [InlineData(PeriodKind.Working)]
    [InlineData(PeriodKind.Waiting)]
    public void ExactlyNinetyPercentIdleIsUnderUsed(PeriodKind kind)
    {
        // Building a period that is exactly 10 % of the window
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Period(kind, TenPercentOfTheWeek(), TimeSpan.Zero) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// One second more busy than the limit is not under-used, for work as well as for waiting
    /// </summary>
    /// <param name="kind">Kind of the only period</param>
    [Theory]
    [InlineData(PeriodKind.Working)]
    [InlineData(PeriodKind.Waiting)]
    public void OneSecondMoreBusyThanTheLimitIsNotUnderUsed(PeriodKind kind)
    {
        // Building a period that is 10 % of the window plus one second
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Period(kind, TenPercentOfTheWeek() + TimeSpan.FromSeconds(1), TimeSpan.Zero),
        };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// One second less busy than the limit is under-used
    /// </summary>
    [Fact]
    public void OneSecondLessBusyThanTheLimitIsUnderUsed()
    {
        // Building a period that is 10 % of the window minus one second
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TenPercentOfTheWeek() - TimeSpan.FromSeconds(1), TimeSpan.Zero),
        };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The busy time is the sum of several separate periods: four periods that make exactly 10 % of the window
    /// </summary>
    [Fact]
    public void SeveralSeparatePeriodsAreAdded()
    {
        // Building four periods of 252 minutes (1008 minutes in total), a day apart
        TimeSpan length = TimeSpan.FromMinutes(252);
        List<ActivityPeriod> periods = new List<ActivityPeriod>();
        for (int day = 1; day <= 4; day++)
        {
            periods.Add(Work(TimeSpan.FromDays(day) + length, TimeSpan.FromDays(day)));
        }

        // Checking the rule on the limit, then with one more second of work
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
        periods.Add(Work(TimeSpan.FromDays(6) + TimeSpan.FromSeconds(1), TimeSpan.FromDays(6)));
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Waiting for you is not idle: 20 hours of waiting in a week is more than 10 % busy
    /// </summary>
    [Fact]
    public void WaitingTimeIsNotIdleTime()
    {
        // Building 20 hours of waiting only
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Wait(TimeSpan.FromHours(20), TimeSpan.Zero) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Work and waiting are added: each alone leaves the agent under-used, together they do not
    /// </summary>
    [Fact]
    public void WorkAndWaitingAreAddedToTheBusyTime()
    {
        // Building 10 hours of work and 10 hours of waiting, one after the other
        ActivityPeriod work = Work(TimeSpan.FromHours(20), TimeSpan.FromHours(10));
        ActivityPeriod waiting = Wait(TimeSpan.FromHours(10), TimeSpan.Zero);

        // Checking the rule on each period alone, then on both
        AgentStateRules.IsUnderUsed(new List<ActivityPeriod> { work }, Now(), StateSettings.Default).ShouldBeTrue();
        AgentStateRules.IsUnderUsed(new List<ActivityPeriod> { waiting }, Now(), StateSettings.Default).ShouldBeTrue();
        AgentStateRules.IsUnderUsed(new List<ActivityPeriod> { work, waiting }, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// Periods that cover the same time are not counted twice: 10 hours twice is 10 hours, not 20
    /// </summary>
    [Fact]
    public void IdenticalOverlappingPeriodsAreNotCountedTwice()
    {
        // Building two periods of work on the same 10 hours
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(10), TimeSpan.Zero),
            Work(TimeSpan.FromHours(10), TimeSpan.Zero),
        };

        // Checking the rule: 10 hours is 6 % of the week, 20 hours would be 12 %
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period of work and a period of waiting that cover the same time are not counted twice
    /// </summary>
    [Fact]
    public void WorkAndWaitingOnTheSameTimeAreNotCountedTwice()
    {
        // Building work and waiting on the same 10 hours
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(10), TimeSpan.Zero),
            Wait(TimeSpan.FromHours(10), TimeSpan.Zero),
        };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Periods that overlap only partly are merged: 15 hours in total, not 20
    /// </summary>
    [Fact]
    public void PartlyOverlappingPeriodsAreMerged()
    {
        // Building 10 hours of work from 15 hours ago and 10 hours of waiting from 10 hours ago (5 hours in common)
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(15), TimeSpan.FromHours(5)),
            Wait(TimeSpan.FromHours(10), TimeSpan.Zero),
        };

        // Checking the rule: 15 hours is 8.9 % of the week, 20 hours would be 11.9 %
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period nested in a longer one adds nothing
    /// </summary>
    [Fact]
    public void PeriodNestedInAnotherOneAddsNothing()
    {
        // Building exactly 10 % of the window as one period, with two shorter periods inside it
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TenPercentOfTheWeek(), TimeSpan.Zero),
            Wait(TimeSpan.FromHours(8), TimeSpan.FromHours(5)),
            Work(TimeSpan.FromHours(4), TimeSpan.FromHours(3)),
        };

        // Checking the rule on the limit: any double count would put the agent above it
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// Only the part of a period inside the window counts: a period that started before the window is cut at its start
    /// </summary>
    [Fact]
    public void PeriodStartedBeforeTheWindowIsCutAtTheStartOfTheWindow()
    {
        // Building a period of 36 hours that started eight days ago, so 12 hours are inside the window
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromDays(8), TimeSpan.FromDays(6) + TimeSpan.FromHours(12)),
        };

        // Checking the rule: 12 hours is 7 % of the week, 36 hours would be 21 %
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period cut at the start of the window still counts the part that is inside the window
    /// </summary>
    [Fact]
    public void PeriodCutAtTheStartOfTheWindowStillCountsItsInsidePart()
    {
        // Building a period that started eight days ago and ended 20 hours ago (148 hours inside the window of 168 hours)
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromDays(8), TimeSpan.FromHours(20)),
        };

        // Checking the rule: about 88 % of the week is busy
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A period that ends after now counts until now only
    /// </summary>
    [Fact]
    public void PeriodEndingAfterNowIsCutAtNow()
    {
        // Building a period that started 10 hours ago and ends in 170 hours
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(10), TimeSpan.FromHours(-170)),
        };

        // Checking the rule: 10 hours is 6 % of the week, 180 hours would be more than the week
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// A period that starts after now is ignored
    /// </summary>
    [Fact]
    public void PeriodStartingAfterNowIsIgnored()
    {
        // Building a long period in the future
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(-10), TimeSpan.FromHours(-200)),
        };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The result depends on "now": the same week of work is recent at one time and old at another
    /// </summary>
    [Fact]
    public void NowIsAParameterOfTheRule()
    {
        // Building work on the whole week before the fixed now
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromDays(7), TimeSpan.Zero) };

        // Checking the rule at the fixed now, then eight days later
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
        AgentStateRules.IsUnderUsed(periods, Now().AddDays(8), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The order of the list does not matter
    /// </summary>
    [Fact]
    public void OrderOfThePeriodsDoesNotMatter()
    {
        // Building 20 hours of busy time in two periods, the latest one first
        List<ActivityPeriod> periods = new List<ActivityPeriod>
        {
            Wait(TimeSpan.FromHours(10), TimeSpan.Zero),
            Work(TimeSpan.FromHours(20), TimeSpan.FromHours(10)),
        };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
    }

    /// <summary>
    /// A very long list of tiny periods that make exactly 10 % of the window is under-used
    /// </summary>
    [Fact]
    public void TenThousandSeparatePeriodsMakingTenPercentAreUnderUsed()
    {
        // Building 10000 periods of 6.048 seconds, one every 60.48 seconds, from the start of the window (1008 minutes in total)
        TimeSpan window = TimeSpan.FromDays(7);
        long step = window.Ticks / 10000;
        long length = step / 10;
        List<ActivityPeriod> periods = new List<ActivityPeriod>();
        for (int index = 0; index < 10000; index++)
        {
            DateTimeOffset start = Now() - window + TimeSpan.FromTicks(step * index);
            periods.Add(new ActivityPeriod(new Project("/home/dev/projects/alpha"), PeriodKind.Working, start, start + TimeSpan.FromTicks(length)));
        }

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
    }

    /// <summary>
    /// The window of the settings is used: 13 hours of work is under-used in a week, not in a day
    /// </summary>
    [Fact]
    public void CustomWindowChangesTheResult()
    {
        // Building 13 hours of work
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromHours(13), TimeSpan.Zero) };

        // Checking the rule with the default window of seven days, then with a window of one day and a limit of 50 %
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeTrue();
        AgentStateRules.IsUnderUsed(periods, Now(), Settings(TimeSpan.FromDays(1), 0.5)).ShouldBeFalse();
    }

    /// <summary>
    /// The idle share of the settings is used, and its limit is inclusive: exactly 50 % idle over a day is under-used
    /// </summary>
    [Fact]
    public void CustomIdleShareLimitIsInclusive()
    {
        // Building exactly 12 hours of work, then 12 hours and one second
        List<ActivityPeriod> halfADay = new List<ActivityPeriod> { Work(TimeSpan.FromHours(12), TimeSpan.Zero) };
        List<ActivityPeriod> moreThanHalfADay = new List<ActivityPeriod>
        {
            Work(TimeSpan.FromHours(12) + TimeSpan.FromSeconds(1), TimeSpan.Zero),
        };
        StateSettings settings = Settings(TimeSpan.FromDays(1), 0.5);

        // Checking the rule on the limit and just over it
        AgentStateRules.IsUnderUsed(halfADay, Now(), settings).ShouldBeTrue();
        AgentStateRules.IsUnderUsed(moreThanHalfADay, Now(), settings).ShouldBeFalse();
    }

    /// <summary>
    /// A lower idle share limit is easier to meet: 80 % idle is under-used with a limit of 75 %, not with 90 %
    /// </summary>
    [Fact]
    public void LowerIdleShareLimitChangesTheResult()
    {
        // Building a period that makes 20 % of the week
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromMinutes(2016), TimeSpan.Zero) };

        // Checking the rule with the default limit of 90 %, then with a limit of 75 %
        AgentStateRules.IsUnderUsed(periods, Now(), StateSettings.Default).ShouldBeFalse();
        AgentStateRules.IsUnderUsed(periods, Now(), Settings(TimeSpan.FromDays(7), 0.75)).ShouldBeTrue();
    }

    /// <summary>
    /// With an idle share of 0, every agent is under-used, even one that works all the window
    /// </summary>
    [Fact]
    public void IdleShareOfZeroIsAlwaysUnderUsed()
    {
        // Building work on the whole week
        List<ActivityPeriod> periods = new List<ActivityPeriod> { Work(TimeSpan.FromDays(7), TimeSpan.Zero) };

        // Checking the rule
        AgentStateRules.IsUnderUsed(periods, Now(), Settings(TimeSpan.FromDays(7), 0.0)).ShouldBeTrue();
    }

    /// <summary>
    /// With an idle share of 1, only an agent that never works or waits is under-used
    /// </summary>
    [Fact]
    public void IdleShareOfOneNeedsNoBusyTime()
    {
        // Building one second of work, and no work at all
        List<ActivityPeriod> oneSecond = new List<ActivityPeriod> { Work(TimeSpan.FromSeconds(1), TimeSpan.Zero) };
        StateSettings settings = Settings(TimeSpan.FromDays(7), 1.0);

        // Checking the rule
        AgentStateRules.IsUnderUsed(new List<ActivityPeriod>(), Now(), settings).ShouldBeTrue();
        AgentStateRules.IsUnderUsed(oneSecond, Now(), settings).ShouldBeFalse();
    }

    /// <summary>
    /// The list of periods is required (an empty list is allowed, a missing list is not)
    /// </summary>
    [Fact]
    public void UnderUsedNeedsAListOfPeriods()
    {
        // Checking the rule without list
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.IsUnderUsed(null!, Now(), StateSettings.Default));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// The list of periods cannot contain a null element
    /// </summary>
    [Fact]
    public void UnderUsedPeriodsCannotContainNull()
    {
        // Checking the rule with a null period in the list
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsUnderUsed(new List<ActivityPeriod> { null! }, Now(), StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// A null period is refused even when it comes after valid periods
    /// </summary>
    [Fact]
    public void UnderUsedPeriodsCannotContainNullAfterValidPeriods()
    {
        // Checking the rule with a null period after a valid one
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsUnderUsed(
                new List<ActivityPeriod> { Work(TimeSpan.FromHours(1), TimeSpan.Zero), null! }, Now(), StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("periods");
    }

    /// <summary>
    /// The "now" of the rule must be expressed in UTC
    /// </summary>
    [Fact]
    public void UnderUsedNowMustBeUtc()
    {
        // Checking the rule with a "now" that is not in UTC
        DateTimeOffset nonUtcNow = new DateTimeOffset(2026, 10, 30, 14, 0, 0, TimeSpan.FromHours(2));
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => AgentStateRules.IsUnderUsed(new List<ActivityPeriod>(), nonUtcNow, StateSettings.Default));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("nowUtc");
    }

    /// <summary>
    /// The settings of the rule are required
    /// </summary>
    [Fact]
    public void UnderUsedNeedsSettings()
    {
        // Checking the rule without settings
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => AgentStateRules.IsUnderUsed(new List<ActivityPeriod>(), Now(), null!));

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
