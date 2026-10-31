/*
 *	@file	test/OpenSpaceIA.Reader.Tests/SubagentRunReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the runs of the sub-agents of Claude Code
 */

// Using
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the runs of the sub-agents of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that each run of the window becomes one run of the core, that a bad run never stops the
/// others, that the memory folders are never walked, and that a run already read is not read again while its files do not
/// change.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. Invented data only; no state shared between tests.</para>
/// </remarks>
public sealed class SubagentRunReaderTests : IDisposable
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the invented project folder
    /// </summary>
    private const string ProjectFolder = "C--invented-project";

    /// <summary>
    /// Invented identifier of the main session
    /// </summary>
    private const string SessionId = "03aa7b0f-2e6a-4eca-b924-ea679485446a";

    #endregion

    #region Variables

    /// <summary>
    /// Start of the window of the tests: every file written by the tests is newer
    /// </summary>
    private static readonly DateTimeOffset _sinceUtc = new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Now, for the tests: after every record written by the tests
    /// </summary>
    private static readonly DateTimeOffset _nowUtc = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Unique folder of the instance, parent of the invented .claude folder; deleted by Dispose
    /// </summary>
    private readonly string _workFolder;

    /// <summary>
    /// Invented .claude folder of the instance
    /// </summary>
    private readonly string _root;

    /// <summary>
    /// Invented working folder of the runs, full on every system
    /// </summary>
    private readonly string _workingFolder;

    /// <summary>
    /// Reader of the files of the invented .claude folder
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

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
    public SubagentRunReaderTests()
    {
        // Creating the folders
        _workFolder = Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(_workFolder, ".claude");
        _workingFolder = Path.Combine(_workFolder, "project");
        Directory.CreateDirectory(_root);

        // Building the reader of the files
        _fileReader = new ClaudeFileReader(new ClaudeFolder(_root));
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to write a text file in UTF-8 without BOM under the invented root, creating its folders
    /// </summary>
    /// <param name="relativePath">[Cannot be null] Path of the file, relative to the root</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <returns>The full path of the file</returns>
    private string WriteText(string relativePath, string text)
    {
        // Creating the folders and the file
        string fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new UTF8Encoding(false).GetBytes(text));

        // Function's return
        return fullPath;
    }

    /// <summary>
    /// Method used to build a record of the transcript, as a line
    /// </summary>
    /// <param name="timestamp">[Cannot be null] Time of the record, ISO 8601</param>
    /// <param name="toolName">[Can be null] Tool called by the assistant, or null for a record of the user</param>
    /// <returns>The line</returns>
    private string Record(string timestamp, string? toolName = null)
    {
        // Escaping the working folder for JSON
        string workingFolder = _workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal);

        // Function's return
        return toolName is null
            ? $"{{\"type\":\"user\",\"timestamp\":\"{timestamp}\",\"cwd\":\"{workingFolder}\",\"isSidechain\":true}}"
            : $"{{\"type\":\"assistant\",\"timestamp\":\"{timestamp}\",\"cwd\":\"{workingFolder}\",\"message\":{{\"model\":\"claude-sonnet-5-5\",\"content\":[{{\"type\":\"tool_use\",\"name\":\"{toolName}\",\"input\":{{}}}}]}}}}";
    }

    /// <summary>
    /// Method used to write the files of a run: its transcript, and its description when one is given
    /// </summary>
    /// <param name="agentId">[Cannot be null] Identifier of the run</param>
    /// <param name="meta">[Can be null] Text of the description, or null for a run without one</param>
    /// <param name="lines">[Cannot be null] Lines of the transcript</param>
    /// <returns>The full path of the transcript</returns>
    private string WriteRun(string agentId, string? meta, params string[] lines)
    {
        // Writing the description, then the transcript
        string folder = Path.Combine("projects", ProjectFolder, SessionId, "subagents");
        if (meta is not null)
            WriteText(Path.Combine(folder, $"agent-{agentId}.meta.json"), meta);

        // Function's return
        return WriteText(Path.Combine(folder, $"agent-{agentId}.jsonl"), string.Join("\n", lines) + "\n");
    }

    /// <summary>
    /// Method used to create a link to a folder: a junction on Windows (a symbolic link needs a privilege there), a symbolic
    /// link elsewhere
    /// </summary>
    /// <param name="link">[Cannot be null] Full path of the link</param>
    /// <param name="target">[Cannot be null] Full path of the folder it leads to</param>
    private static void CreateFolderLink(string link, string target)
    {
        // Creating the link of the system
        if (OperatingSystem.IsWindows())
        {
            using Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{target}\"") { CreateNoWindow = true, UseShellExecute = false })!;
            mklink.WaitForExit();
            mklink.ExitCode.ShouldBe(0);
        }
        else
        {
            Directory.CreateSymbolicLink(link, target);
        }
    }

    /// <summary>
    /// Method used to remove a link to a folder, not what it leads to
    /// </summary>
    /// <param name="link">[Cannot be null] Full path of the link</param>
    private static void RemoveFolderLink(string link)
    {
        // Removing the link itself: a junction is removed as a folder, a symbolic link of Linux as a file
        if (OperatingSystem.IsWindows())
            Directory.Delete(link);
        else
            File.Delete(link);
    }

    /// <summary>
    /// Method used to describe everything under the invented root: folders, files, attributes, dates, sizes and contents
    /// </summary>
    /// <returns>One line per folder or file, in a stable order</returns>
    private List<string> Snapshot()
    {
        // Listing the entries of the tree
        List<string> lines = new List<string>();
        foreach (string entry in Directory.EnumerateFileSystemEntries(_root, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(_root, entry);
            lines.Add(Directory.Exists(entry)
                ? $"D {relative} {File.GetAttributes(entry)}"
                : $"F {relative} {File.GetAttributes(entry)} {File.GetCreationTimeUtc(entry).Ticks} {File.GetLastWriteTimeUtc(entry).Ticks} "
                    + $"{new FileInfo(entry).Length} {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry)))}");
        }
        lines.Sort(StringComparer.Ordinal);

        // Function's return
        return lines;
    }

    #endregion

    #region Publics

    /// <summary>
    /// The reader of the files is required
    /// </summary>
    [Fact]
    public void FileReaderCannotBeNull()
    {
        // Building the reader without reader of the files
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new SubagentRunReader(null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("fileReader");
    }

    /// <summary>
    /// Without projects folder, there is no run and no problem
    /// </summary>
    [Fact]
    public void MissingProjectsFolderGivesNothing()
    {
        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        runs.Runs.ShouldBeEmpty();
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A run gives its identifier, its agent type, its session, its project, its first and last records, its end and its
    /// real model
    /// </summary>
    [Fact]
    public void RunGivesEveryFactOfTheCore()
    {
        // Writing a run that ends with its hand back
        WriteRun("a1b2c3", "{\"agentType\":\"csharp-reviewer\",\"model\":\"opus\"}",
            Record("2026-10-31T12:14:34.000Z"),
            Record("2026-10-31T12:15:00.000Z", "Read"),
            Record("2026-10-31T12:17:59.000Z", "SubagentHandback"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the run: the real model of the transcript wins over the model asked
        runs.Runs.ShouldBe(new List<AgentRun>()
        {
            new AgentRun("a1b2c3", "csharp-reviewer", SessionId, new Project(_workingFolder),
                new DateTimeOffset(2026, 10, 31, 12, 14, 34, TimeSpan.Zero), new DateTimeOffset(2026, 10, 31, 12, 17, 59, TimeSpan.Zero), true, "claude-sonnet-5-5"),
        });
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A run is finished when its description says so, or when its last tool call is its hand back; otherwise it runs
    /// </summary>
    [Fact]
    public void RunIsFinishedByItsDescriptionOrItsHandBack()
    {
        // Writing a run finished by its description, one by its hand back, and one still running
        WriteRun("a1", "{\"agentType\":\"Explore\",\"finished\":true}", Record("2026-10-31T12:00:00Z", "Read"));
        WriteRun("a2", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z", "SubagentHandback"));
        WriteRun("a3", "{\"agentType\":\"Explore\",\"finished\":false}", Record("2026-10-31T12:00:00Z", "Read"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the ends
        runs.Runs.Select(run => $"{run.Id}={run.IsFinished}").ShouldBe(new List<string>() { "a1=True", "a2=True", "a3=False" });
    }

    /// <summary>
    /// The model asked in the description is kept when the transcript gives none
    /// </summary>
    [Fact]
    public void ModelAskedIsKeptWhenTheTranscriptGivesNone()
    {
        // Writing a run whose transcript has no message of the assistant
        WriteRun("a1", "{\"agentType\":\"Explore\",\"model\":\"haiku\"}", Record("2026-10-31T12:00:00Z"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the model
        runs.Runs.Single().Model.ShouldBe("haiku");
    }

    /// <summary>
    /// A run without description has no agent type: it is left out and reported, and the others are read
    /// </summary>
    [Fact]
    public void RunWithoutDescriptionIsLeftOutAndReported()
    {
        // Writing a run without description next to a complete one
        string orphan = WriteRun("a1", null, Record("2026-10-31T12:00:00Z"));
        WriteRun("a2", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        runs.Runs.Select(run => run.Id).ShouldBe(new List<string>() { "a2" });
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].FilePath.ShouldBe(orphan);
        runs.Problems[0].Reason.ShouldContain("no description");
    }

    /// <summary>
    /// A transcript without description that copies a run read in another session (a resumed session) is not reported
    /// </summary>
    [Fact]
    public void CopyOfARunReadElsewhereIsNotReported()
    {
        // Writing a run with its description, and its copy without description in another session
        WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        WriteText(Path.Combine("projects", ProjectFolder, "other-session", "subagents", "agent-a1.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that the run is read once, and that the copy is not reported
        runs.Runs.Select(run => run.Id).ShouldBe(new List<string>() { "a1" });
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A run whose transcript gives no time or no working folder is left out and reported
    /// </summary>
    /// <param name="line">Only line of the transcript</param>
    /// <param name="reason">Part of the reason expected</param>
    [Theory]
    [InlineData("{\"type\":\"api-request\"}", "no record with a time")]
    [InlineData("{\"type\":\"user\",\"timestamp\":\"2026-10-31T12:00:00Z\"}", "no working folder")]
    public void RunWithoutTimeOrFolderIsLeftOut(string line, string reason)
    {
        // Writing the run
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", line);

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        runs.Runs.ShouldBeEmpty();
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].FilePath.ShouldBe(transcript);
        runs.Problems[0].Reason.ShouldContain(reason);
    }

    /// <summary>
    /// The lines that cannot be read are reported once for the file, with their number; the run is still read
    /// </summary>
    [Fact]
    public void UnreadableLinesAreReportedOncePerFile()
    {
        // Writing a run with three bad lines
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"), "{ bad", "[1]", "{ bad again", Record("2026-10-31T12:05:00Z"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        runs.Runs.Count.ShouldBe(1);
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].FilePath.ShouldBe(transcript);
        runs.Problems[0].Reason.ShouldContain("3 lines");
    }

    /// <summary>
    /// A run whose transcript has not changed since the start of the window is not read
    /// </summary>
    [Fact]
    public void RunOlderThanTheWindowIsNotRead()
    {
        // Writing an old run without description, which would be reported if it were read
        string old = WriteRun("a1", null, Record("2026-10-01T12:00:00Z"));
        File.SetLastWriteTimeUtc(old, _sinceUtc.UtcDateTime.AddSeconds(-1));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that nothing was read
        runs.Runs.ShouldBeEmpty();
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A memory folder is never walked, even when it holds what looks like a run
    /// </summary>
    [Fact]
    public void MemoryFoldersAreNeverWalked()
    {
        // Writing a run under the memory folder of the project, in any case
        WriteText(Path.Combine("projects", ProjectFolder, "Memory", "subagents", "agent-a1.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");
        WriteText(Path.Combine("projects", ProjectFolder, "memory", "subagents", "agent-a2.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that nothing was read or reported
        runs.Runs.ShouldBeEmpty();
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A file whose identifier of run is not made of letters and digits is left out and reported
    /// </summary>
    [Fact]
    public void InvalidRunIdentifierIsReported()
    {
        // Writing a transcript whose name holds a space
        string transcript = WriteRun("a 1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        runs.Runs.ShouldBeEmpty();
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].FilePath.ShouldBe(transcript);
        runs.Problems[0].Reason.ShouldContain("identifier");
    }

    /// <summary>
    /// A run already read is taken from the cache while its files do not change: even locked, it gives no problem
    /// </summary>
    [Fact]
    public void UnchangedRunIsTakenFromTheCache()
    {
        // Skipping where the exclusive lock is not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The exclusive lock of FileShare.None is reliable on Windows only");

        // Writing a run and reading it once
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc).Runs.Count.ShouldBe(1);

        // Locking the transcript, so that a second reading of the file would fail
        using FileStream locker = new FileStream(transcript, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        SubagentRuns runs = reader.Read(_sinceUtc, _nowUtc);

        // Checking that the run comes from the cache
        runs.Runs.Count.ShouldBe(1);
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A run whose transcript or description has changed is read again
    /// </summary>
    [Fact]
    public void ChangedRunIsReadAgain()
    {
        // Writing a run and reading it once
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc).Runs.Single().IsFinished.ShouldBeFalse();

        // Adding a record to the transcript, then finishing the run in its description
        File.AppendAllText(transcript, Record("2026-10-31T12:30:00Z") + "\n");
        AgentRun grown = reader.Read(_sinceUtc, _nowUtc).Runs.Single();
        WriteText(Path.Combine("projects", ProjectFolder, SessionId, "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Explore\",\"finished\":true}");
        AgentRun finished = reader.Read(_sinceUtc, _nowUtc).Runs.Single();

        // Checking that each change was read
        grown.LastRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 10, 31, 12, 30, 0, TimeSpan.Zero));
        finished.IsFinished.ShouldBeTrue();
    }

    /// <summary>
    /// A run whose files are deleted disappears from the next reading
    /// </summary>
    [Fact]
    public void DeletedRunDisappears()
    {
        // Writing a run and reading it once
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc).Runs.Count.ShouldBe(1);

        // Deleting the transcript, then reading again
        File.Delete(transcript);
        SubagentRuns runs = reader.Read(_sinceUtc, _nowUtc);

        // Checking that the run is gone
        runs.Runs.ShouldBeEmpty();
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A subagents folder that is a link (a junction on Windows, a symbolic link on Linux) is reported and not read
    /// </summary>
    [Fact]
    public void SubagentsFolderThatIsALinkIsNotRead()
    {
        // Writing a run outside the folder, and a subagents link that leads to it
        string outside = Path.Combine(_workFolder, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "agent-a1.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");
        string sessionFolder = Path.Combine(_root, "projects", ProjectFolder, SessionId);
        Directory.CreateDirectory(sessionFolder);
        string link = Path.Combine(sessionFolder, "subagents");
        if (OperatingSystem.IsWindows())
        {
            using Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{outside}\"") { CreateNoWindow = true, UseShellExecute = false })!;
            mklink.WaitForExit();
            mklink.ExitCode.ShouldBe(0);
        }
        else
        {
            Directory.CreateSymbolicLink(link, outside);
        }
        try
        {
            // Reading the runs
            SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

            // Checking that the link is reported and not read
            runs.Runs.ShouldBeEmpty();
            runs.Problems.Count.ShouldBe(1);
            runs.Problems[0].FilePath.ShouldBe(link);
            runs.Problems[0].Reason.ShouldContain("not followed");
        }
        finally
        {
            // Removing the link itself: a junction is removed as a folder, a symbolic link of Linux as a file
            if (OperatingSystem.IsWindows())
                Directory.Delete(link);
            else
                File.Delete(link);
        }
    }

    /// <summary>
    /// A reading that failed is not kept: once the file can be read again, the run appears
    /// </summary>
    [Fact]
    public void FailedReadingIsNotKept()
    {
        // Skipping where the exclusive lock is not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The exclusive lock of FileShare.None is reliable on Windows only");

        // Writing a run and reading it while its transcript is locked
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        SubagentRuns locked;
        using (FileStream locker = new FileStream(transcript, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            locked = reader.Read(_sinceUtc, _nowUtc);

        // Reading again once the lock is gone
        SubagentRuns unlocked = reader.Read(_sinceUtc, _nowUtc);

        // Checking that the failure was reported, then that the run appeared
        locked.Runs.ShouldBeEmpty();
        locked.Problems.Count.ShouldBe(1);
        locked.Problems[0].Reason.ShouldContain(nameof(FileReadFailure.Unreadable));
        unlocked.Runs.Count.ShouldBe(1);
        unlocked.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A run whose description alone has changed, with the same size and a new last write, is read again
    /// </summary>
    /// <remarks>
    /// The new last write is set by the test: two writes a few milliseconds apart can get the same last write (see
    /// <see cref="RewriteWithTheSameSizeAndTheSameLastWriteIsNotSeen"/>), which made this test fail on the CI on 2026-10-31.
    /// </remarks>
    [Fact]
    public void RunWhoseDescriptionAloneChangedIsReadAgain()
    {
        // Writing a run and reading it once
        WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc).Runs.Single().AgentTypeName.ShouldBe("Explore");

        // Changing the description only, to a description of the same size, one second later than the first one
        string meta = Path.Combine(_root, "projects", ProjectFolder, SessionId, "subagents", "agent-a1.meta.json");
        DateTime firstWriteUtc = File.GetLastWriteTimeUtc(meta);
        WriteText(Path.Combine("projects", ProjectFolder, SessionId, "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Planner\"}");
        File.SetLastWriteTimeUtc(meta, firstWriteUtc.AddSeconds(1));
        AgentRun changed = reader.Read(_sinceUtc, _nowUtc).Runs.Single();

        // Checking that the change was read
        changed.AgentTypeName.ShouldBe("Planner");
    }

    /// <summary>
    /// Known limit of the cache: a description rewritten with the same size and the same last write is not seen, and the run
    /// keeps what was read before
    /// </summary>
    /// <remarks>
    /// The cache knows a file by its size and its last write only (<see cref="CachedRun"/>). This test pins the limit so that it
    /// stays a known choice: if the cache one day compares the content, this test must change with it.
    /// </remarks>
    [Fact]
    public void RewriteWithTheSameSizeAndTheSameLastWriteIsNotSeen()
    {
        // Writing a run and reading it once
        WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"));
        SubagentRunReader reader = new SubagentRunReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc).Runs.Single().AgentTypeName.ShouldBe("Explore");

        // Rewriting the description with the same size, then giving it back its first last write, as two writes in the same
        // tick of the clock would do
        string meta = Path.Combine(_root, "projects", ProjectFolder, SessionId, "subagents", "agent-a1.meta.json");
        DateTime firstWriteUtc = File.GetLastWriteTimeUtc(meta);
        WriteText(Path.Combine("projects", ProjectFolder, SessionId, "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Planner\"}");
        File.SetLastWriteTimeUtc(meta, firstWriteUtc);
        AgentRun cached = reader.Read(_sinceUtc, _nowUtc).Runs.Single();

        // Checking that the run comes from the cache, with the agent type read before
        cached.AgentTypeName.ShouldBe("Explore");
    }

    /// <summary>
    /// A projects folder or a project folder that is a link is reported and not followed
    /// </summary>
    /// <param name="level">Level of the link: the projects folder, or a project folder</param>
    [Theory]
    [InlineData("projects")]
    [InlineData("project")]
    public void LinkAboveTheRunsIsNotFollowed(string level)
    {
        // Writing a run outside the folder, and a link to it at the given level
        string outside = Path.Combine(_workFolder, "outside");
        string sessionPart = Path.Combine(SessionId, "subagents");
        Directory.CreateDirectory(Path.Combine(outside, level == "projects" ? Path.Combine(ProjectFolder, sessionPart) : sessionPart));
        string link = level == "projects" ? Path.Combine(_root, "projects") : Path.Combine(_root, "projects", ProjectFolder);
        Directory.CreateDirectory(Path.GetDirectoryName(link)!);
        CreateFolderLink(link, outside);
        try
        {
            // Reading the runs
            SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

            // Checking that the link is reported and not followed
            runs.Runs.ShouldBeEmpty();
            runs.Problems.Count.ShouldBe(1);
            runs.Problems[0].FilePath.ShouldBe(link);
            runs.Problems[0].Reason.ShouldContain("not followed");
        }
        finally
        {
            RemoveFolderLink(link);
        }
    }

    /// <summary>
    /// A project folder named memory is never walked either
    /// </summary>
    [Fact]
    public void ProjectFolderNamedMemoryIsNeverWalked()
    {
        // Writing a complete run under a project folder named memory
        WriteText(Path.Combine("projects", "memory", SessionId, "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Explore\"}");
        WriteText(Path.Combine("projects", "memory", SessionId, "subagents", "agent-a1.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that nothing was read or reported
        runs.Runs.ShouldBeEmpty();
        runs.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A session folder whose name is not plain is reported and not read
    /// </summary>
    [Fact]
    public void SessionFolderWithAnOddNameIsReported()
    {
        // Writing a complete run under a session folder whose name holds a space
        string folder = Path.Combine(_root, "projects", ProjectFolder, "odd name");
        WriteText(Path.Combine("projects", ProjectFolder, "odd name", "subagents", "agent-a1.meta.json"), "{\"agentType\":\"Explore\"}");
        WriteText(Path.Combine("projects", ProjectFolder, "odd name", "subagents", "agent-a1.jsonl"), Record("2026-10-31T12:00:00Z") + "\n");

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that it is reported and not read
        runs.Runs.ShouldBeEmpty();
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].FilePath.ShouldBe(folder);
        runs.Problems[0].Reason.ShouldContain("not read");
    }

    /// <summary>
    /// A record dated more than five minutes after now is not read, and the run keeps its true times
    /// </summary>
    [Fact]
    public void RecordInTheFutureIsNotRead()
    {
        // Writing a run with a record five minutes after now (accepted) and one in 2099 (refused)
        WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"), Record("2026-11-01T00:05:00Z"), Record("2099-01-01T00:00:00Z"));

        // Reading the runs
        SubagentRuns runs = new SubagentRunReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the times and the problem
        runs.Runs.Single().LastRecordAtUtc.ShouldBe(new DateTimeOffset(2026, 11, 1, 0, 5, 0, TimeSpan.Zero));
        runs.Problems.Count.ShouldBe(1);
        runs.Problems[0].Reason.ShouldContain("1 lines");
    }

    /// <summary>
    /// Now must be in UTC
    /// </summary>
    [Fact]
    public void NowMustBeInUtc()
    {
        // Reading with a now that has an offset
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new SubagentRunReader(_fileReader).Read(_sinceUtc, new DateTimeOffset(2026, 11, 1, 1, 0, 0, TimeSpan.FromHours(1))));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("nowUtc");
    }

    /// <summary>
    /// The reader writes nothing, even in a folder whose files are read-only
    /// </summary>
    [Fact]
    public void ReaderWritesNothing()
    {
        // Preparing a read-only run, a run without description and a bad line
        string transcript = WriteRun("a1", "{\"agentType\":\"Explore\"}", Record("2026-10-31T12:00:00Z"), "{ bad");
        WriteRun("a2", null, Record("2026-10-31T12:00:00Z"));
        File.SetAttributes(transcript, FileAttributes.ReadOnly);
        List<string> before = Snapshot();
        SubagentRunReader reader = new SubagentRunReader(_fileReader);

        // Reading several times
        for (int turn = 0; turn < 3; turn++)
            reader.Read(_sinceUtc, _nowUtc).Runs.Count.ShouldBe(1);

        // Checking that nothing changed
        Snapshot().ShouldBe(before);
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
