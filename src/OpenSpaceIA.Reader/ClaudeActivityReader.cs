/*
 *	@file	src/OpenSpaceIA.Reader/ClaudeActivityReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Adapter of the read port of the core: the activity of this machine, read in the files of Claude Code
 */

// Using
using OpenSpaceIA.Core.Model;
using OpenSpaceIA.Core.Ports.Read;
using OpenSpaceIA.Core.States;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Adapter of the read port of the core: the activity of this machine, read in the files of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the only door of the Reader: the desktop (and later the collector) asks it for a snapshot, and
/// never sees a file (doc/analyse/roadmap.md, 0.4.0).</para>
/// <para><b>Behavior:</b> each snapshot reads, in this order, the agent definitions, the open sessions, the reasons of their
/// waits, the sub-agent runs and the periods of the chief, over the window of history of the settings (the longer of the
/// two windows of the state rules), with now given by the clock. A session whose status is not written yet (the first
/// second of a session) is left out without a problem: it appears in the next snapshot (decision of Michaël, 2026-10-31). A
/// waiting session gets the reason read in its transcript. The model and the task list of a session are not read yet.
/// The agent types are the definitions of the user, then the types seen only in the runs. Every problem of every part is
/// given with the snapshot, without any content of a file. The reading runs outside the thread of the caller, one at a time:
/// the readers keep what they read between two snapshots, in memory only. Nothing is ever written.</para>
/// </remarks>
public sealed class ClaudeActivityReader : IActivityReader
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Lock that makes the readings one at a time, since the readers keep a state between them
    /// </summary>
    private readonly object _gate = new object();

    /// <summary>
    /// Machine whose activity is read
    /// </summary>
    private readonly Machine _machine;

    /// <summary>
    /// Clock that gives now
    /// </summary>
    private readonly TimeProvider _clock;

    /// <summary>
    /// Settings of the state rules, which give the window of history
    /// </summary>
    private readonly StateSettings _settings;

    /// <summary>
    /// Reader of the agent definitions
    /// </summary>
    private readonly AgentDefinitionReader _agentDefinitions;

    /// <summary>
    /// Reader of the open sessions
    /// </summary>
    private readonly LiveSessionReader _sessions;

    /// <summary>
    /// Reader of the reasons of the waits
    /// </summary>
    private readonly WaitReasonReader _waitReasons;

    /// <summary>
    /// Reader of the sub-agent runs
    /// </summary>
    private readonly SubagentRunReader _runs;

    /// <summary>
    /// Reader of the periods of the chief
    /// </summary>
    private readonly ChiefPeriodReader _chiefPeriods;

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
    /// Constructor reading the folder of Claude Code of the current user, on this machine, with the clock of the system and
    /// the default settings
    /// </summary>
    public ClaudeActivityReader()
        : this(ClaudeFolder.FromUserProfile(), new SystemProcessProbe(), TimeProvider.System, StateSettings.Default, new Machine(Environment.MachineName))
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="folder">[Cannot be null] Folder of Claude Code</param>
    /// <param name="processProbe">[Cannot be null] Port used to know whether the process of a session still runs</param>
    /// <param name="clock">[Cannot be null] Clock that gives now</param>
    /// <param name="settings">[Cannot be null] Settings of the state rules, which give the window of history</param>
    /// <param name="machine">[Cannot be null] Machine whose activity is read</param>
    /// <exception cref="ArgumentNullException">If an argument is null</exception>
    internal ClaudeActivityReader(ClaudeFolder folder, IProcessProbe processProbe, TimeProvider clock, StateSettings settings, Machine machine)
    {
        // Checks
        if (folder is null)
            throw new ArgumentNullException(nameof(folder), $"The value of the parameter: {nameof(folder)} cannot be null");
        if (processProbe is null)
            throw new ArgumentNullException(nameof(processProbe), $"The value of the parameter: {nameof(processProbe)} cannot be null");
        if (clock is null)
            throw new ArgumentNullException(nameof(clock), $"The value of the parameter: {nameof(clock)} cannot be null");
        if (settings is null)
            throw new ArgumentNullException(nameof(settings), $"The value of the parameter: {nameof(settings)} cannot be null");
        if (machine is null)
            throw new ArgumentNullException(nameof(machine), $"The value of the parameter: {nameof(machine)} cannot be null");

        // Building the readers on one reader of the files
        ClaudeFileReader fileReader = new ClaudeFileReader(folder);
        _machine = machine;
        _clock = clock;
        _settings = settings;
        _agentDefinitions = new AgentDefinitionReader(fileReader);
        _sessions = new LiveSessionReader(fileReader, processProbe);
        _waitReasons = new WaitReasonReader(fileReader);
        _runs = new SubagentRunReader(fileReader);
        _chiefPeriods = new ChiefPeriodReader(fileReader);
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to turn the problems of a reader into problems of the core
    /// </summary>
    /// <param name="problems">[Cannot be null] Problems of a reader</param>
    /// <returns>The same problems, for the core</returns>
    private static IEnumerable<ReadingProblem> ToCore(IEnumerable<ReadProblem> problems)
    {
        // Function's return
        return problems.Select(problem => new ReadingProblem(problem.FilePath, problem.Reason));
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the activity of the machine at this moment, on the thread of the caller
    /// </summary>
    /// <param name="cancellationToken">Token used to stop the reading between two of its parts</param>
    /// <returns>The activity of the machine at the moment of the reading</returns>
    /// <exception cref="OperationCanceledException">If the reading is cancelled</exception>
    internal ActivitySnapshot Read(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            // Reading now and the start of the window of history
            DateTimeOffset nowUtc = _clock.GetUtcNow().ToUniversalTime();
            TimeSpan window = _settings.UnderUsedWindow > _settings.OverloadedWindow ? _settings.UnderUsedWindow : _settings.OverloadedWindow;
            DateTimeOffset sinceUtc = nowUtc - window;

            // Reading each part, stopping between two parts when asked
            cancellationToken.ThrowIfCancellationRequested();
            AgentDefinitions definitions = _agentDefinitions.Read();
            cancellationToken.ThrowIfCancellationRequested();
            LiveSessions live = _sessions.Read();
            WaitReasons waits = _waitReasons.Read(live.Sessions);
            cancellationToken.ThrowIfCancellationRequested();
            SubagentRuns runs = _runs.Read(sinceUtc, nowUtc);
            cancellationToken.ThrowIfCancellationRequested();
            ChiefPeriods chief = _chiefPeriods.Read(sinceUtc, nowUtc);

            // Building the sessions whose status is known, a waiting one with its reason
            List<Session> sessions = new List<Session>();
            foreach (SessionFile facts in live.Sessions.Select(session => session.Facts).Where(facts => facts.Status is not null))
            {
                SessionStatus status = facts.Status!.Value;
                WaitReason? reason = status == SessionStatus.Waiting
                    ? waits.Reasons.TryGetValue(facts.SessionId, out WaitReason read) ? read : WaitReason.Question
                    : null;
                sessions.Add(new Session(facts.SessionId, facts.Project, status, reason, facts.StartedAtUtc, null, null));
            }

            // Gathering the agent types and the problems
            IReadOnlyList<AgentType> agentTypes = RunActivity.AddAgentTypesOfRuns(definitions.AgentTypes, runs.Runs);
            List<ReadingProblem> problems = ToCore(definitions.Problems).Concat(ToCore(live.Problems)).Concat(ToCore(waits.Problems))
                .Concat(ToCore(runs.Problems)).Concat(ToCore(chief.Problems)).ToList();

            // Function's return
            return new ActivitySnapshot(_machine, nowUtc, sessions, agentTypes, runs.Runs, chief.Periods, problems);
        }
    }

    /// <inheritdoc/>
    public Task<ActivitySnapshot> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        // Reading the files outside the thread of the caller, which may be the thread of a screen
        Task<ActivitySnapshot> reading = Task.Run(() => Read(cancellationToken), cancellationToken);

        // Function's return
        return reading;
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
