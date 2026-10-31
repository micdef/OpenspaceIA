/*
 *	@file	src/OpenSpaceIA.Reader/SystemProcessProbe.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Asks Windows or Linux whether the process of a session is still running
 */

// Using
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to ask Windows or Linux whether the process of a session of Claude Code is still running
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> tells a live session from the file of a closed session whose process ID was given to another
/// program (doc/analyse/claude-model-decisions.md, decision 3).</para>
/// <para><b>Behavior:</b> the start time written by Claude Code (<c>procStart</c>) is compared exactly with the start time
/// given by the system, as measured on 2026-10-31. On Windows, it is the start of the process as a FILETIME (100 ns since
/// 1601, UTC), compared with <see cref="Process.StartTime"/>. On Linux, it is field 22 of <c>/proc/&lt;pid&gt;/stat</c>
/// (clock ticks since the boot), read and compared as text; this file is public, it holds no secret; a zombie is closed. A
/// process that the system refuses to show is unknown, not closed (on Windows, a process of another user or of an elevated
/// session). On any other system, the answer is unknown. Nothing is ever changed: the process is only looked at. Known
/// limits, not verified: the key <c>pidDomain</c> of the session file is not read, so a file left from before a reboot of
/// Linux, whose process ID and start time match a new process, would be alive; and a <c>/proc</c> that hides the processes
/// (<c>hidepid</c>) or another namespace of processes would make every session closed.</para>
/// </remarks>
internal sealed class SystemProcessProbe : IProcessProbe
{
    #region Members

    #region Constants

    /// <summary>
    /// Index of the start time (field 22) among the fields that follow the name of the process in <c>/proc/&lt;pid&gt;/stat</c>
    /// (the first field after the name is field 3)
    /// </summary>
    private const int LinuxStartFieldIndex = 19;

    /// <summary>
    /// Index of the state of the process (field 3) among the fields that follow its name in <c>/proc/&lt;pid&gt;/stat</c>
    /// </summary>
    private const int LinuxStateFieldIndex = 0;

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
    /// Method used to check a process on Windows
    /// </summary>
    /// <param name="processId">ID of the process</param>
    /// <param name="processStart">[Cannot be null] Start time written by Claude Code: a FILETIME</param>
    /// <param name="detail">[Can be null] Why the answer is unknown; null otherwise</param>
    /// <returns>Whether the process runs and started at this time</returns>
    private static ProcessLiveness CheckOnWindows(int processId, string processStart, out string? detail)
    {
        // Refusing a start time that is not a FILETIME
        detail = null;
        if (!long.TryParse(processStart, NumberStyles.None, CultureInfo.InvariantCulture, out long fileTime))
        {
            detail = "The start time of the session is not a FILETIME of Windows";

            // Function's return
            return ProcessLiveness.Unknown;
        }

        try
        {
            // Comparing the start time of the process with the one of the session
            using Process process = Process.GetProcessById(processId);

            // Function's return
            return process.StartTime.ToFileTimeUtc() == fileTime ? ProcessLiveness.Alive : ProcessLiveness.Closed;
        }
        catch (ArgumentException)
        {
            // Function's return: no process has this ID
            return ProcessLiveness.Closed;
        }
        catch (InvalidOperationException)
        {
            // Function's return: the process has ended meanwhile
            return ProcessLiveness.Closed;
        }
        catch (Win32Exception exception)
        {
            detail = $"The system refuses to give the start time of the process: {FolderListing.Describe(exception)}";

            // Function's return
            return ProcessLiveness.Unknown;
        }
    }

