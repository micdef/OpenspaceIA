/*
 *	@file	test/OpenSpaceIA.Reader.Tests/SessionFileParserTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of a session file of Claude Code
 */

// Using
using System.Text.Json.Nodes;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of a session file of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves which session files are read, and that every file that cannot give the facts of a session
/// is reported instead of giving a wrong session.</para>
/// <para><b>Behavior:</b> invented texts only, shaped like the files measured on 2026-10-31; no file, no state shared
/// between tests.</para>
/// </remarks>
public sealed class SessionFileParserTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Invented path of the file, named after its process ID, full on every system
    /// </summary>
    private static readonly string _filePath = Path.Combine(Path.GetTempPath(), "invented", "sessions", "1234.json");

    /// <summary>
    /// Invented working folder of the session, full on every system
    /// </summary>
    private static readonly string _workingFolder = Path.Combine(Path.GetTempPath(), "invented", "project");

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
    /// Method used to build the text of a valid session file, then to change it
    /// </summary>
    /// <param name="change">[Can be null] Change made to the object before it is written</param>
    /// <returns>The text of the file</returns>
    private static string Json(Action<JsonObject>? change = null)
    {
        // Building the object of a valid file
        JsonObject session = new JsonObject()
        {
            ["pid"] = 1234,
            ["sessionId"] = "invented-session",
            ["cwd"] = _workingFolder,
            ["startedAt"] = 1793392190183L,
            ["procStart"] = "134378657888122155",
            ["version"] = "2.1.312",
            ["kind"] = "interactive",
            ["status"] = "busy",
        };
        change?.Invoke(session);

        // Function's return
        return session.ToJsonString();
    }

    /// <summary>
    /// Method used to parse a text with a new list of problems
    /// </summary>
    /// <param name="text">[Cannot be null] Text of the invented file</param>
    /// <param name="problems">The problems reported by the parser</param>
    /// <returns>[Can be null] The facts of the session, or null when the file is skipped</returns>
    private static SessionFile? Parse(string text, out List<ReadProblem> problems)
    {
        // Parsing the text
        problems = new List<ReadProblem>();

        // Function's return
        return SessionFileParser.Parse(_filePath, text, problems);
    }

    #endregion

    #region Publics

    /// <summary>
    /// A valid file gives every fact of the session; the other keys are left out
    /// </summary>
    [Fact]
    public void ValidFileGivesTheFactsOfTheSession()
    {
        // Parsing a valid file
        SessionFile? file = Parse(Json(), out List<ReadProblem> problems);

        // Checking the facts
        file.ShouldBe(new SessionFile(1234, "invented-session", new Project(_workingFolder), "134378657888122155",
            DateTimeOffset.FromUnixTimeMilliseconds(1793392190183L), SessionStatus.Busy));
        file!.StartedAtUtc.Offset.ShouldBe(TimeSpan.Zero);
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// The three statuses of Claude Code are read
    /// </summary>
    /// <param name="text">Status written by Claude Code</param>
    /// <param name="status">Status expected</param>
    [Theory]
    [InlineData("busy", SessionStatus.Busy)]
    [InlineData("waiting", SessionStatus.Waiting)]
    [InlineData("idle", SessionStatus.Idle)]
    public void KnownStatusesAreRead(string text, SessionStatus status)
    {
        // Parsing the file
        SessionFile? file = Parse(Json(session => session["status"] = text), out List<ReadProblem> problems);

        // Checking the status
        file.ShouldNotBeNull();
        file.Status.ShouldBe(status);
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A missing status is not a problem: Claude Code writes it about one second after the file
    /// </summary>
    [Fact]
    public void MissingStatusIsUnknownWithoutAProblem()
    {
        // Parsing a file without status
        SessionFile? file = Parse(Json(session => session.Remove("status")), out List<ReadProblem> problems);

        // Checking the status
        file.ShouldNotBeNull();
        file.Status.ShouldBeNull();
        problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A status that is not known is left unknown and reported; the session is kept
    /// </summary>
    /// <param name="change">Kind of status that is not known</param>
    [Theory]
    [InlineData("other text")]
    [InlineData("uppercase")]
    [InlineData("number")]
    [InlineData("null")]
    public void UnknownStatusIsReported(string change)
    {
        // Building a status that is not known
        SessionFile? file = Parse(Json(session => session["status"] = change switch
        {
            "other text" => "sleeping",
            "uppercase" => "BUSY",
            "number" => 1,
            _ => null,
        }), out List<ReadProblem> problems);

        // Checking that the session is kept with an unknown status, and that the status is reported
        file.ShouldNotBeNull();
        file.Status.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(_filePath);
        problems[0].Reason.ShouldContain("key: status is not a known status");
    }

    /// <summary>
    /// A file whose process ID is missing or invalid is skipped and reported
    /// </summary>
    /// <param name="change">Kind of invalid process ID</param>
    [Theory]
    [InlineData("missing")]
    [InlineData("text")]
    [InlineData("zero")]
    [InlineData("negative")]
    [InlineData("decimal")]
    [InlineData("too large")]
    public void InvalidProcessIdSkipsTheFile(string change)
    {
        // Building the invalid process ID
        SessionFile? file = Parse(Json(session =>
        {
            if (change == "missing")
            {
                session.Remove("pid");
            }
            else
            {
                session["pid"] = change switch
                {
                    "text" => JsonValue.Create("1234"),
                    "zero" => JsonValue.Create(0),
                    "negative" => JsonValue.Create(-1234),
                    "decimal" => JsonValue.Create(1234.5),
                    _ => JsonValue.Create(4294967296L),
                };
            }
        }), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        file.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("key: pid is missing");
    }

    /// <summary>
    /// A file whose process ID is not its name is skipped and reported: one of them is wrong
    /// </summary>
    [Fact]
    public void ProcessIdThatIsNotTheNameOfTheFileSkipsTheFile()
    {
        // Parsing a file named 1234.json that gives another process ID
        SessionFile? file = Parse(Json(session => session["pid"] = 4321), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        file.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("key: pid is not the name of the file");
    }

    /// <summary>
    /// A file whose identifier, working folder or process start is missing or invalid is skipped and reported
    /// </summary>
    /// <param name="key">Key that is made invalid</param>
    /// <param name="change">Kind of invalid value</param>
    [Theory]
    [InlineData("sessionId", "missing")]
    [InlineData("sessionId", "empty")]
    [InlineData("sessionId", "blank")]
    [InlineData("sessionId", "number")]
    [InlineData("cwd", "missing")]
    [InlineData("cwd", "relative")]
    [InlineData("cwd", "number")]
    [InlineData("procStart", "missing")]
    [InlineData("procStart", "empty")]
    [InlineData("procStart", "letters")]
    [InlineData("procStart", "negative")]
    [InlineData("procStart", "number")]
    public void InvalidTextKeySkipsTheFile(string key, string change)
    {
        // Building the invalid value
        SessionFile? file = Parse(Json(session =>
        {
            if (change == "missing")
            {
                session.Remove(key);
            }
            else
            {
                session[key] = change switch
                {
                    "empty" => JsonValue.Create(string.Empty),
                    "blank" => JsonValue.Create("   "),
                    "number" => JsonValue.Create(1234),
                    "relative" => JsonValue.Create(Path.Combine("invented", "project")),
                    "letters" => JsonValue.Create("1343786a7888"),
                    _ => JsonValue.Create("-134378657888122155"),
                };
            }
        }), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        file.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain($"key: {key} is missing");
    }

    /// <summary>
    /// A file whose start is missing, not a whole number, or out of the range of the dates is skipped and reported
    /// </summary>
    /// <param name="change">Kind of invalid start</param>
    [Theory]
    [InlineData("missing")]
    [InlineData("text")]
    [InlineData("decimal")]
    [InlineData("too large")]
    [InlineData("too small")]
    public void InvalidStartSkipsTheFile(string change)
    {
        // Building the invalid start
        SessionFile? file = Parse(Json(session =>
        {
            if (change == "missing")
            {
                session.Remove("startedAt");
            }
            else
            {
                session["startedAt"] = change switch
                {
                    "text" => JsonValue.Create("1793392190183"),
                    "decimal" => JsonValue.Create(1793392190183.5),
                    "too large" => JsonValue.Create(DateTimeOffset.MaxValue.ToUnixTimeMilliseconds() + 1),
                    _ => JsonValue.Create(DateTimeOffset.MinValue.ToUnixTimeMilliseconds() - 1),
                };
            }
        }), out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        file.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldContain("key: startedAt is missing");
    }

    /// <summary>
    /// The first and the last dates of the range are read
    /// </summary>
    [Fact]
    public void StartAtTheEdgesOfTheRangeIsRead()
    {
        // Parsing the two edges of the range
        SessionFile? first = Parse(Json(session => session["startedAt"] = DateTimeOffset.MinValue.ToUnixTimeMilliseconds()), out List<ReadProblem> firstProblems);
        SessionFile? last = Parse(Json(session => session["startedAt"] = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()), out List<ReadProblem> lastProblems);

        // Checking that both are read
        first.ShouldNotBeNull();
        last.ShouldNotBeNull();
        firstProblems.ShouldBeEmpty();
        lastProblems.ShouldBeEmpty();
    }

    /// <summary>
    /// A text that is not a JSON object is skipped and reported
    /// </summary>
    /// <param name="text">Text that is not a JSON object</param>
    /// <param name="reason">Part of the reason expected</param>
    [Theory]
    [InlineData("", "not valid JSON")]
    [InlineData("{ \"pid\": 1234", "not valid JSON")]
    [InlineData("{ \"pid\": 1234 } trailing", "not valid JSON")]
    [InlineData("[ 1234 ]", "not a JSON object")]
    [InlineData("\"text\"", "not a JSON object")]
    public void TextThatIsNotAJsonObjectIsSkipped(string text, string reason)
    {
        // Parsing the text
        SessionFile? file = Parse(text, out List<ReadProblem> problems);

        // Checking that the file is skipped and reported
        file.ShouldBeNull();
        problems.Count.ShouldBe(1);
        problems[0].FilePath.ShouldBe(_filePath);
        problems[0].Reason.ShouldContain(reason);
    }

    /// <summary>
    /// A problem never holds a value of the file
    /// </summary>
    [Fact]
    public void ProblemNeverHoldsAValueOfTheFile()
    {
        // Parsing a file whose identifier is valid and whose start time holds an invented secret
        Parse(Json(session => session["procStart"] = "invented-secret"), out List<ReadProblem> problems);

        // Checking the reason
        problems.Count.ShouldBe(1);
        problems[0].Reason.ShouldNotContain("invented-secret");
        problems[0].Reason.ShouldNotContain("invented-session");
    }

    /// <summary>
    /// The arguments of the parser are required
    /// </summary>
    [Fact]
    public void ArgumentsCannotBeNull()
    {
        // Calling the parser without each argument
        ArgumentNullException withoutPath = Should.Throw<ArgumentNullException>(() => SessionFileParser.Parse(" ", "{}", new List<ReadProblem>()));
        ArgumentNullException withoutText = Should.Throw<ArgumentNullException>(() => SessionFileParser.Parse(_filePath, null!, new List<ReadProblem>()));
        ArgumentNullException withoutProblems = Should.Throw<ArgumentNullException>(() => SessionFileParser.Parse(_filePath, "{}", null!));

        // Checking the parameters in fault
        withoutPath.ParamName.ShouldBe("filePath");
        withoutText.ParamName.ShouldBe("text");
        withoutProblems.ParamName.ShouldBe("problems");
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
