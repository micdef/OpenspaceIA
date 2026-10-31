/*
 *	@file	test/OpenSpaceIA.Reader.Tests/LocalSmokeTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Local test that reads the real folder of Claude Code of the user and shows the state of the agents
 */

// Using
using System.Diagnostics;
using System.Globalization;
using OpenSpaceIA.Core.Model;
using OpenSpaceIA.Core.States;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Local test that reads the real folder of Claude Code of the user and shows the state of the agents
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the done criterion of 0.4.0: a local test, never run in the CI, shows the real state of your agents
/// (doc/analyse/roadmap.md, 0.4.0).</para>
/// <para><b>Behavior:</b> explicit: it never runs unless it is asked for (see test/README.md), so that a normal run of the
/// tests never reads the files of the user. It reads two snapshots with the default adapter, applies the state rules of the
/// core with the default settings, and writes a report in the output of the test: counts, names of the agent types and
/// states, times, and the problems grouped by kind. It never writes a content of a file, and it writes nothing on the
/// disk. It checks only what must hold on any machine, even a busy one between its two snapshots: the reading succeeds, one
/// agent type per name, and an agent type for every run. The report shows the names of the projects: check it before you
/// share it.</para>
/// </remarks>
public sealed class LocalSmokeTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Output of the test, where the report is written
    /// </summary>
    private readonly ITestOutputHelper _output;

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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="output">[Cannot be null] Output of the test, given by xUnit</param>
    public LocalSmokeTests(ITestOutputHelper output)
    {
        // Setting the value
        _output = output;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to write a line of the report
    /// </summary>
    /// <param name="text">[Cannot be null] Line of the report</param>
    private void Report(string text)
    {
        // Writing the line
        _output.WriteLine(text);
    }

    /// <summary>
    /// Method used to give the total length of periods, in hours
    /// </summary>
    /// <param name="periods">[Cannot be null] Periods</param>
    /// <returns>The total length, in hours, with one decimal</returns>
    private static string Hours(IEnumerable<ActivityPeriod> periods)
    {
        // Function's return
        return periods.Sum(period => (period.EndUtc - period.StartUtc).TotalHours).ToString("0.0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Method used to give the kind of a problem: its reason up to the first colon, without the path or the values after it
    /// </summary>
    /// <param name="problem">[Cannot be null] Problem of the reading</param>
    /// <returns>The kind of the problem</returns>
    private static string KindOf(ReadingProblem problem)
    {
        // Finding the first colon
        int colon = problem.Reason.IndexOf(':', StringComparison.Ordinal);

        // Function's return
        return colon < 0 ? problem.Reason : problem.Reason.Substring(0, colon);
    }

    #endregion

    #region Publics

    /// <summary>
    /// The real folder of the user gives a coherent snapshot, and the report shows the state of the agents
    /// </summary>
    /// <returns>A task that ends with the test</returns>
    [Fact(Explicit = true)]
    public async Task RealFolderShowsTheStateOfTheAgentsAsync()
    {
        // Reading two snapshots with the default adapter, and timing them
        ClaudeActivityReader reader = new ClaudeActivityReader();
        Stopwatch watch = Stopwatch.StartNew();
        ActivitySnapshot cold = await reader.ReadSnapshotAsync(TestContext.Current.CancellationToken);
        long coldMilliseconds = watch.ElapsedMilliseconds;
        watch.Restart();
        ActivitySnapshot snapshot = await reader.ReadSnapshotAsync(TestContext.Current.CancellationToken);
        long warmMilliseconds = watch.ElapsedMilliseconds;

        // Writing the report of the machine and of the times
        StateSettings settings = StateSettings.Default;
        DateTimeOffset nowUtc = snapshot.TakenAtUtc;
        Report($"Machine {snapshot.Machine.Name}, snapshot taken at {nowUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} UTC");
        Report($"Times: first snapshot {coldMilliseconds} ms, second {warmMilliseconds} ms");

        // Writing the chiefs and their state
        Report($"Chief: {snapshot.Sessions.Count} open session(s)");
        foreach (Session session in snapshot.Sessions)
            Report($"  {session.Project.Name}: {session.Status}{(session.WaitReason is null ? string.Empty : $" ({session.WaitReason})")} -> {AgentStateRules.GetChiefState(session)}");
        Report($"  periods of the window: work {Hours(snapshot.ChiefPeriods.Where(period => period.Kind == PeriodKind.Working))} h, "
            + $"waiting {Hours(snapshot.ChiefPeriods.Where(period => period.Kind == PeriodKind.Waiting))} h; "
            + $"overloaded {AgentStateRules.IsOverloaded(snapshot.ChiefPeriods, nowUtc, settings)} (under-used does not apply to the chief)");

        // Writing each agent type and its state
        Report($"Agent types: {snapshot.AgentTypes.Count}, runs of the window: {snapshot.Runs.Count} ({snapshot.Runs.Count(run => !run.IsFinished)} in progress)");
        foreach (AgentType agentType in snapshot.AgentTypes)
        {
            IReadOnlyList<ActivityPeriod> periods = RunActivity.GetPeriods(agentType.Name, snapshot.Runs);
            Report($"  {agentType.Name}: {periods.Count} run(s), {Hours(periods)} h -> {AgentStateRules.GetAgentTypeState(agentType, snapshot.Runs)}; "
                + $"overloaded {AgentStateRules.IsOverloaded(periods, nowUtc, settings)}, under-used {AgentStateRules.IsUnderUsed(periods, snapshot.ChiefPeriods, nowUtc, settings)}");
        }

        // Writing the problems, grouped by kind
        Report($"Problems: {snapshot.Problems.Count}");
        foreach (IGrouping<string, ReadingProblem> kind in snapshot.Problems.GroupBy(KindOf).OrderByDescending(group => group.Count()))
            Report($"  {kind.Count()} x {kind.Key}");

        // Checking what must hold on any machine, even a busy one: the machine, the order of the snapshots, one agent type per
        // name, and an agent type for every run
        snapshot.Machine.Name.ShouldBe(Environment.MachineName);
        snapshot.TakenAtUtc.ShouldBeGreaterThanOrEqualTo(cold.TakenAtUtc);
        snapshot.AgentTypes.Select(agentType => agentType.Name).ShouldBeUnique();
        snapshot.Runs.ShouldAllBe(run => snapshot.AgentTypes.Any(agentType => agentType.Name == run.AgentTypeName));
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
