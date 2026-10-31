/*
 *	@file	src/OpenSpaceIA.Reader/WaitReasonReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the reason why each chief waits for the user
 */

// Using
using System.Globalization;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the reason why each waiting chief waits for the user: a question or a permission
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the status <c>waiting</c> of a session says that Claude waits for the user; its transcript says why
/// (doc/analyse/claude-model-decisions.md, decision 2).</para>
/// <para><b>Behavior:</b> only the sessions whose status is waiting are read. The transcript of a session is
/// <c>projects/&lt;project&gt;/&lt;session&gt;.jsonl</c>; it is searched in every project folder, never in a <c>memory</c>
/// folder (in any case) nor through a link. It is read with <see cref="ClaudeFileReader.ReadCompleteLines"/>, from where the
/// previous reading of this instance stopped: only its new complete lines are read, and a transcript that became shorter is
/// read again from its start. The reason is given by <see cref="OpenToolCalls"/>. A waiting session always gets a reason:
/// when its transcript is missing or cannot be read, it gets a question (the decision for a wait without open call) and the
/// doubt is reported. The lines that cannot be read are reported once per transcript, with their number. The cursors of the
/// transcripts that were not read this time are forgotten. Nothing is ever written. One instance is not meant to be used by
/// two threads at the same time.</para>
/// </remarks>
internal sealed class WaitReasonReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of the transcripts, in the folder of Claude Code
    /// </summary>
    private const string ProjectsFolderName = "projects";

    /// <summary>
    /// Name of the folders of notes, never searched
    /// </summary>
    private const string MemoryFolderName = "memory";

    /// <summary>
    /// End of the name of a transcript
    /// </summary>
    private const string TranscriptExtension = ".jsonl";

    #endregion

    #region Variables

    /// <summary>
    /// Reader of the files of Claude Code
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

    /// <summary>
    /// Where the reading of each transcript stopped, by full path of the transcript
    /// </summary>
    private readonly Dictionary<string, TranscriptCursor> _cursors = new Dictionary<string, TranscriptCursor>(StringComparer.Ordinal);

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
    /// <exception cref="ArgumentNullException">If the reader of the files is null</exception>
    public WaitReasonReader(ClaudeFileReader fileReader)
    {
        // Check
        if (fileReader is null)
            throw new ArgumentNullException(nameof(fileReader), $"The value of the parameter: {nameof(fileReader)} cannot be null");

        // Setting the value
        _fileReader = fileReader;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to find the transcript of a session in the project folders
    /// </summary>
    /// <param name="sessionId">[Cannot be null] Identifier of the session, made of letters, digits, hyphens and underscores</param>
    /// <param name="problems">[Cannot be null] Problems, where a folder that cannot be listed is added</param>
    /// <returns>[Can be null] The transcript; null when no project folder holds it</returns>
    private FileInfo? FindTranscript(string sessionId, List<ReadProblem> problems)
    {
        // Looking in each project folder that is not a link nor a memory folder
        string projectsFolder = Path.Join(_fileReader.Folder.RootPath, ProjectsFolderName);
        if (FolderListing.IsLinkedFolder(projectsFolder))
        {
            // Function's return
            return null;
        }
        foreach (FileSystemInfo project in FolderListing.ListEntries(projectsFolder, problems))
        {
            if (project is not DirectoryInfo || FolderListing.IsLink(project.Attributes)
                || string.Equals(project.Name, MemoryFolderName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            FileInfo transcript = new FileInfo(Path.Join(projectsFolder, project.Name, sessionId + TranscriptExtension));
            if (transcript.Exists)
            {
                // Function's return
                return transcript;
            }
        }

        // Function's return
        return null;
    }

    /// <summary>
    /// Method used to read the reason of the wait of a session
    /// </summary>
    /// <param name="session">[Cannot be null] Waiting session</param>
    /// <param name="problems">[Cannot be null] Problems, where every doubt is added</param>
    /// <param name="used">[Cannot be null] Full paths of the transcripts read, so that the other cursors are forgotten</param>
    /// <returns>The reason of the wait</returns>
    private WaitReason ReadReason(LiveSession session, List<ReadProblem> problems, HashSet<string> used)
    {
        // Finding the transcript, or giving a question
        FileInfo? transcript = FindTranscript(session.Facts.SessionId, problems);
        if (transcript is null)
        {
            problems.Add(new ReadProblem(Path.Join(_fileReader.Folder.RootPath, ProjectsFolderName),
                "The waiting session has no transcript in the project folders: the reason of its wait is taken as a question"));

            // Function's return
            return WaitReason.Question;
        }

        // Reading the transcript from where the previous reading stopped, from its start when it became shorter
        string path = transcript.FullName;
        used.Add(path);
        if (!_cursors.TryGetValue(path, out TranscriptCursor? cursor))
        {
            cursor = new TranscriptCursor();
            _cursors[path] = cursor;
        }
        cursor.RestartIfShorter(transcript.Length);
        LineReadResult result = _fileReader.ReadCompleteLines(path, cursor.Offset, cursor.Calls.Add);
        if (!result.IsSuccess)
        {
            problems.Add(new ReadProblem(path, $"The transcript cannot be read ({result.Failure}): {result.Detail}; the reason of the wait is taken from what was read before"));

            // Function's return
            return cursor.Calls.GetWaitReason();
        }
        cursor.MoveTo(result.EndOffset);

        // Reporting the lines that cannot be read
        int unreadable = cursor.Calls.UnreadableLineCount + result.TooLongLineCount;
        if (unreadable > 0)
        {
            problems.Add(new ReadProblem(path, string.Create(CultureInfo.InvariantCulture,
                $"{unreadable} lines of the transcript cannot be read ({result.TooLongLineCount} longer than the limit in this reading): they are left out")));
        }

        // Function's return
        return cursor.Calls.GetWaitReason();
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the reason of the wait of each waiting session
    /// </summary>
    /// <param name="sessions">[Cannot be null] Open sessions; only the waiting ones are read</param>
    /// <returns>The reason of each waiting session, and the problems met</returns>
    /// <exception cref="ArgumentNullException">If the sessions are null</exception>
    public WaitReasons Read(IReadOnlyList<LiveSession> sessions)
    {
        // Check
        if (sessions is null)
            throw new ArgumentNullException(nameof(sessions), $"The value of the parameter: {nameof(sessions)} cannot be null");

        // Reading the reason of each waiting session
        Dictionary<string, WaitReason> reasons = new Dictionary<string, WaitReason>(StringComparer.Ordinal);
        List<ReadProblem> problems = new List<ReadProblem>();
        HashSet<string> used = new HashSet<string>(StringComparer.Ordinal);
        foreach (LiveSession session in sessions.Where(session => session.Facts.Status == SessionStatus.Waiting))
            reasons[session.Facts.SessionId] = ReadReason(session, problems, used);

        // Forgetting the cursors of the transcripts not read this time
        foreach (string path in _cursors.Keys.Where(path => !used.Contains(path)).ToList())
            _cursors.Remove(path);

        // Function's return
        return new WaitReasons(reasons, problems);
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