    /// <summary>
    /// Method used to check a process on Linux
    /// </summary>
    /// <param name="processId">ID of the process</param>
    /// <param name="processStart">[Cannot be null] Start time written by Claude Code: field 22 of the stat file</param>
    /// <param name="detail">[Can be null] Why the answer is unknown; null otherwise</param>
    /// <returns>Whether the process runs and started at this time</returns>
    private static ProcessLiveness CheckOnLinux(int processId, string processStart, out string? detail)
    {
        // Reading the stat file of the process
        detail = null;
        string stat;
        try
        {
            stat = File.ReadAllText($"/proc/{processId.ToString(CultureInfo.InvariantCulture)}/stat");
        }
        catch (FileNotFoundException)
        {
            // Function's return: no process has this ID
            return ProcessLiveness.Closed;
        }
        catch (DirectoryNotFoundException)
        {
            // Function's return: no process has this ID
            return ProcessLiveness.Closed;
        }
        catch (UnauthorizedAccessException exception)
        {
            detail = $"The system refuses to show the process: {FolderListing.Describe(exception)}";

            // Function's return
            return ProcessLiveness.Unknown;
        }
        catch (IOException exception)
        {
            detail = $"The stat file of the process cannot be read (the process may have ended meanwhile): {FolderListing.Describe(exception)}";

            // Function's return
            return ProcessLiveness.Unknown;
        }

        // Function's return
        return CompareLinuxStat(stat, processStart, out detail);
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to compare the text of a stat file of Linux with the start time written by Claude Code
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the part of the check of Linux that needs no running process, so that it can be tested on every
    /// system.</para>
    /// <para><b>Behavior:</b> the name of the process (field 2) is between brackets and may hold spaces and brackets: the
    /// fields are read after the last closing bracket. A zombie (state <c>Z</c>) or a dead process (state <c>X</c>) has
    /// ended, even when its start time is the same: it is closed.</para>
    /// </remarks>
    /// <param name="stat">[Cannot be null] Text of <c>/proc/&lt;pid&gt;/stat</c></param>
    /// <param name="processStart">[Cannot be null] Start time written by Claude Code</param>
    /// <param name="detail">[Can be null] Why the answer is unknown; null otherwise</param>
    /// <returns>
    /// Alive   ==> The process runs and started at this time<br/>
    /// Closed  ==> The process started at another time, or it has ended<br/>
    /// Unknown ==> The text does not have the expected fields
    /// </returns>
    public static ProcessLiveness CompareLinuxStat(string stat, string processStart, out string? detail)
    {
        // Taking the fields after the name of the process
        detail = null;
        int endOfName = stat.LastIndexOf(')');
        string[] fields = endOfName < 0 ? Array.Empty<string>() : stat.Substring(endOfName + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length <= LinuxStartFieldIndex)
        {
            detail = "The stat file of the process does not have the expected fields";

            // Function's return
            return ProcessLiveness.Unknown;
        }

        // Closing a process that has ended, then comparing the start times
        if (fields[LinuxStateFieldIndex] == "Z" || fields[LinuxStateFieldIndex] == "X")
        {
            // Function's return
            return ProcessLiveness.Closed;
        }

        // Function's return
        return string.Equals(fields[LinuxStartFieldIndex], processStart, StringComparison.Ordinal) ? ProcessLiveness.Alive : ProcessLiveness.Closed;
    }

    /// <inheritdoc/>
    public ProcessLiveness Check(int processId, string processStart, out string? detail)
    {
        // Checks
        if (processId <= 0)
            throw new ArgumentOutOfRangeException(nameof(processId), processId, $"The value of the parameter: {nameof(processId)} must be greater than 0");
        if (string.IsNullOrEmpty(processStart) || !processStart.All(char.IsAsciiDigit))
            throw new ArgumentException($"The value of the parameter: {nameof(processStart)} must hold digits only", nameof(processStart));

        // Asking the system of the machine
        if (OperatingSystem.IsWindows())
        {
            // Function's return
            return CheckOnWindows(processId, processStart, out detail);
        }
        if (OperatingSystem.IsLinux())
        {
            // Function's return
            return CheckOnLinux(processId, processStart, out detail);
        }

        // Function's return: another system is not supported
        detail = "The system of the machine is not supported: only Windows and Linux are";
        return ProcessLiveness.Unknown;
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
