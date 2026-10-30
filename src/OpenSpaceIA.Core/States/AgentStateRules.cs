/*
 *	@file	src/OpenSpaceIA.Core/States/AgentStateRules.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Rules that compute the state of an agent, "overloaded" and "under-used"
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.States;

/// <summary>
/// Service used to compute the state of an agent, and whether it is overloaded or under-used
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the rules of doc/analyse/state-definitions.md, in one place, so that every screen shows the same
/// judgment of the workload.</para>
/// <para><b>Behavior:</b> pure functions: no state, no clock, no file. "Now" and the settings are parameters, so that every
/// rule gives the same answer for the same inputs.</para>
/// </remarks>
public static class AgentStateRules
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
    /// Method used to get the start of a window that ends now
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> every rule looks at a sliding window that ends now.</para>
    /// <para><b>Behavior:</b> a window longer than the time since the smallest date that can be written starts at that date,
    /// instead of failing with an overflow.</para>
    /// </remarks>
    /// <param name="nowUtc">Now, in UTC</param>
    /// <param name="window">Length of the window, greater than zero</param>
    /// <returns>The start of the window, in UTC</returns>
    private static DateTimeOffset GetWindowStartUtc(DateTimeOffset nowUtc, TimeSpan window)
    {
        // Function's return
        return nowUtc - DateTimeOffset.MinValue < window ? DateTimeOffset.MinValue : nowUtc - window;
    }

    /// <summary>
    /// Method used to know whether a period has a part of more than zero inside a window
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the only test of "this period counts in the window", shared by every rule
    /// (doc/analyse/state-definitions.md, § 6).</para>
    /// <para><b>Behavior:</b> true when the period starts before now, ends after the start of the window, and lasts more than
    /// zero. A period that only touches the window (it ends exactly at its start, or starts exactly now) or that lasts zero
    /// is ignored, so it adds no project.</para>
    /// </remarks>
    /// <param name="period">[Cannot be null] Period to test, already checked</param>
    /// <param name="windowStartUtc">Start of the window, in UTC</param>
    /// <param name="nowUtc">End of the window (now), in UTC</param>
    /// <returns>
    /// True  ==> The period counts in the window<br/>
    /// False ==> The period is ignored
    /// </returns>
    private static bool HasPartInWindow(ActivityPeriod period, DateTimeOffset windowStartUtc, DateTimeOffset nowUtc)
    {
        // Function's return
        return period.StartUtc < nowUtc && period.EndUtc > windowStartUtc && period.EndUtc > period.StartUtc;
    }

    /// <summary>
    /// Method used to cut a period to a window
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> only the part of a period inside the window counts (doc/analyse/state-definitions.md, § 6).</para>
    /// <para><b>Behavior:</b> a start before the window becomes the start of the window; an end after now becomes now.</para>
    /// </remarks>
    /// <param name="period">[Cannot be null] Period that has a part inside the window</param>
    /// <param name="windowStartUtc">Start of the window, in UTC</param>
    /// <param name="nowUtc">End of the window (now), in UTC</param>
    /// <returns>The part of the period inside the window</returns>
    private static TimeRange CutToWindow(ActivityPeriod period, DateTimeOffset windowStartUtc, DateTimeOffset nowUtc)
    {
        // Keeping the later start and the earlier end
        DateTimeOffset startUtc = period.StartUtc < windowStartUtc ? windowStartUtc : period.StartUtc;
        DateTimeOffset endUtc = period.EndUtc > nowUtc ? nowUtc : period.EndUtc;

        // Function's return
        return new TimeRange(startUtc, endUtc);
    }

    /// <summary>
    /// Method used to merge the ranges that overlap or touch
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> a time is never counted twice, even when runs of the same agent type go in parallel
    /// (doc/analyse/state-definitions.md, § 5).</para>
    /// <para><b>Behavior:</b> the ranges are sorted by start; a range that starts before or when the previous one ends is
    /// merged into it, even when it is inside it.</para>
    /// </remarks>
    /// <param name="ranges">[Cannot be null] Ranges to merge, in any order</param>
    /// <returns>The merged ranges, sorted by start, without overlap</returns>
    private static List<TimeRange> Merge(IEnumerable<TimeRange> ranges)
    {
        // Merging the ranges, sorted by start
        List<TimeRange> merged = new List<TimeRange>();
        foreach (TimeRange range in ranges.OrderBy(range => range.StartUtc))
        {
            if (merged.Count > 0 && range.StartUtc <= merged[merged.Count - 1].EndUtc)
            {
                // Extending the last range, which may already end later (a range inside another one)
                TimeRange last = merged[merged.Count - 1];
                merged[merged.Count - 1] = new TimeRange(last.StartUtc, range.EndUtc > last.EndUtc ? range.EndUtc : last.EndUtc);
            }
            else
            {
                // Starting a new range after a break
                merged.Add(range);
            }
        }

        // Function's return
        return merged;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to get the state of the chief (a main session) at this moment
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> places the chief at its desk, at the coffee machine or in the lounge
    /// (doc/analyse/state-definitions.md, § 2).</para>
    /// <para><b>Behavior:</b> busy gives working, waiting gives waiting for you, idle gives idle.</para>
    /// </remarks>
    /// <param name="session">[Cannot be null] Main session of the chief</param>
    /// <returns>The state of the chief</returns>
    /// <exception cref="ArgumentNullException">If the session is null</exception>
    public static AgentState GetChiefState(Session session)
    {
        // Check
        ModelGuard.RequireNotNull(session, nameof(session));

        // Function's return
        return session.Status switch
        {
            SessionStatus.Busy => AgentState.Working,
            SessionStatus.Waiting => AgentState.WaitingForYou,
            _ => AgentState.Idle,
        };
    }

    /// <summary>
    /// Method used to get the state of an agent type at this moment
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> places the character of an agent type at its desk or in the lounge
    /// (doc/analyse/state-definitions.md, § 2).</para>
    /// <para><b>Behavior:</b> working when at least one run of this agent type (same name, case sensitive) is not finished;
    /// idle otherwise. Never waiting for you: the runs of the sub-agents do not wait for the user. The whole list is checked
    /// before the answer.</para>
    /// </remarks>
    /// <param name="agentType">[Cannot be null] Agent type to judge</param>
    /// <param name="runs">[Cannot be null] Runs of the sub-agents, of any agent type</param>
    /// <returns>
    /// Working ==> At least one run of the agent type is in progress<br/>
    /// Idle    ==> No run of the agent type is in progress
    /// </returns>
    /// <exception cref="ArgumentNullException">If the agent type or the list of runs is null</exception>
    /// <exception cref="ArgumentException">If the list of runs contains a null element</exception>
    public static AgentState GetAgentTypeState(AgentType agentType, IReadOnlyList<AgentRun> runs)
    {
        // Checks
        ModelGuard.RequireNotNull(agentType, nameof(agentType));
        IReadOnlyList<AgentRun> checkedRuns = ModelGuard.CopyWithoutNull(runs, nameof(runs));

        // Looking for a run in progress of this agent type
        bool isWorking = checkedRuns.Any(run => !run.IsFinished && string.Equals(run.AgentTypeName, agentType.Name, StringComparison.Ordinal));

        // Function's return
        return isWorking ? AgentState.Working : AgentState.Idle;
    }

    /// <summary>
    /// Method used to know whether an agent is overloaded
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> shows the agent that chains several projects and never stops (doc/analyse/state-definitions.md,
    /// § 3 and § 6).</para>
    /// <para><b>Behavior:</b> only the periods of work count, cut to the window that ends now; a period whose part inside the
    /// window lasts zero is ignored and adds no project. The agent is overloaded when it
    /// worked on at least the minimum number of different projects, and when its longest break is shorter than the maximum
    /// break. A break is any time of the window without work: between two periods, from the start of the window to the first
    /// period, and from the last period to now; a period of waiting is a break. Without work in the window, it is not
    /// overloaded.</para>
    /// </remarks>
    /// <param name="periods">[Cannot be null] Periods of the agent, in any order</param>
    /// <param name="nowUtc">Now, in UTC</param>
    /// <param name="settings">[Cannot be null] Settings of the rule</param>
    /// <returns>
    /// True  ==> The agent is overloaded<br/>
    /// False ==> The agent is not overloaded
    /// </returns>
    /// <exception cref="ArgumentNullException">If the list of periods or the settings is null</exception>
    /// <exception cref="ArgumentException">If the list of periods contains a null element, or if now is not in UTC</exception>
    public static bool IsOverloaded(IReadOnlyList<ActivityPeriod> periods, DateTimeOffset nowUtc, StateSettings settings)
    {
        // Checks
        IReadOnlyList<ActivityPeriod> checkedPeriods = ModelGuard.CopyWithoutNull(periods, nameof(periods));
        ModelGuard.RequireUtc(nowUtc, nameof(nowUtc));
        ModelGuard.RequireNotNull(settings, nameof(settings));

        // Keeping the periods of work that count in the window
        DateTimeOffset windowStartUtc = GetWindowStartUtc(nowUtc, settings.OverloadedWindow);
        List<ActivityPeriod> work = checkedPeriods
            .Where(period => period.Kind == PeriodKind.Working && HasPartInWindow(period, windowStartUtc, nowUtc))
            .ToList();

        // Refusing an agent without work in the window, or spread over too few projects
        int projectCount = work.Select(period => period.Project).Distinct().Count();
        if (work.Count == 0 || projectCount < settings.OverloadedMinimumProjects)
        {
            // Function's return
            return false;
        }

        // Measuring the longest break, the start and the end of the window included
        TimeSpan longestBreak = TimeSpan.Zero;
        DateTimeOffset breakStartUtc = windowStartUtc;
        foreach (TimeRange range in Merge(work.Select(period => CutToWindow(period, windowStartUtc, nowUtc))))
        {
            if (range.StartUtc - breakStartUtc > longestBreak)
                longestBreak = range.StartUtc - breakStartUtc;

            breakStartUtc = range.EndUtc;
        }

        if (nowUtc - breakStartUtc > longestBreak)
            longestBreak = nowUtc - breakStartUtc;

        // Function's return
        return longestBreak < settings.OverloadedMaximumBreak;
    }

    /// <summary>
    /// Method used to know whether an agent is under-used
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> shows the agent that is almost always in the lounge (doc/analyse/state-definitions.md, § 4 and
    /// § 6).</para>
    /// <para><b>Behavior:</b> the periods of work and of waiting are cut to the window that ends now and merged, so that a time
    /// is never counted twice; waiting for the user is not idle. The agent is under-used when the idle time is at least the
    /// share of the window given by the settings. The durations are compared in ticks, in decimal, so that a share written
    /// with few decimals (0.9, 0.75) is compared exactly; the share is a double, so a share like 1/3 is rounded to 15
    /// significant digits first. An agent without any period is under-used.</para>
    /// </remarks>
    /// <param name="periods">[Cannot be null] Periods of the agent, in any order</param>
    /// <param name="nowUtc">Now, in UTC</param>
    /// <param name="settings">[Cannot be null] Settings of the rule</param>
    /// <returns>
    /// True  ==> The agent is under-used<br/>
    /// False ==> The agent is not under-used
    /// </returns>
    /// <exception cref="ArgumentNullException">If the list of periods or the settings is null</exception>
    /// <exception cref="ArgumentException">If the list of periods contains a null element, or if now is not in UTC</exception>
    public static bool IsUnderUsed(IReadOnlyList<ActivityPeriod> periods, DateTimeOffset nowUtc, StateSettings settings)
    {
        // Checks
        IReadOnlyList<ActivityPeriod> checkedPeriods = ModelGuard.CopyWithoutNull(periods, nameof(periods));
        ModelGuard.RequireUtc(nowUtc, nameof(nowUtc));
        ModelGuard.RequireNotNull(settings, nameof(settings));

        // Adding the time of work and of waiting in the window, without counting a time twice
        DateTimeOffset windowStartUtc = GetWindowStartUtc(nowUtc, settings.UnderUsedWindow);
        long busyTicks = Merge(checkedPeriods
                .Where(period => HasPartInWindow(period, windowStartUtc, nowUtc))
                .Select(period => CutToWindow(period, windowStartUtc, nowUtc)))
            .Sum(range => range.Duration.Ticks);

        // Comparing the idle time with the share asked
        long windowTicks = (nowUtc - windowStartUtc).Ticks;
        decimal idleTicks = windowTicks - busyTicks;

        // Function's return
        return idleTicks >= (decimal)settings.UnderUsedIdleShare * windowTicks;
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
