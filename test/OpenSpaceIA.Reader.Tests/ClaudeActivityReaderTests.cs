/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ClaudeActivityReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the adapter of the read port: the snapshot of the activity of a machine
 */

// Using
using System.Text;
using OpenSpaceIA.Core.Model;
using OpenSpaceIA.Core.States;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the adapter of the read port: the snapshot of the activity of a machine
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that every part of the Reader ends in one coherent snapshot of the core, with its problems.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. The clock is fixed and the processes are invented; no state shared
/// between tests.</para>
/// </remarks>
public sealed class ClaudeActivityReaderTests : IDisposable
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the invented project folder
    /// </summary>
    private const string ProjectFolder = "C--invented-project";

    #endregion

    #region Variables

    /// <summary>
    /// Now, for the tests
    /// </summary>
    private static readonly DateTimeOffset _nowUtc = new DateTimeOffset(2026, 10, 31, 13, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Unique folder of the instance, parent of the invented .claude folder; deleted by Dispose
    /// </summary>
    private readonly string _workFolder;

    /// <summary>
    /// Invented .claude folder of the instance
    /// </summary>
    private readonly string _root;

    /// <summary>
    /// Invented working folder of the sessions, full on every system
    /// </summary>
    private readonly string _workingFolder;

    /// <summary>
    /// Fake port that invents the processes
    /// </summary>
    private readonly FakeProcessProbe _probe = new FakeProcessProbe();

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
    /// Constructor creating the invented .claude folder of the instance
    /// </summary>
    public ClaudeActivityReaderTests()
    {
        // Creating the folders
        _workFolder = Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(_workFolder, ".claude");
        _workingFolder = Path.Combine(_workFolder, "project");
        Directory.CreateDirectory(_root);
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to build the adapter on the invented folder, with the fixed clock and the fake processes
    /// </summary>
    /// <returns>The adapter</returns>
    private ClaudeActivityReader CreateReader()
    {
        // Function's return
        return new ClaudeActivityReader(new ClaudeFolder(_root), _probe, new FixedClock(_nowUtc), StateSettings.Default, new Machine("invented-machine"));
    }

    /// <summary>
    /// Method used to write a text file in UTF-8 without BOM under the invented root, creating its folders
    /// </summary>
    /// <param name="relativePath">[Cannot be null] Path of the file, relative to the root</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    private void WriteText(string relativePath, string text)
    {
        // Creating the folders and the file
        string fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new UTF8Encoding(false).GetBytes(text));
    }

    /// <summary>
    /// Method used to write a session file
    /// </summary>
    /// <param name="processId">ID of the invented process</param>
    /// <param name="sessionId">[Cannot be null] Identifier of the session</param>
    /// <param name="status">[Can be null] Status written, or null for none</param>
    private void WriteSession(int processId, string sessionId, string? status)
    {
        // Building the text with an escaped working folder, and the status when there is one
        string workingFolder = _workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal);
        string statusPart = status is null ? string.Empty : $",\"status\":\"{status}\"";
        WriteText(Path.Combine("sessions", $"{processId}.json"),
            $"{{\"pid\":{processId},\"sessionId\":\"{sessionId}\",\"cwd\":\"{workingFolder}\",\"startedAt\":1793448000000,\"procStart\":\"1\"{statusPart}}}");
        _probe.Answers[processId] = ProcessLiveness.Alive;
    }

    #endregion

    #region Publics

    /// <summary>
    /// The arguments of the adapter are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Building the adapter without each argument
        ClaudeFolder folder = new ClaudeFolder(_root);
        FixedClock clock = new FixedClock(_nowUtc);
        Machine machine = new Machine("invented-machine");
        Should.Throw<ArgumentNullException>(() => new ClaudeActivityReader(null!, _probe, clock, StateSettings.Default, machine)).ParamName.ShouldBe("folder");
        Should.Throw<ArgumentNullException>(() => new ClaudeActivityReader(folder, null!, clock, StateSettings.Default, machine)).ParamName.ShouldBe("processProbe");
        Should.Throw<ArgumentNullException>(() => new ClaudeActivityReader(folder, _probe, null!, StateSettings.Default, machine)).ParamName.ShouldBe("clock");
        Should.Throw<ArgumentNullException>(() => new ClaudeActivityReader(folder, _probe, clock, null!, machine)).ParamName.ShouldBe("settings");
        Should.Throw<ArgumentNullException>(() => new ClaudeActivityReader(folder, _probe, clock, StateSettings.Default, null!)).ParamName.ShouldBe("machine");
    }

    /// <summary>
    /// An empty folder of Claude Code gives an empty snapshot of the machine, taken now
    /// </summary>
    [Fact]
    public async Task EmptyFolderGivesAnEmptySnapshotAsync()
    {
        // Reading the snapshot
        ActivitySnapshot snapshot = await CreateReader().ReadSnapshotAsync(CancellationToken.None);

        // Checking the snapshot
        snapshot.Machine.ShouldBe(new Machine("invented-machine"));
        snapshot.TakenAtUtc.ShouldBe(_nowUtc);
        snapshot.Sessions.ShouldBeEmpty();
        snapshot.AgentTypes.ShouldBeEmpty();
        snapshot.Runs.ShouldBeEmpty();
        snapshot.ChiefPeriods.ShouldBeEmpty();
        snapshot.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Every part of the folder ends in the snapshot: the sessions with their status and the reason of a wait, the agent
    /// types of the definitions and of the runs, the runs, and the periods of the chief; a session without status is left out
    /// </summary>
    [Fact]
    public async Task EveryPartEndsInTheSnapshotAsync()
    {
        // Writing a definition, three sessions, the transcript of the waiting one, and a run of a built-in agent
        string workingFolder = _workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal);
        WriteText(Path.Combine("agents", "helper.md"), "---\nname: helper\ndescription: Helps.\n---\n");
        WriteSession(10, "s1", "waiting");
        WriteSession(20, "s2", "busy");
        WriteSession(30, "s3", null);
        WriteText(Path.Combine("projects", ProjectFolder, "s1.jsonl"),
            $"{{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:00:00Z\",\"cwd\":\"{workingFolder}\",\"message\":{{\"content\":\"invented\"}}}}\n"
            + "{\"type\":\"assistant\",\"timestamp\":\"2026-10-31T12:05:00Z\",\"message\":{\"stop_reason\":\"tool_use\",\"content\":[{\"type\":\"tool_use\",\"id\":\"t1\",\"name\":\"AskUserQuestion\",\"input\":{}}]}}\n");
        WriteText(Path.Combine("projects", ProjectFolder, "s1", "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Explore\",\"finished\":true}");
        WriteText(Path.Combine("projects", ProjectFolder, "s1", "subagents", "agent-a1.jsonl"),
            $"{{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:01:00Z\",\"cwd\":\"{workingFolder}\"}}\n");

        // Reading the snapshot
        ActivitySnapshot snapshot = await CreateReader().ReadSnapshotAsync(CancellationToken.None);

        // Checking each part
        Project project = new Project(_workingFolder);
        DateTimeOffset started = DateTimeOffset.FromUnixTimeMilliseconds(1793448000000);
        snapshot.Sessions.ShouldBe(new List<Session>()
        {
            new Session("s1", project, SessionStatus.Waiting, WaitReason.Question, started, null, null),
            new Session("s2", project, SessionStatus.Busy, null, started, null, null),
        });
        snapshot.AgentTypes.ShouldBe(new List<AgentType>() { new AgentType("helper", "Helps.", null), new AgentType("Explore", null, null) });
        snapshot.Runs.Single().AgentTypeName.ShouldBe("Explore");
        snapshot.ChiefPeriods.ShouldBe(new List<ActivityPeriod>()
        {
            new ActivityPeriod(project, PeriodKind.Working, new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 31, 12, 5, 0, TimeSpan.Zero)),
        });
        snapshot.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// The problems of every part are given with the snapshot, as problems of the core
    /// </summary>
    [Fact]
    public async Task ProblemsOfEveryPartAreGivenAsync()
    {
        // Writing a broken definition and a broken session file
        WriteText(Path.Combine("agents", "broken.md"), "no front matter");
        WriteText(Path.Combine("sessions", "40.json"), "{ not json");

        // Reading the snapshot
        ActivitySnapshot snapshot = await CreateReader().ReadSnapshotAsync(CancellationToken.None);

        // Checking the problems
        snapshot.Problems.Select(problem => Path.GetFileName(problem.Source)).ShouldBe(new List<string>() { "broken.md", "40.json" });
    }

    /// <summary>
    /// A reading cancelled before it starts is not done
    /// </summary>
    [Fact]
    public async Task CancelledReadingIsNotDoneAsync()
    {
        // Reading with a token already cancelled
        using CancellationTokenSource cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Checking the cancellation
        await Should.ThrowAsync<OperationCanceledException>(() => CreateReader().ReadSnapshotAsync(cancellation.Token));
    }

    /// <summary>
    /// The default adapter reads the folder of the user without failing to be built
    /// </summary>
    [Fact]
    public void DefaultAdapterCanBeBuilt()
    {
        // Building the adapter of the user, without reading
        Should.NotThrow(() => new ClaudeActivityReader());
    }

    /// <summary>
    /// Method used to delete the invented folder of the instance
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> leaves no file behind in the temporary folder of the system.</para>
    /// <para><b>Behavior:</b> removes the read-only attribute of the files first, so that the deletion cannot be refused.</para>
    /// </remarks>
    public void Dispose()
    {
        // Removing the read-only attributes
        if (Directory.Exists(_workFolder))
        {
            foreach (string file in Directory.EnumerateFiles(_workFolder, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);

            // Deleting the folder
            Directory.Delete(_workFolder, true);
        }
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
