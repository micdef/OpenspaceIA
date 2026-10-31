/*
 *	@file	src/OpenSpaceIA.Reader/SubagentRunReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the runs of the sub-agents of Claude Code
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
/// Service used to read the runs of the sub-agents of Claude Code, in the folder <c>projects</c>
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> each run is a period of work of an agent type, and a run in progress makes its agent type work
/// (doc/analyse/state-definitions.md, § 2 and § 5; doc/analyse/claude-model-decisions.md, decision 4).</para>
/// <para><b>Behavior:</b> walks <c>projects/&lt;project&gt;/&lt;session&gt;/subagents/</c>, each folder in the ordinal order
/// of its names; a <c>memory</c> folder (in any case) is never walked, a session folder whose name is not made of letters,
/// digits, hyphens and underscores is left out, and a folder that is a link is reported and not followed. Each transcript
/// <c>agent-&lt;id&gt;.jsonl</c> written since the start of the window is a run: its description
/// <c>agent-&lt;id&gt;.meta.json</c> gives its agent type (without it, the run is left out and reported), and its transcript,
/// read line by line, gives its times, its project, its real model and its last tool. A run is finished when its
/// description says so, or when its last tool call is its hand back (<c>SubagentHandback</c>). The model is the real model
/// of the transcript, or the model asked when the transcript gives none. The lines that cannot be read are reported once
/// per file, with their number. A run whose two files have the same size and last write as at the previous reading is taken
/// from the memory of this instance; a reading that failed is not kept, so that it is tried again. Nothing is ever written.
/// One instance is not meant to be used by two threads at the same time.</para>
/// </remarks>
internal sealed class SubagentRunReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of the transcripts, in the folder of Claude Code
    /// </summary>
    private const string ProjectsFolderName = "projects";

    /// <summary>
    /// Name of the folder of the sub-agent runs, in the folder of a session
    /// </summary>
    private const string SubagentsFolderName = "subagents";

    /// <summary>
    /// Name of the folders of notes, never walked
    /// </summary>
    private const string MemoryFolderName = "memory";

    /// <summary>
    /// Start of the name of a transcript of a run
    /// </summary>
    private const string RunPrefix = "agent-";

    /// <summary>
    /// End of the name of a transcript of a run
    /// </summary>
    private const string TranscriptExtension = ".jsonl";

    /// <summary>
    /// End of the name of the description of a run
    /// </summary>
    private const string MetaExtension = ".meta.json";

    /// <summary>
    /// Name of the tool that ends a run
    /// </summary>
    private const string HandBackToolName = "SubagentHandback";

    #endregion

    #region Variables

    /// <summary>
    /// Reader of the files of Claude Code
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

    /// <summary>
    /// Runs already read, by full path of their transcript
    /// </summary>
    private readonly Dictionary<string, CachedRun> _cache = new Dictionary<string, CachedRun>(StringComparer.Ordinal);

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
    public SubagentRunReader(ClaudeFileReader fileReader)
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
    /// Method used to know whether a name is made of letters, digits, hyphens and underscores only
    /// </summary>
    /// <param name="name">[Cannot be null] Name to check</param>
    /// <returns>
    /// True  ==> The name is not empty and holds letters, digits, hyphens and underscores only<br/>
    /// False ==> In any other case
    /// </returns>
    private static bool IsPlainName(string name)
    {
        // Function's return
        return name.Length > 0 && name.All(character => char.IsAsciiLetterOrDigit(character) || character == '-' || character == '_');
    }

    /// <summary>
    /// Method used to list the plain subfolders of a folder, reporting those that are links
    /// </summary>
    /// <param name="folder">[Cannot be null] Full path of the folder</param>
    /// <param name="problems">[Cannot be null] Problems, where a folder that cannot be listed or a link is added</param>
    /// <returns>The full paths of the subfolders that are not links, in the ordinal order of their names</returns>
    private static List<string> ListPlainFolders(string folder, List<ReadProblem> problems)
    {
        // Keeping the folders, and reporting the links
        List<string> folders = new List<string>();
        foreach (FileSystemInfo entry in FolderListing.ListEntries(folder, problems))
        {
            if (entry is not DirectoryInfo)
                continue;

            string path = Path.Join(folder, entry.Name);
            if (FolderListing.IsLink(entry.Attributes))
                problems.Add(new ReadProblem(path, "The folder is a junction, a symbolic link or another reparse point: it is not followed"));
            else
                folders.Add(path);
        }

        // Function's return
        return folders;
    }

    /// <summary>
    /// Method used to read a run from its files, without the cache
    /// </summary>
    /// <param name="runId">[Cannot be null] Identifier of the run</param>
    /// <param name="sessionId">[Cannot be null] Identifier of the main session</param>
    /// <param name="transcriptPath">[Cannot be null] Full path of the transcript</param>
    /// <param name="metaPath">[Can be null] Full path of the description; null when it does not exist</param>
    /// <param name="problems">[Cannot be null] Problems met while the run is read</param>
    /// <param name="isFinal">False when a reading failed, so that the result must not be kept</param>
    /// <returns>[Can be null] The run; null when it is left out</returns>
    private AgentRun? ReadRun(string runId, string sessionId, string transcriptPath, string? metaPath, List<ReadProblem> problems, out bool isFinal)
    {
        // Leaving out a run without description, which cannot be placed at a desk
        isFinal = true;
        if (metaPath is null)
        {
            problems.Add(new ReadProblem(transcriptPath, "The run has no description (agent-<id>.meta.json), so its agent type is unknown: it is left out"));

            // Function's return
            return null;
        }

        // Reading the description
        FileReadResult metaResult = _fileReader.ReadText(metaPath);
        if (!metaResult.IsSuccess)
        {
            problems.Add(new ReadProblem(metaPath, $"The description cannot be read ({metaResult.Failure}): {metaResult.Detail}"));
            isFinal = false;

            // Function's return
            return null;
        }
        SubagentMeta? meta = SubagentMetaParser.Parse(metaPath, metaResult.Content!, problems);
        if (meta is null)
        {
            // Function's return
            return null;
        }

        // Summarizing the transcript, line by line
        TranscriptSummary summary = new TranscriptSummary();
        LineReadResult linesResult = _fileReader.ReadLines(transcriptPath, summary.Add);
        if (!linesResult.IsSuccess)
        {
            problems.Add(new ReadProblem(transcriptPath, $"The transcript cannot be read ({linesResult.Failure}): {linesResult.Detail}"));
            isFinal = false;

            // Function's return
            return null;
        }
        if (summary.UnreadableLineCount > 0 || linesResult.TooLongLineCount > 0)
        {
            problems.Add(new ReadProblem(transcriptPath, string.Create(CultureInfo.InvariantCulture,
                $"{summary.UnreadableLineCount + linesResult.TooLongLineCount} lines of the transcript cannot be read ({linesResult.TooLongLineCount} longer than the limit): they are left out")));
        }

        // Leaving out a run whose transcript gives no time or no project
        if (summary.FirstRecordAtUtc is null || summary.LastRecordAtUtc is null)
        {
            problems.Add(new ReadProblem(transcriptPath, "The transcript has no record with a time: the run is left out"));

            // Function's return
            return null;
        }
        if (summary.WorkingFolder is null)
        {
            problems.Add(new ReadProblem(transcriptPath, "The transcript gives no working folder, so the project is unknown: the run is left out"));

            // Function's return
            return null;
        }

        // Function's return
        return new AgentRun(runId, meta.AgentType, sessionId, new Project(summary.WorkingFolder), summary.FirstRecordAtUtc.Value, summary.LastRecordAtUtc.Value,
            meta.IsFinished || summary.LastToolName == HandBackToolName, summary.Model ?? meta.Model);
    }

    /// <summary>
    /// Method used to read the runs of the subagents folder of a session
    /// </summary>
    /// <param name="sessionFolder">[Cannot be null] Full path of the folder of the session</param>
    /// <param name="sinceUtc">Start of the window: older transcripts are not read</param>
    /// <param name="runs">[Cannot be null] Runs read, where the new ones are added</param>
    /// <param name="problems">[Cannot be null] Problems, where every run left out is added</param>
    /// <param name="seen">[Cannot be null] Full paths of the transcripts seen, so that the others are removed from the cache</param>
    private void ReadSession(string sessionFolder, DateTimeOffset sinceUtc, List<AgentRun> runs, List<ReadProblem> problems, HashSet<string> seen)
    {
        // Refusing a subagents folder that is a link
        string subagentsFolder = Path.Join(sessionFolder, SubagentsFolderName);
        if (!Directory.Exists(subagentsFolder))
            return;
        if (FolderListing.IsLinkedFolder(subagentsFolder))
        {
            problems.Add(new ReadProblem(subagentsFolder, "The folder is a junction, a symbolic link or another reparse point: it is not followed"));
            return;
        }

        // Knowing the files of the folder by their names, to find the description of each run
        List<FileSystemInfo> entries = FolderListing.ListEntries(subagentsFolder, problems);
        Dictionary<string, FileSystemInfo> files = entries.Where(entry => entry is FileInfo).ToDictionary(entry => entry.Name, StringComparer.Ordinal);
        string sessionId = Path.GetFileName(sessionFolder);
        foreach (FileSystemInfo entry in entries)
        {
            // Keeping the transcripts of runs written since the start of the window
            if (entry is not FileInfo transcript || !transcript.Name.StartsWith(RunPrefix, StringComparison.Ordinal)
                || !transcript.Name.EndsWith(TranscriptExtension, StringComparison.Ordinal) || transcript.LastWriteTimeUtc < sinceUtc.UtcDateTime)
            {
                continue;
            }

            // Refusing an identifier of run that is not made of letters and digits
            string transcriptPath = Path.Join(subagentsFolder, transcript.Name);
            string runId = transcript.Name.Substring(RunPrefix.Length, transcript.Name.Length - RunPrefix.Length - TranscriptExtension.Length);
            if (runId.Length == 0 || !runId.All(char.IsAsciiLetterOrDigit))
            {
                problems.Add(new ReadProblem(transcriptPath, "The identifier of the run in the name of the file is not made of letters and digits: the run is left out"));
                continue;
            }

            // Taking the run from the cache when its files have not changed, or reading it
            seen.Add(transcriptPath);
            FileInfo? meta = files.TryGetValue(RunPrefix + runId + MetaExtension, out FileSystemInfo? metaEntry) ? metaEntry as FileInfo : null;
            long metaLength = meta?.Length ?? -1;
            DateTime metaWriteUtc = meta?.LastWriteTimeUtc ?? DateTime.MinValue;
            if (!_cache.TryGetValue(transcriptPath, out CachedRun? cached) || !cached.IsSameAs(transcript.Length, transcript.LastWriteTimeUtc, metaLength, metaWriteUtc))
            {
                List<ReadProblem> runProblems = new List<ReadProblem>();
                AgentRun? run = ReadRun(runId, sessionId, transcriptPath, meta is null ? null : Path.Join(subagentsFolder, meta.Name), runProblems, out bool isFinal);
                cached = new CachedRun(transcript.Length, transcript.LastWriteTimeUtc, metaLength, metaWriteUtc, run, runProblems);
                if (isFinal)
                    _cache[transcriptPath] = cached;
                else
                    _cache.Remove(transcriptPath);
            }

            // Adding the run and its problems
            if (cached.Run is not null)
                runs.Add(cached.Run);
            problems.AddRange(cached.Problems);
        }
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the runs whose transcript was written since the start of a window
    /// </summary>
    /// <param name="sinceUtc">Start of the window: a run whose transcript was last written before it is not read</param>
    /// <returns>The runs read, and the problems met</returns>
    public SubagentRuns Read(DateTimeOffset sinceUtc)
    {
        // Refusing a projects folder that is a link
        List<AgentRun> runs = new List<AgentRun>();
        List<ReadProblem> problems = new List<ReadProblem>();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        string projectsFolder = Path.Join(_fileReader.Folder.RootPath, ProjectsFolderName);
        if (FolderListing.IsLinkedFolder(projectsFolder))
        {
            problems.Add(new ReadProblem(projectsFolder, "The projects folder is a junction, a symbolic link or another reparse point: it is not followed, so no run is read"));
        }
        else
        {
            // Walking the projects and their sessions, never a memory folder
            foreach (string projectFolder in ListPlainFolders(projectsFolder, problems))
            {
                if (string.Equals(Path.GetFileName(projectFolder), MemoryFolderName, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (string sessionFolder in ListPlainFolders(projectFolder, problems))
                {
                    string name = Path.GetFileName(sessionFolder);
                    if (!string.Equals(name, MemoryFolderName, StringComparison.OrdinalIgnoreCase) && IsPlainName(name))
                        ReadSession(sessionFolder, sinceUtc, runs, problems, seen);
                }
            }
        }

        // Forgetting the runs whose transcript was not seen this time
        foreach (string path in _cache.Keys.Where(path => !seen.Contains(path)).ToList())
            _cache.Remove(path);

        // Function's return
        return new SubagentRuns(runs, problems);
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
