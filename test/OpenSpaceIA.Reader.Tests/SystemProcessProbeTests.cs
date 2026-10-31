/*
 *	@file	test/OpenSpaceIA.Reader.Tests/SystemProcessProbeTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the check of a process on the real system
 */

// Using
using System.Diagnostics;
using System.Globalization;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the check of a process on the real system (Windows or Linux)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a running process is alive only with its own start time, and that a process ID that
/// no process has is closed, on the system that runs the tests.</para>
/// <para><b>Behavior:</b> the process of the tests is the running process; its start time is read from the system the way
/// Claude Code writes it (a FILETIME on Windows, field 22 of <c>/proc/self/stat</c> on Linux).</para>
/// </remarks>
public sealed class SystemProcessProbeTests
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
    /// Method used to get the start time of the process of the tests, as Claude Code writes it on this system
    /// </summary>
    /// <returns>The start time: a FILETIME on Windows, field 22 of the stat file on Linux</returns>
    private static string StartOfTheCurrentProcess()
    {
        // Reading the start on Windows
        if (OperatingSystem.IsWindows())
        {
            using Process current = Process.GetCurrentProcess();

            // Function's return
            return current.StartTime.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture);
        }

        // Reading field 22 on Linux, after the name of the process
        string stat = File.ReadAllText($"/proc/{Environment.ProcessId.ToString(CultureInfo.InvariantCulture)}/stat");

        // Function's return
        return stat.Substring(stat.LastIndexOf(')') + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries)[19];
    }

    #endregion

    #region Publics

    /// <summary>
    /// The running process, with its own start time, is alive
    /// </summary>
    [Fact]
    public void RunningProcessWithItsStartIsAlive()
    {
        // Skipping on a system that is not supported
        Assert.SkipUnless(OperatingSystem.IsWindows() || OperatingSystem.IsLinux(), "Only Windows and Linux are supported");

        // Checking the process of the tests
        ProcessLiveness liveness = new SystemProcessProbe().Check(Environment.ProcessId, StartOfTheCurrentProcess(), out string? detail);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Alive);
        detail.ShouldBeNull();
    }

    /// <summary>
    /// The running process with another start time is closed: its ID was given to another program
    /// </summary>
    [Fact]
    public void RunningProcessWithAnotherStartIsClosed()
    {
        // Skipping on a system that is not supported
        Assert.SkipUnless(OperatingSystem.IsWindows() || OperatingSystem.IsLinux(), "Only Windows and Linux are supported");

        // Checking the process of the tests with a start one unit later
        string start = StartOfTheCurrentProcess();
        string otherStart = (ulong.Parse(start, CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture);
        ProcessLiveness liveness = new SystemProcessProbe().Check(Environment.ProcessId, otherStart, out string? detail);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Closed);
        detail.ShouldBeNull();
    }

    /// <summary>
    /// The running process with a start one unit earlier is closed too: the comparison is exact on both sides
    /// </summary>
    [Fact]
    public void RunningProcessWithAnEarlierStartIsClosed()
    {
        // Skipping on a system that is not supported
        Assert.SkipUnless(OperatingSystem.IsWindows() || OperatingSystem.IsLinux(), "Only Windows and Linux are supported");

        // Checking the process of the tests with a start one unit earlier
        string start = StartOfTheCurrentProcess();
        string earlierStart = (ulong.Parse(start, CultureInfo.InvariantCulture) - 1).ToString(CultureInfo.InvariantCulture);
        ProcessLiveness liveness = new SystemProcessProbe().Check(Environment.ProcessId, earlierStart, out _);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Closed);
    }

    /// <summary>
    /// A stat line of Linux is read after the last closing bracket, so a name with spaces and brackets does not shift the
    /// fields; a zombie or a dead process is closed; a line without the expected fields is unknown
    /// </summary>
    /// <param name="name">Name of the invented process</param>
    /// <param name="state">State of the invented process</param>
    /// <param name="start">Start time in the invented line</param>
    /// <param name="livenessValue">Answer expected (an integer, because the enumeration is internal)</param>
    [Theory]
    [InlineData("claude", "S", "4279", (int)ProcessLiveness.Alive)]
    [InlineData("a) b 1 2 3", "R", "4279", (int)ProcessLiveness.Alive)]
    [InlineData("(x) (y)", "S", "4279", (int)ProcessLiveness.Alive)]
    [InlineData("claude", "S", "4280", (int)ProcessLiveness.Closed)]
    [InlineData("claude", "Z", "4279", (int)ProcessLiveness.Closed)]
    [InlineData("claude", "X", "4279", (int)ProcessLiveness.Closed)]
    public void StatLineOfLinuxIsCompared(string name, string state, string start, int livenessValue)
    {
        // Building a stat line of 52 fields, the start time being field 22
        string line = $"327 ({name}) {state} 1 327 327 0 -1 4194560 100 0 0 0 5 2 0 0 20 0 7 0 {start} 1000000 2000 18446744073709551615 1 1 0 0 0 0 0 0 0 0 0 0 17 3 0 0 0 0 0 1 1 1 1 1 1 1 0\n";

        // Comparing it with the start time of the session
        ProcessLiveness liveness = SystemProcessProbe.CompareLinuxStat(line, "4279", out string? detail);

        // Checking the answer
        liveness.ShouldBe((ProcessLiveness)livenessValue);
        detail.ShouldBeNull();
    }

    /// <summary>
    /// A stat text without the expected fields is unknown, with its reason
    /// </summary>
    /// <param name="stat">Text without the expected fields</param>
    [Theory]
    [InlineData("")]
    [InlineData("327 claude S 1 327 327 0 -1 4194560 100 0 0 0 5 2 0 0 20 0 7 0 4279 1000000")]
    [InlineData("327 (claude) S 1 327 327 0 -1 4194560 100 0 0 0 5 2 0 0 20 0 7 0")]
    public void StatTextWithoutTheExpectedFieldsIsUnknown(string stat)
    {
        // Comparing the text with a start time
        ProcessLiveness liveness = SystemProcessProbe.CompareLinuxStat(stat, "4279", out string? detail);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Unknown);
        detail.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// The shortest stat line that holds field 22 is read
    /// </summary>
    [Fact]
    public void ShortestStatLineWithTheStartIsRead()
    {
        // Comparing a line that ends with field 22
        ProcessLiveness liveness = SystemProcessProbe.CompareLinuxStat("327 (claude) S 1 327 327 0 -1 4194560 100 0 0 0 5 2 0 0 20 0 7 0 4279", "4279", out _);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Alive);
    }

    /// <summary>
    /// A process ID that no process has is closed
    /// </summary>
    [Fact]
    public void ProcessIdOfNoProcessIsClosed()
    {
        // Skipping on a system that is not supported
        Assert.SkipUnless(OperatingSystem.IsWindows() || OperatingSystem.IsLinux(), "Only Windows and Linux are supported");

        // Checking the largest process ID, which no system gives
        ProcessLiveness liveness = new SystemProcessProbe().Check(int.MaxValue, "1", out string? detail);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Closed);
        detail.ShouldBeNull();
    }

    /// <summary>
    /// On Windows, a start time too large for a FILETIME is unknown, with its reason, not closed
    /// </summary>
    [Fact]
    public void StartTooLargeForAFileTimeIsUnknownOnWindows()
    {
        // Skipping where the start time is not a FILETIME
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The start time is a FILETIME on Windows only");

        // Checking the process of the tests with a start time of 20 digits
        ProcessLiveness liveness = new SystemProcessProbe().Check(Environment.ProcessId, "99999999999999999999", out string? detail);

        // Checking the answer
        liveness.ShouldBe(ProcessLiveness.Unknown);
        detail.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// A process ID must be greater than 0
    /// </summary>
    /// <param name="processId">Process ID that must be refused</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ProcessIdMustBeGreaterThanZero(int processId)
    {
        // Checking an invalid process ID
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new SystemProcessProbe().Check(processId, "1", out _));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("processId");
    }

    /// <summary>
    /// A start time must hold digits only
    /// </summary>
    /// <param name="processStart">[Can be null] Start time that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12a")]
    [InlineData("-1")]
    [InlineData(" 1")]
    [InlineData("١٢")]
    public void ProcessStartMustHoldDigitsOnly(string? processStart)
    {
        // Checking an invalid start time
        ArgumentException exception = Should.Throw<ArgumentException>(() => new SystemProcessProbe().Check(1, processStart!, out _));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("processStart");
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
