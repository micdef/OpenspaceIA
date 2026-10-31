/*
 *	@file	src/OpenSpaceIA.Reader/LiveSessionReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the open sessions of Claude Code
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the open sessions of Claude Code, in the folder <c>sessions</c>
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> each open session is a chief of the office (doc/analyse/claude-model-decisions.md, decisions 1 and
/// 3); the file of a closed session must never bring a chief back.</para>
/// <para><b>Behavior:</b> lists the top of the sessions folder in the ordinal order of the names, and reads each
/// <c>.json</c> file with <see cref="ClaudeFileReader"/>; every other file (the <c>.key</c> files above all) is never
/// opened. A subfolder is reported and not read. A sessions folder that is a junction, a symbolic link or another reparse
/// point is reported and not read. A file deleted between the listing and its reading is a session that has just ended
/// (Claude Code deletes the file at the end of a session): it is left out without a problem. Each session is then checked with the <see cref="IProcessProbe"/>: a closed session is
/// left out without a problem (its file stays behind after a crash), an alive one is kept, and one whose process cannot be
/// checked is kept and reported. When nothing exists at the place of the sessions folder, there is no session and no
/// problem. Nothing is ever written.</para>
/// </remarks>
internal sealed class LiveSessionReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of the sessions, in the folder of Claude Code
    /// </summary>
    private const string SessionsFolderName = "sessions";

    /// <summary>
    /// Extension of a session file
    /// </summary>
    private const string SessionExtension = ".json";

    #endregion

    #region Variables

    /// <summary>
    /// Reader of the files of Claude Code
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

    /// <summary>
    /// Port used to know whether the process of a session still runs
    /// </summary>
    private readonly IProcessProbe _processProbe;

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
    /// <param name="fileReader">[Cannot be null] Reader of the files of Claude Code</param>
    /// <param name="processProbe">[Cannot be null] Port used to know whether the process of a session still runs</param>
    /// <exception cref="ArgumentNullException">If the reader of the files or the port is null</exception>
    public LiveSessionReader(ClaudeFileReader fileReader, IProcessProbe processProbe)
    {
        // Checks
        if (fileReader is null)
            throw new ArgumentNullException(nameof(fileReader), $"The value of the parameter: {nameof(fileReader)} cannot be null");
        if (processProbe is null)
            throw new ArgumentNullException(nameof(processProbe), $"The value of the parameter: {nameof(processProbe)} cannot be null");

        // Setting the values
        _fileReader = fileReader;
        _processProbe = processProbe;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    // #N/A

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the open sessions
    /// </summary>
    /// <returns>The open sessions, and the problems met</returns>
    public LiveSessions Read()
    {
        // Refusing a sessions folder that is a link
        List<LiveSession> sessions = new List<LiveSession>();
        List<ReadProblem> problems = new List<ReadProblem>();
        string sessionsFolder = Path.Combine(_fileReader.Folder.RootPath, SessionsFolderName);
        if (FolderListing.IsLinkedFolder(sessionsFolder))
        {
            problems.Add(new ReadProblem(sessionsFolder, "The sessions folder is a junction, a symbolic link or another reparse point: it is not followed, so no session is read"));

            // Function's return
            return new LiveSessions(sessions, problems);
        }

        foreach (FileSystemInfo entry in FolderListing.ListEntries(sessionsFolder, problems))
        {
            // Reporting a subfolder, which Claude Code does not create there
            string entryPath = Path.Join(sessionsFolder, entry.Name);
            if (entry is DirectoryInfo)
            {
                problems.Add(new ReadProblem(entryPath, "A subfolder of the sessions folder is not read"));
                continue;
            }

            // Leaving out every file that is not a session file, without opening it
            if (!string.Equals(entry.Extension, SessionExtension, StringComparison.Ordinal))
                continue;

            // Reading the file, and reporting a failure; a file deleted since the listing is a session that has just ended
            FileReadResult result = _fileReader.ReadText(entryPath);
            if (result.Failure == FileReadFailure.NotFound)
                continue;
            if (!result.IsSuccess)
            {
                problems.Add(new ReadProblem(entryPath, $"The session file cannot be read ({result.Failure}): {result.Detail}"));
                continue;
            }

            // Parsing the file, whose problems are added by the parser
            SessionFile? file = SessionFileParser.Parse(entryPath, result.Content!, problems);
            if (file is null)
                continue;

            // Keeping a session whose process runs, or cannot be checked; leaving out a closed one
            ProcessLiveness liveness = _processProbe.Check(file.ProcessId, file.ProcessStart, out string? detail);
            if (liveness == ProcessLiveness.Closed)
                continue;
            if (liveness == ProcessLiveness.Unknown)
                problems.Add(new ReadProblem(entryPath, $"The process of the session cannot be checked, so the session is kept: {detail}"));

            sessions.Add(new LiveSession(file, liveness));
        }

        // Function's return
        return new LiveSessions(sessions, problems);
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
