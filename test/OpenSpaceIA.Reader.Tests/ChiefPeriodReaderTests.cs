/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ChiefPeriodReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the periods of the chief in its transcripts
 */

// Using
using System.Diagnostics;
using System.Globalization;
using System.Text;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the periods of the chief in its transcripts
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that every transcript of a chief written in the window gives its periods, that the
/// transcripts of the sub-agents and the memory folders are never read as a chief, and that a transcript is read only where
/// it grew.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. Invented data only; no state shared between tests.</para>
/// </remarks>
public sealed class ChiefPeriodReaderTests : IDisposable
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Start of the window of the tests: before every invented record and every file written
    /// </summary>
    private static readonly DateTimeOffset _sinceUtc = new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Now, for the tests: after every invented record
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
    /// Invented working folder of the sessions, full on every system
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
    public ChiefPeriodReaderTests()
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
    /// Method used to write the lines of one turn: a message of the user, then the end of the turn
    /// </summary>
    /// <param name="start">Minutes after noon of the message</param>
    /// <param name="end">Minutes after noon of the end of the turn</param>
    /// <returns>The two lines, each with its line feed</returns>
    private string Turn(int start, int end)
    {
        // Writing the two records
        string workingFolder = _workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal);
        string prompt = $"{{\"type\":\"user\",\"timestamp\":\"{Time(start)}\",\"cwd\":\"{workingFolder}\",\"message\":{{\"content\":\"invented\"}}}}\n";
        string answer = $"{{\"type\":\"assistant\",\"timestamp\":\"{Time(end)}\",\"message\":{{\"stop_reason\":\"end_turn\",\"content\":[]}}}}\n";

        // Function's return
        return prompt + answer;
    }

    /// <summary>
    /// Method used to write the text of a time of the day of the tests
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <returns>The time, ISO 8601</returns>
    private static string Time(int minutes)
    {
        // Function's return
        return At(minutes).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Method used to build a time of the day of the tests
    /// </summary>
    /// <param name="minutes">Minutes after noon</param>
    /// <returns>The time, in UTC</returns>
    private static DateTimeOffset At(int minutes)
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero).AddMinutes(minutes);
    }

    /// <summary>
    /// Method used to describe the periods read, in a stable order
    /// </summary>
    /// <param name="periods">[Cannot be null] Result of the reading</param>
    /// <returns>Each period as "start-end", in minutes after noon, sorted</returns>
    private static string Describe(ChiefPeriods periods)
    {
        // Function's return
        return string.Join(", ", periods.Periods
            .Select(period => $"{(int)(period.StartUtc - At(0)).TotalMinutes}-{(int)(period.EndUtc - At(0)).TotalMinutes}")
            .OrderBy(text => text, StringComparer.Ordinal));
    }

    #endregion

    #region Publics

    /// <summary>
    /// The reader of the files is required, and now must be in UTC
    /// </summary>
    [Fact]
    public void ArgumentsAreChecked()
    {
        // Building the reader without reader of the files, then reading with a now that has an offset
        ArgumentNullException withoutFileReader = Should.Throw<ArgumentNullException>(() => new ChiefPeriodReader(null!));
        ArgumentOutOfRangeException offset = Should.Throw<ArgumentOutOfRangeException>(
            () => new ChiefPeriodReader(_fileReader).Read(_sinceUtc, new DateTimeOffset(2026, 11, 1, 1, 0, 0, TimeSpan.FromHours(1))));

        // Checking the parameters in fault
        withoutFileReader.ParamName.ShouldBe("fileReader");
        offset.ParamName.ShouldBe("nowUtc");
    }

    /// <summary>
    /// Without projects folder, there is no period and no problem
    /// </summary>
    [Fact]
    public void MissingProjectsFolderGivesNothing()
    {
        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the result
        periods.Periods.ShouldBeEmpty();
        periods.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Every transcript of a chief, in every project, gives its periods on its project
    /// </summary>
    [Fact]
    public void EveryTranscriptOfAChiefGivesItsPeriods()
    {
        // Writing two transcripts in two projects
        WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), Turn(0, 5));
        WriteText(Path.Combine("projects", "C--b", "s2.jsonl"), Turn(10, 20) + Turn(30, 31));

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the periods
        Describe(periods).ShouldBe("0-5, 10-20, 30-31");
        periods.Periods.ShouldAllBe(period => period.Kind == PeriodKind.Working && period.Project == new Project(_workingFolder));
        periods.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// The transcripts of the sub-agents and the memory folders are never read as a chief
    /// </summary>
    [Fact]
    public void SubAgentsAndMemoryAreNotReadAsAChief()
    {
        // Writing a run of a sub-agent and transcripts in memory folders, at the project level and inside a project
        WriteText(Path.Combine("projects", "C--a", "s1", "subagents", "agent-a1.jsonl"), Turn(0, 5));
        WriteText(Path.Combine("projects", "memory", "s2.jsonl"), Turn(10, 20));
        WriteText(Path.Combine("projects", "C--a", "memory", "s3.jsonl"), Turn(30, 40));

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that nothing was read
        periods.Periods.ShouldBeEmpty();
        periods.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A transcript that has not changed since the start of the window is not read
    /// </summary>
    [Fact]
    public void TranscriptOlderThanTheWindowIsNotRead()
    {
        // Writing an old transcript
        string old = WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), Turn(0, 5));
        File.SetLastWriteTimeUtc(old, _sinceUtc.UtcDateTime.AddSeconds(-1));

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that nothing was read
        periods.Periods.ShouldBeEmpty();
    }

    /// <summary>
    /// A transcript is read only where it grew: a turn added after the first reading is added to the periods
    /// </summary>
    [Fact]
    public void TranscriptIsReadOnlyWhereItGrew()
    {
        // Writing a transcript with a turn and a bad line, and reading it once
        string transcript = WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), "{ bad\n" + Turn(0, 5));
        ChiefPeriodReader reader = new ChiefPeriodReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc);

        // Adding a turn, then reading again
        File.AppendAllText(transcript, Turn(10, 12));
        ChiefPeriods periods = reader.Read(_sinceUtc, _nowUtc);

        // Checking both turns, and the bad line counted once
        Describe(periods).ShouldBe("0-5, 10-12");
        periods.Problems.Single().Reason.ShouldContain("1 lines");
    }

    /// <summary>
    /// A session resumed in a new transcript copies the history of the old one: the same period is kept once
    /// </summary>
    [Fact]
    public void SamePeriodInTwoTranscriptsIsKeptOnce()
    {
        // Writing the same turn in two transcripts, and a turn of its own in the second one
        WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), Turn(0, 5));
        WriteText(Path.Combine("projects", "C--a", "s2.jsonl"), Turn(0, 5) + Turn(10, 12));

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking that the copy is kept once
        Describe(periods).ShouldBe("0-5, 10-12");
    }

    /// <summary>
    /// The lines longer than the limit are reported with the others that cannot be read
    /// </summary>
    [Fact]
    public void LinesTooLongAreReported()
    {
        // Writing a turn and a line longer than the limit of a small reader
        string transcript = WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), new string('x', 2000) + "\n" + Turn(0, 5));
        ClaudeFileReader smallReader = new ClaudeFileReader(new ClaudeFolder(_root), 1048576, 1000);

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(smallReader).Read(_sinceUtc, _nowUtc);

        // Checking the period and the problem
        Describe(periods).ShouldBe("0-5");
        periods.Problems.Single().FilePath.ShouldBe(transcript);
        periods.Problems.Single().Reason.ShouldContain("1 longer than the limit");
    }

    /// <summary>
    /// A transcript that cannot be read keeps the periods read before, is reported, and is read again cleanly next time
    /// </summary>
    [Fact]
    public void UnreadableTranscriptKeepsThePeriodsReadBefore()
    {
        // Skipping where the exclusive lock is not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The exclusive lock of FileShare.None is reliable on Windows only");

        // Writing a transcript, reading it once, then adding a turn
        string transcript = WriteText(Path.Combine("projects", "C--a", "s1.jsonl"), Turn(0, 5));
        ChiefPeriodReader reader = new ChiefPeriodReader(_fileReader);
        reader.Read(_sinceUtc, _nowUtc);
        File.AppendAllText(transcript, Turn(10, 12));

        // Reading while the transcript is locked, then once it is free
        ChiefPeriods locked;
        using (FileStream locker = new FileStream(transcript, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            locked = reader.Read(_sinceUtc, _nowUtc);
        ChiefPeriods free = reader.Read(_sinceUtc, _nowUtc);

        // Checking the periods kept and the problem, then the whole transcript
        Describe(locked).ShouldBe("0-5");
        locked.Problems.Single().Reason.ShouldContain(nameof(FileReadFailure.Unreadable));
        Describe(free).ShouldBe("0-5, 10-12");
        free.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A record dated more than five minutes after now is not read
    /// </summary>
    [Fact]
    public void RecordInTheFutureIsNotRead()
    {
        // Writing a turn whose end is in 2099, then a true end
        string workingFolder = _workingFolder.Replace("\\", "\\\\", StringComparison.Ordinal);
        WriteText(Path.Combine("projects", "C--a", "s1.jsonl"),
            $"{{\"type\":\"user\",\"timestamp\":\"{Time(0)}\",\"cwd\":\"{workingFolder}\",\"message\":{{\"content\":\"invented\"}}}}\n"
            + "{\"type\":\"assistant\",\"timestamp\":\"2099-01-01T00:00:00Z\",\"message\":{\"stop_reason\":\"end_turn\",\"content\":[]}}\n"
            + $"{{\"type\":\"assistant\",\"timestamp\":\"{Time(5)}\",\"message\":{{\"stop_reason\":\"end_turn\",\"content\":[]}}}}\n");

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the period and the problem
        Describe(periods).ShouldBe("0-5");
        periods.Problems.Single().Reason.ShouldContain("1 lines");
    }

    /// <summary>
    /// A turn whose project cannot be known is reported
    /// </summary>
    [Fact]
    public void TurnWithoutProjectIsReported()
    {
        // Writing a turn without working folder
        string transcript = WriteText(Path.Combine("projects", "C--a", "s1.jsonl"),
            $"{{\"type\":\"user\",\"timestamp\":\"{Time(0)}\",\"message\":{{\"content\":\"invented\"}}}}\n"
            + $"{{\"type\":\"assistant\",\"timestamp\":\"{Time(5)}\",\"message\":{{\"stop_reason\":\"end_turn\",\"content\":[]}}}}\n");

        // Reading the periods
        ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

        // Checking the problem
        periods.Periods.ShouldBeEmpty();
        periods.Problems.Single().FilePath.ShouldBe(transcript);
        periods.Problems.Single().Reason.ShouldContain("1 turns");
    }

    /// <summary>
    /// A project folder that is a link (a junction on Windows, a symbolic link on Linux) is reported and not followed
    /// </summary>
    [Fact]
    public void ProjectFolderThatIsALinkIsNotFollowed()
    {
        // Writing a transcript outside the folder, and a project link that leads to it
        string outside = Path.Combine(_workFolder, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "s1.jsonl"), Turn(0, 5));
        Directory.CreateDirectory(Path.Combine(_root, "projects"));
        string link = Path.Combine(_root, "projects", "C--a");
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
            // Reading the periods
            ChiefPeriods periods = new ChiefPeriodReader(_fileReader).Read(_sinceUtc, _nowUtc);

            // Checking that the link is reported and not followed
            periods.Periods.ShouldBeEmpty();
            periods.Problems.Single().FilePath.ShouldBe(link);
            periods.Problems.Single().Reason.ShouldContain("not followed");
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
