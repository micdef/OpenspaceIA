/*
 *	@file	test/OpenSpaceIA.Reader.Tests/WaitReasonReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the reason why a chief waits for the user
 */

// Using
using System.Text;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the reason why a chief waits for the user
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that only the waiting sessions are read, that the transcript is found and read only where
/// it grew, and that a waiting session always gets a reason, even when its transcript cannot help.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. Invented data only; no state shared between tests.</para>
/// </remarks>
public sealed class WaitReasonReaderTests : IDisposable
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
    public WaitReasonReaderTests()
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
    /// Method used to build an open session of the invented project
    /// </summary>
    /// <param name="sessionId">[Cannot be null] Identifier of the session</param>
    /// <param name="status">[Can be null] Status of the session</param>
    /// <returns>The open session</returns>
    private LiveSession Session(string sessionId, SessionStatus? status)
    {
        // Function's return
        return new LiveSession(new SessionFile(1234, sessionId, new Project(Path.Combine(_workFolder, "project")), "1",
            new DateTimeOffset(2026, 10, 31, 9, 0, 0, TimeSpan.Zero), status), ProcessLiveness.Alive);
    }

    /// <summary>
    /// Method used to write the transcript of a session
    /// </summary>
    /// <param name="sessionId">[Cannot be null] Identifier of the session</param>
    /// <param name="text">[Cannot be null] Text of the transcript</param>
    /// <returns>The full path of the transcript</returns>
    private string WriteTranscript(string sessionId, string text)
    {
        // Creating the folder and the file
        string fullPath = Path.Combine(_root, "projects", ProjectFolder, sessionId + ".jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new UTF8Encoding(false).GetBytes(text));

        // Function's return
        return fullPath;
    }

    /// <summary>
    /// Method used to write a record of the assistant that calls a tool, as a line with its line feed
    /// </summary>
    /// <param name="id">[Cannot be null] Identifier of the call</param>
    /// <param name="name">[Cannot be null] Name of the tool</param>
    /// <returns>The line</returns>
    private static string Call(string id, string name)
    {
        // Function's return
        return $"{{\"type\":\"assistant\",\"message\":{{\"content\":[{{\"type\":\"tool_use\",\"id\":\"{id}\",\"name\":\"{name}\",\"input\":{{}}}}]}}}}\n";
    }

    /// <summary>
    /// Method used to write a record of the user that gives the result of a tool, as a line with its line feed
    /// </summary>
    /// <param name="id">[Cannot be null] Identifier of the call answered</param>
    /// <returns>The line</returns>
    private static string Result(string id)
    {
        // Function's return
        return $"{{\"type\":\"user\",\"message\":{{\"content\":[{{\"type\":\"tool_result\",\"tool_use_id\":\"{id}\"}}]}}}}\n";
    }

    #endregion

    #region Publics

    /// <summary>
    /// The reader of the files is required, and so are the sessions
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Building the reader without reader of the files, then reading without sessions
        ArgumentNullException withoutFileReader = Should.Throw<ArgumentNullException>(() => new WaitReasonReader(null!));
        ArgumentNullException withoutSessions = Should.Throw<ArgumentNullException>(() => new WaitReasonReader(_fileReader).Read(null!));

        // Checking the parameters in fault
        withoutFileReader.ParamName.ShouldBe("fileReader");
        withoutSessions.ParamName.ShouldBe("sessions");
    }

    /// <summary>
    /// A waiting session gets the reason given by its open tool call
    /// </summary>
    [Fact]
    public void WaitingSessionGetsTheReasonOfItsOpenCall()
    {
        // Writing two transcripts: one with an open question, one with an open read
        WriteTranscript("s1", Call("toolu_1", "AskUserQuestion"));
        WriteTranscript("s2", Call("toolu_2", "Read"));

        // Reading the reasons
        WaitReasons reasons = new WaitReasonReader(_fileReader).Read(new List<LiveSession>() { Session("s1", SessionStatus.Waiting), Session("s2", SessionStatus.Waiting) });

        // Checking the reasons
        reasons.Reasons["s1"].ShouldBe(WaitReason.Question);
        reasons.Reasons["s2"].ShouldBe(WaitReason.Permission);
        reasons.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A session that does not wait is not read and gets no reason: even a broken transcript gives no problem
    /// </summary>
    /// <param name="statusValue">Status of the session (an integer, so that null can be given; -1 for null)</param>
    [Theory]
    [InlineData((int)SessionStatus.Busy)]
    [InlineData((int)SessionStatus.Idle)]
    [InlineData(-1)]
    public void SessionThatDoesNotWaitIsNotRead(int statusValue)
    {
        // Writing a transcript that would be reported if it were read
        WriteTranscript("s1", "{ not json\n");
        SessionStatus? status = statusValue < 0 ? null : (SessionStatus)statusValue;

        // Reading the reasons
        WaitReasons reasons = new WaitReasonReader(_fileReader).Read(new List<LiveSession>() { Session("s1", status) });

        // Checking that nothing was read
        reasons.Reasons.ShouldBeEmpty();
        reasons.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A waiting session without transcript gets a question, as decided, and the missing transcript is reported
    /// </summary>
    [Fact]
    public void WaitingSessionWithoutTranscriptGetsAQuestion()
    {
        // Reading the reason of a session whose transcript does not exist
        WaitReasons reasons = new WaitReasonReader(_fileReader).Read(new List<LiveSession>() { Session("s1", SessionStatus.Waiting) });

        // Checking the reason and the problem
        reasons.Reasons["s1"].ShouldBe(WaitReason.Question);
        reasons.Problems.Count.ShouldBe(1);
        reasons.Problems[0].Reason.ShouldContain("no transcript");
    }

    /// <summary>
    /// The transcript is read only where it grew: a call answered after the first reading is closed at the second one
    /// </summary>
    [Fact]
    public void TranscriptIsReadOnlyWhereItGrew()
    {
        // Writing a transcript with an open read, and reading it once
        string transcript = WriteTranscript("s1", Call("toolu_1", "Read"));
        WaitReasonReader reader = new WaitReasonReader(_fileReader);
        List<LiveSession> sessions = new List<LiveSession>() { Session("s1", SessionStatus.Waiting) };
        reader.Read(sessions).Reasons["s1"].ShouldBe(WaitReason.Permission);

        // Answering the read and opening a question, then reading again
        File.AppendAllText(transcript, Result("toolu_1") + Call("toolu_2", "AskUserQuestion"));
        WaitReasons reasons = reader.Read(sessions);

        // Checking the reason
        reasons.Reasons["s1"].ShouldBe(WaitReason.Question);
    }

    /// <summary>
    /// A line that Claude Code is still writing is read once it is ended
    /// </summary>
    [Fact]
    public void UnfinishedLineIsReadOnceEnded()
    {
        // Writing a transcript whose question is not ended yet, and reading it once
        string question = Call("toolu_1", "AskUserQuestion");
        string transcript = WriteTranscript("s1", question.Substring(0, 30));
        WaitReasonReader reader = new WaitReasonReader(_fileReader);
        List<LiveSession> sessions = new List<LiveSession>() { Session("s1", SessionStatus.Waiting) };
        WaitReasons before = reader.Read(sessions);

        // Ending the line, then reading again
        File.AppendAllText(transcript, question.AsSpan(30));
        WaitReasons after = reader.Read(sessions);

        // Checking that the unfinished line was not counted as unreadable, then that it was read whole
        before.Problems.ShouldBeEmpty();
        after.Reasons["s1"].ShouldBe(WaitReason.Question);
        after.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A transcript that became shorter (rewritten) is read again from its start
    /// </summary>
    [Fact]
    public void ShorterTranscriptIsReadAgainFromItsStart()
    {
        // Writing a long transcript with an open read, and reading it once
        string transcript = WriteTranscript("s1", Result("toolu_0") + Result("toolu_0") + Call("toolu_1", "Read"));
        WaitReasonReader reader = new WaitReasonReader(_fileReader);
        List<LiveSession> sessions = new List<LiveSession>() { Session("s1", SessionStatus.Waiting) };
        reader.Read(sessions).Reasons["s1"].ShouldBe(WaitReason.Permission);

        // Rewriting it shorter, with an open question only
        WriteTranscript("s1", Call("toolu_9", "AskUserQuestion"));
        WaitReasons reasons = reader.Read(sessions);

        // Checking that the new content was read from the start, and the old one forgotten
        reasons.Reasons["s1"].ShouldBe(WaitReason.Question);
        new FileInfo(transcript).Length.ShouldBeLessThan(200);
    }

    /// <summary>
    /// The lines that cannot be read are reported once per transcript, and the reason is still given
    /// </summary>
    [Fact]
    public void UnreadableLinesAreReported()
    {
        // Writing a transcript with two bad lines and an open read
        string transcript = WriteTranscript("s1", "{ bad\n[1]\n" + Call("toolu_1", "Read"));

        // Reading the reason
        WaitReasons reasons = new WaitReasonReader(_fileReader).Read(new List<LiveSession>() { Session("s1", SessionStatus.Waiting) });

        // Checking the reason and the problem
        reasons.Reasons["s1"].ShouldBe(WaitReason.Permission);
        reasons.Problems.Count.ShouldBe(1);
        reasons.Problems[0].FilePath.ShouldBe(transcript);
        reasons.Problems[0].Reason.ShouldContain("2 lines");
    }

    /// <summary>
    /// The transcript of a session is never searched in a memory folder
    /// </summary>
    [Fact]
    public void TranscriptIsNeverSearchedInAMemoryFolder()
    {
        // Writing a transcript with the name of the session under a project folder named memory
        string fullPath = Path.Combine(_root, "projects", "memory", "s1.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, Call("toolu_1", "Read"));

        // Reading the reason
        WaitReasons reasons = new WaitReasonReader(_fileReader).Read(new List<LiveSession>() { Session("s1", SessionStatus.Waiting) });

        // Checking that it was not found there
        reasons.Reasons["s1"].ShouldBe(WaitReason.Question);
        reasons.Problems.Single().Reason.ShouldContain("no transcript");
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
