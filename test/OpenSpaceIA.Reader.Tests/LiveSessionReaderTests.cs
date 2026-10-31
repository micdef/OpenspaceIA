/*
 *	@file	test/OpenSpaceIA.Reader.Tests/LiveSessionReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the open sessions of Claude Code
 */

// Using
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the open sessions of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that only the open sessions are kept, that the key files are never opened, and that a bad
/// file never stops the reading of the others.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. The processes are invented by a fake port; no state shared between
/// tests.</para>
/// </remarks>
public sealed class LiveSessionReaderTests : IDisposable
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Unique folder of the instance, parent of the invented .claude folder; deleted by Dispose
    /// </summary>
    private readonly string _workFolder;

    /// <summary>
    /// Invented .claude folder of the instance
    /// </summary>
    private readonly string _root;

    /// <summary>
    /// Reader of the files of the invented .claude folder
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

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
    public LiveSessionReaderTests()
    {
        // Creating the folders
        _workFolder = Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(_workFolder, ".claude");
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
    /// Method used to write a valid session file, named after its process ID
    /// </summary>
    /// <param name="processId">ID of the invented process</param>
    /// <param name="processStart">[Cannot be null] Start time of the invented process</param>
    /// <returns>The full path of the file</returns>
    private string WriteSession(int processId, string processStart)
    {
        // Building the text with an escaped working folder, full on every system
        string workingFolder = Path.Combine(_workFolder, "project").Replace("\\", "\\\\", StringComparison.Ordinal);
        string text = $"{{\"pid\":{processId},\"sessionId\":\"session-{processId}\",\"cwd\":\"{workingFolder}\",\"startedAt\":1793392190183,\"procStart\":\"{processStart}\",\"status\":\"idle\"}}";

        // Function's return
        return WriteText(Path.Combine("sessions", $"{processId}.json"), text);
    }

    /// <summary>
    /// Method used to list the process IDs of the sessions read, in their order
    /// </summary>
    /// <param name="sessions">[Cannot be null] Result of the reading</param>
    /// <returns>The process IDs, separated by a comma and a space</returns>
    private static string ProcessIds(LiveSessions sessions)
    {
        // Function's return
        return string.Join(", ", sessions.Sessions.Select(session => session.File.ProcessId));
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
    /// The reader of the files and the port are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Building the reader without each argument
        ArgumentNullException withoutFileReader = Should.Throw<ArgumentNullException>(() => new LiveSessionReader(null!, _probe));
        ArgumentNullException withoutProbe = Should.Throw<ArgumentNullException>(() => new LiveSessionReader(_fileReader, null!));

        // Checking the parameters in fault
        withoutFileReader.ParamName.ShouldBe("fileReader");
        withoutProbe.ParamName.ShouldBe("processProbe");
    }

    /// <summary>
    /// Without sessions folder, there is no session and no problem
    /// </summary>
    [Fact]
    public void MissingSessionsFolderGivesNothing()
    {
        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the result
        sessions.Sessions.ShouldBeEmpty();
        sessions.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// An alive session is kept, a closed one is left out without a problem, and one whose process cannot be checked is
    /// kept and reported
    /// </summary>
    [Fact]
    public void OnlyTheSessionsThatMayBeOpenAreKept()
    {
        // Writing three sessions and inventing their processes
        WriteSession(10, "100");
        WriteSession(20, "200");
        string unknown = WriteSession(30, "300");
        _probe.Answers[10] = ProcessLiveness.Alive;
        _probe.Answers[20] = ProcessLiveness.Closed;
        _probe.Answers[30] = ProcessLiveness.Unknown;

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the result
        ProcessIds(sessions).ShouldBe("10, 30");
        sessions.Sessions[0].Liveness.ShouldBe(ProcessLiveness.Alive);
        sessions.Sessions[1].Liveness.ShouldBe(ProcessLiveness.Unknown);
        sessions.Problems.Count.ShouldBe(1);
        sessions.Problems[0].FilePath.ShouldBe(unknown);
        sessions.Problems[0].Reason.ShouldContain("cannot be checked");
        sessions.Problems[0].Reason.ShouldContain(FakeProcessProbe.UnknownDetail);
    }

    /// <summary>
    /// The port is asked with the process ID and the start time written in the file
    /// </summary>
    [Fact]
    public void ProbeIsAskedWithTheProcessAndItsStart()
    {
        // Writing a session
        WriteSession(10, "134378657888122155");
        _probe.Answers[10] = ProcessLiveness.Alive;

        // Reading the sessions
        new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the question asked
        _probe.Questions.ShouldBe(new List<string>() { "10 134378657888122155" });
    }

    /// <summary>
    /// The sessions are read in the ordinal order of the file names
    /// </summary>
    [Fact]
    public void SessionsAreReadInTheOrderOfTheFileNames()
    {
        // Writing sessions out of order: "100" comes before "20" in the ordinal order
        WriteSession(20, "1");
        WriteSession(100, "1");
        WriteSession(3, "1");
        _probe.Answers[3] = ProcessLiveness.Alive;
        _probe.Answers[20] = ProcessLiveness.Alive;
        _probe.Answers[100] = ProcessLiveness.Alive;

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the order
        ProcessIds(sessions).ShouldBe("100, 20, 3");
    }

    /// <summary>
    /// The key files, and every file that is not a session file, are never opened: even locked, they give no problem
    /// </summary>
    [Fact]
    public void KeyFilesAreNeverOpened()
    {
        // Writing a session, its key file and a note, and locking the key file where a lock is enforced
        WriteSession(10, "100");
        string key = WriteText(Path.Combine("sessions", "10.0123456789abcdef.key"), "invented-secret");
        WriteText(Path.Combine("sessions", "notes.txt"), "invented");
        _probe.Answers[10] = ProcessLiveness.Alive;
        using FileStream locker = new FileStream(key, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking that only the session file was read, and that nothing was reported
        ProcessIds(sessions).ShouldBe("10");
        sessions.Problems.ShouldBeEmpty();
        _probe.Questions.Count.ShouldBe(1);
    }

    /// <summary>
    /// A broken session file is reported and does not stop the others; its process is never checked
    /// </summary>
    [Fact]
    public void BrokenSessionFileDoesNotStopTheOthers()
    {
        // Writing a broken file between two good ones
        WriteSession(10, "100");
        string broken = WriteText(Path.Combine("sessions", "15.json"), "{ not json");
        WriteSession(20, "200");
        _probe.Answers[10] = ProcessLiveness.Alive;
        _probe.Answers[20] = ProcessLiveness.Alive;

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the result
        ProcessIds(sessions).ShouldBe("10, 20");
        sessions.Problems.Count.ShouldBe(1);
        sessions.Problems[0].FilePath.ShouldBe(broken);
        _probe.Questions.Count.ShouldBe(2);
    }

    /// <summary>
    /// A session file that cannot be read (here, too large) is reported with the reason of the failure
    /// </summary>
    [Fact]
    public void UnreadableSessionFileIsReportedWithItsReason()
    {
        // Writing a session larger than the limit of a small reader
        string large = WriteSession(10, new string('1', 300));
        ClaudeFileReader smallReader = new ClaudeFileReader(new ClaudeFolder(_root), 64);

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(smallReader, _probe).Read();

        // Checking the result
        sessions.Sessions.ShouldBeEmpty();
        sessions.Problems.Count.ShouldBe(1);
        sessions.Problems[0].FilePath.ShouldBe(large);
        sessions.Problems[0].Reason.ShouldContain(nameof(FileReadFailure.TooLarge));
    }

    /// <summary>
    /// A subfolder of the sessions folder is reported and not read
    /// </summary>
    [Fact]
    public void SubfolderIsReportedAndNotRead()
    {
        // Writing a session in a subfolder
        WriteText(Path.Combine("sessions", "sub", "10.json"), "{}");

        // Reading the sessions
        LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

        // Checking the result
        sessions.Sessions.ShouldBeEmpty();
        sessions.Problems.Count.ShouldBe(1);
        sessions.Problems[0].FilePath.ShouldBe(Path.Combine(_root, "sessions", "sub"));
        sessions.Problems[0].Reason.ShouldContain("subfolder");
        _probe.Questions.ShouldBeEmpty();
    }

    /// <summary>
    /// A sessions folder that is a link (a junction on Windows, a symbolic link on Linux) is reported and not read
    /// </summary>
    [Fact]
    public void SessionsFolderThatIsALinkIsNotRead()
    {
        // Writing a session outside the folder, and a sessions link that leads to it
        string outside = Path.Combine(_workFolder, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "10.json"), "{}");
        string link = Path.Combine(_root, "sessions");
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
            // Reading the sessions
            LiveSessions sessions = new LiveSessionReader(_fileReader, _probe).Read();

            // Checking that the link is reported and not read
            sessions.Sessions.ShouldBeEmpty();
            sessions.Problems.Count.ShouldBe(1);
            sessions.Problems[0].FilePath.ShouldBe(link);
            sessions.Problems[0].Reason.ShouldContain("not followed");
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
    /// The reader writes nothing, even in a folder whose files are read-only
    /// </summary>
    [Fact]
    public void ReaderWritesNothing()
    {
        // Preparing a read-only session, a broken one and a key file
        string good = WriteSession(10, "100");
        string broken = WriteText(Path.Combine("sessions", "15.json"), "invented");
        WriteText(Path.Combine("sessions", "10.0123.key"), "invented-secret");
        File.SetAttributes(good, FileAttributes.ReadOnly);
        File.SetAttributes(broken, FileAttributes.ReadOnly);
        _probe.Answers[10] = ProcessLiveness.Alive;
        List<string> before = Snapshot();
        LiveSessionReader reader = new LiveSessionReader(_fileReader, _probe);

        // Reading several times
        for (int turn = 0; turn < 3; turn++)
            reader.Read().Sessions.Count.ShouldBe(1);

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

    /// <summary>
    /// Fake port that gives the answer chosen by the test for each process ID, and remembers each question
    /// </summary>
    private sealed class FakeProcessProbe : IProcessProbe
    {
        #region Members

        #region Constants

        /// <summary>
        /// Detail given with an unknown answer
        /// </summary>
        public const string UnknownDetail = "invented reason of the doubt";

        #endregion

        #endregion

        #region Accessors/Properties

        #region Members & Properties

        /// <summary>
        /// Answer for each process ID; a process ID without an answer is closed
        /// </summary>
        public Dictionary<int, ProcessLiveness> Answers { get; } = new Dictionary<int, ProcessLiveness>();

        /// <summary>
        /// Questions asked, as "process ID, space, start time"
        /// </summary>
        public List<string> Questions { get; } = new List<string>();

        #endregion

        #endregion

        #region Methods

        #region Publics

        /// <inheritdoc/>
        public ProcessLiveness Check(int processId, string processStart, out string? detail)
        {
            // Remembering the question, and giving the chosen answer
            Questions.Add($"{processId} {processStart}");
            ProcessLiveness answer = Answers.TryGetValue(processId, out ProcessLiveness chosen) ? chosen : ProcessLiveness.Closed;
            detail = answer == ProcessLiveness.Unknown ? UnknownDetail : null;

            // Function's return
            return answer;
        }

        #endregion

        #endregion
    }
}
