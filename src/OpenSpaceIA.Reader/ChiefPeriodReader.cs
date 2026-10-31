/*
 *	@file	src/OpenSpaceIA.Reader/ChiefPeriodReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the periods of work and of waiting of the chief, in its transcripts
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
/// Service used to read the periods of work and of waiting of the chief, in the transcripts of its sessions
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the chief is one agent of the office, whatever its session; its periods over the window feed
/// "overloaded" and "under-used" (doc/analyse/state-definitions.md, § 3 to § 5).</para>
/// <para><b>Behavior:</b> the transcripts of the chief are the files <c>projects/&lt;project&gt;/&lt;session&gt;.jsonl</c>
/// last written since the start of the window; the transcripts of the sub-agents (deeper) are not, a <c>memory</c> folder
/// (in any case) is never read, and a folder that is a link is reported and not followed. Each transcript is followed with
/// <see cref="TranscriptFollowing"/> by a <see cref="ChiefPeriodTracker"/>, from where the previous reading of this instance
/// stopped; a record dated more than five minutes after now is not read. A transcript that cannot be read keeps the periods
/// read before and is reported, and its cursor is forgotten, so that the next reading starts again cleanly. The lines that
/// cannot be read so far, and the turns without project, are reported in one problem each per transcript and per reading.
/// A session resumed in a new transcript copies the history of the old one: the same period (same project, kind, start and
/// end) is kept once. The cursors of the transcripts not seen this time are forgotten. Nothing is ever written. One instance
/// is not meant to be
/// used by two threads at the same time.</para>
/// </remarks>
internal sealed class ChiefPeriodReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of the transcripts, in the folder of Claude Code
    /// </summary>
    private const string ProjectsFolderName = "projects";

    /// <summary>
    /// Name of the folders of notes, never read
    /// </summary>
    private const string MemoryFolderName = "memory";

    /// <summary>
    /// End of the name of a transcript
    /// </summary>
    private const string TranscriptExtension = ".jsonl";

    #endregion

    #region Variables

    /// <summary>
    /// Margin added to now for the latest time accepted in a transcript, so that a small gap between the clocks is not refused
    /// </summary>
    private static readonly TimeSpan _clockMargin = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Reader of the files of Claude Code
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

    /// <summary>
    /// Where the reading of each transcript stopped, by full path of the transcript
    /// </summary>
    private readonly Dictionary<string, TranscriptCursor<ChiefPeriodTracker>> _cursors = new Dictionary<string, TranscriptCursor<ChiefPeriodTracker>>(StringComparer.Ordinal);

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
    public ChiefPeriodReader(ClaudeFileReader fileReader)
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
    /// Method used to list the transcripts of the chief written since the start of the window
    /// </summary>
    /// <param name="sinceUtc">Start of the window</param>
    /// <param name="problems">[Cannot be null] Problems, where a folder that cannot be listed or a link is added</param>
    /// <returns>The transcripts, project after project, each in the ordinal order of the names</returns>
    private List<FileInfo> ListTranscripts(DateTimeOffset sinceUtc, List<ReadProblem> problems)
    {
        // Refusing a projects folder that is a link
        List<FileInfo> transcripts = new List<FileInfo>();
        string projectsFolder = Path.Join(_fileReader.Folder.RootPath, ProjectsFolderName);
        if (FolderListing.IsLinkedFolder(projectsFolder))
        {
            problems.Add(new ReadProblem(projectsFolder, "The projects folder is a junction, a symbolic link or another reparse point: it is not followed, so no period of the chief is read"));

            // Function's return
            return transcripts;
        }

        // Keeping the transcripts at the top of each project folder that is not a link nor a memory folder
        foreach (FileSystemInfo project in FolderListing.ListEntries(projectsFolder, problems))
        {
            if (project is not DirectoryInfo || string.Equals(project.Name, MemoryFolderName, StringComparison.OrdinalIgnoreCase))
                continue;

            string projectFolder = Path.Join(projectsFolder, project.Name);
            if (FolderListing.IsLink(project.Attributes))
            {
                problems.Add(new ReadProblem(projectFolder, "The folder is a junction, a symbolic link or another reparse point: it is not followed"));
                continue;
            }
            transcripts.AddRange(FolderListing.ListEntries(projectFolder, problems).OfType<FileInfo>()
                .Where(file => file.Name.EndsWith(TranscriptExtension, StringComparison.Ordinal) && file.LastWriteTimeUtc >= sinceUtc.UtcDateTime));
        }

        // Function's return
        return transcripts;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the periods of the chief in the window
    /// </summary>
    /// <param name="sinceUtc">Start of the window: older transcripts are not read, older periods are left out</param>
    /// <param name="nowUtc">Now, in UTC: a record more than five minutes later is not true, and is not read</param>
    /// <returns>The periods of the chief, and the problems met</returns>
    /// <exception cref="ArgumentOutOfRangeException">If now is not in UTC</exception>
    public ChiefPeriods Read(DateTimeOffset sinceUtc, DateTimeOffset nowUtc)
    {
        // Check
        if (nowUtc.Offset != TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(nowUtc), nowUtc, $"The value of the parameter: {nameof(nowUtc)} must be in UTC");

        // Following each transcript of the window
        DateTimeOffset latestAcceptedUtc = nowUtc + _clockMargin;
        List<ActivityPeriod> periods = new List<ActivityPeriod>();
        List<ReadProblem> problems = new List<ReadProblem>();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (FileInfo transcript in ListTranscripts(sinceUtc, problems))
        {
            // Reading the new lines; a failure keeps what was read and forgets the cursor
            string path = Path.Join(transcript.DirectoryName, transcript.Name);
            seen.Add(path);
            if (!_cursors.TryGetValue(path, out TranscriptCursor<ChiefPeriodTracker>? cursor))
            {
                cursor = new TranscriptCursor<ChiefPeriodTracker>();
                _cursors[path] = cursor;
            }
            LineReadResult result = TranscriptFollowing.Follow(_fileReader, path, transcript.Length, cursor, tracker => tracker.LatestAcceptedUtc = latestAcceptedUtc);
            if (!result.IsSuccess)
            {
                problems.Add(new ReadProblem(path, $"The transcript cannot be read ({result.Failure}): {result.Detail}; the periods read before are kept"));
                _cursors.Remove(path);
            }

            // Adding the periods, and reporting the doubts
            periods.AddRange(cursor.Follower.GetPeriods(sinceUtc));
            int unreadable = cursor.Follower.UnreadableLineCount + cursor.TooLongLineCount;
            if (unreadable > 0)
            {
                problems.Add(new ReadProblem(path, string.Create(CultureInfo.InvariantCulture,
                    $"{unreadable} lines of the transcript cannot be read ({cursor.TooLongLineCount} longer than the limit): they are left out, so a period may be missing or too long")));
            }
            if (cursor.Follower.TurnWithoutProjectCount > 0)
            {
                problems.Add(new ReadProblem(path, string.Create(CultureInfo.InvariantCulture,
                    $"{cursor.Follower.TurnWithoutProjectCount} turns have no working folder, so their project is unknown: they are left out")));
            }
        }

        // Forgetting the cursors of the transcripts not seen this time
        foreach (string path in _cursors.Keys.Where(path => !seen.Contains(path)).ToList())
            _cursors.Remove(path);

        // Keeping once a period found in two transcripts (a session resumed in a new transcript copies the old history)
        List<ActivityPeriod> distinctPeriods = periods.Distinct().ToList();

        // Function's return
        return new ChiefPeriods(distinctPeriods, problems);
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
