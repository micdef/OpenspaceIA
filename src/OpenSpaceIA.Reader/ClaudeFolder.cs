/*
 *	@file	src/OpenSpaceIA.Reader/ClaudeFolder.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	The folder of Claude Code, and the only files the reader may open in it
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
/// The folder of Claude Code (<c>~/.claude</c> by default), and the only files the reader may open in it
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the folder holds secrets, account data and private notes (doc/analyse/spike-claude-files.md, § 1).
/// The reader opens only what the screens need, chosen by an allow-list: any file that is not in it is refused, so that a
/// new file of Claude Code is never read by mistake.</para>
/// <para><b>Behavior:</b> immutable; it never touches the disk. The allow-list holds four kinds of files, compared with
/// their case: <c>sessions/&lt;name&gt;.json</c>, the <c>.md</c> agent definitions under <c>agents/</c> (at the top or in a
/// subfolder, since Claude Code reads both), the descriptions of the sub-agent runs
/// <c>projects/&lt;folder&gt;/&lt;session&gt;/subagents/agent-&lt;id&gt;.meta.json</c>, and the <c>.jsonl</c> transcripts under
/// <c>projects/&lt;folder&gt;/</c>, outside any <c>memory</c> folder (that name is compared without its case, since it is the
/// same folder on Windows). A relative path, a path outside the folder, a path that climbs out of it with two dots, a path
/// the system cannot resolve, and a segment holding a colon (an NTFS stream such as <c>memory::$INDEX_ALLOCATION</c> names
/// the memory folder itself) or a character forbidden in a file name are refused. This check reads names only: a junction
/// or a symbolic link inside the folder is refused by <see cref="ClaudeFileReader"/>, which looks at the disk.</para>
/// </remarks>
internal sealed class ClaudeFolder
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of Claude Code in the profile of the user
    /// </summary>
    private const string FolderName = ".claude";

    /// <summary>
    /// Name of the folder of the notes that Claude Code keeps per project, never read
    /// </summary>
    private const string MemoryFolderName = "memory";

    #endregion

    #region Variables

    /// <summary>
    /// Separators of the segments of a path: both are accepted on Windows, and the slash elsewhere
    /// </summary>
    private static readonly char[] _separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

    /// <summary>
    /// Characters refused in a segment: those forbidden in a file name by the system, and the colon on every system, since
    /// it names an NTFS stream on Windows
    /// </summary>
    private static readonly char[] _forbiddenNameCharacters = Path.GetInvalidFileNameChars().Append(':').Distinct().ToArray();

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

    /// <summary>
    /// Full path of the folder, without trailing separator
    /// </summary>
    public string RootPath { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rootPath">[Cannot be null] Full path of the folder of Claude Code</param>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces</exception>
    /// <exception cref="ArgumentException">If the path is not fully qualified (it would depend on the current folder), if it is
    /// a device path (two backslashes, then a question mark or a dot: it would bypass the next check), or if it is the root of a disk (the whole
    /// disk would be open)</exception>
    public ClaudeFolder(string rootPath)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentNullException(nameof(rootPath), $"The value of the parameter: {nameof(rootPath)} cannot be null or empty");
        if (!Path.IsPathFullyQualified(rootPath))
            throw new ArgumentException($"The value of the parameter: {nameof(rootPath)} must be a full path", nameof(rootPath));
        if (rootPath.StartsWith(@"\\?\", StringComparison.Ordinal) || rootPath.StartsWith(@"\\.\", StringComparison.Ordinal))
            throw new ArgumentException($"The value of the parameter: {nameof(rootPath)} cannot be a device path", nameof(rootPath));

        // Normalizing the path, then refusing the root of a disk
        string normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        if (string.Equals(Path.GetPathRoot(normalized), normalized, StringComparison.Ordinal))
            throw new ArgumentException($"The value of the parameter: {nameof(rootPath)} cannot be the root of a disk", nameof(rootPath));

        // Setting the normalized path
        RootPath = normalized;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to know whether a file name has a given extension and a name before it
    /// </summary>
    /// <param name="fileName">[Cannot be null] Name of the file</param>
    /// <param name="extension">[Cannot be null] Extension, with its dot, compared with its case</param>
    /// <returns>
    /// True  ==> The name ends with the extension and has at least one character before it<br/>
    /// False ==> In any other case
    /// </returns>
    private static bool HasExtension(string fileName, string extension)
    {
        // Function's return
        return fileName.Length > extension.Length && fileName.EndsWith(extension, StringComparison.Ordinal);
    }

    /// <summary>
    /// Method used to know whether segments of a path, relative to the folder, name a file of the allow-list
    /// </summary>
    /// <param name="segments">[Cannot be null] Segments of the relative path, without empty ones</param>
    /// <returns>
    /// True  ==> The segments name a session file, an agent definition, a description of a sub-agent run or a transcript<br/>
    /// False ==> In any other case
    /// </returns>
    private static bool IsInAllowList(string[] segments)
    {
        // Refusing a segment that names an NTFS stream, or holds a character forbidden in a file name
        if (segments.Any(segment => segment.IndexOfAny(_forbiddenNameCharacters) >= 0))
        {
            // Function's return
            return false;
        }

        // Live sessions: sessions/<name>.json
        if (segments.Length == 2 && segments[0] == "sessions")
        {
            // Function's return
            return HasExtension(segments[1], ".json");
        }

        // Definitions of the agents: agents/.../<name>.md, at the top or in subfolders, which Claude Code reads too
        if (segments.Length >= 2 && segments[0] == "agents")
        {
            // Function's return
            return HasExtension(segments[^1], ".md");
        }

        // Transcripts: projects/<folder>/.../<name>.jsonl, and descriptions of runs: projects/<folder>/<session>/subagents/agent-<id>.meta.json,
        // outside any memory folder
        if (segments.Length >= 3 && segments[0] == "projects")
        {
            bool isInMemory = segments.Skip(1).Any(segment => string.Equals(segment, MemoryFolderName, StringComparison.OrdinalIgnoreCase));
            bool isRunDescription = segments.Length == 5 && segments[3] == "subagents" && segments[4].StartsWith("agent-", StringComparison.Ordinal)
                && HasExtension(segments[4], ".meta.json") && segments[4].Length > "agent-.meta.json".Length;

            // Function's return
            return !isInMemory && (HasExtension(segments[^1], ".jsonl") || isRunDescription);
        }

        // Function's return
        return false;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to get the folder of Claude Code in the profile of the current user
    /// </summary>
    /// <returns>The folder <c>.claude</c> of the profile of the user</returns>
    public static ClaudeFolder FromUserProfile()
    {
        // Function's return
        return new ClaudeFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), FolderName));
    }

    /// <summary>
    /// Method used to know whether the reader may open a file
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the single gate of the reader: no file is opened without passing it.</para>
    /// <para><b>Behavior:</b> the path is made absolute and simplified (two dots are resolved) without touching the disk,
    /// then its part inside the folder is compared with the allow-list. A path the system cannot resolve gives false, never
    /// an exception.</para>
    /// </remarks>
    /// <param name="fullPath">[Cannot be null] Full path of the file</param>
    /// <returns>
    /// True  ==> The file is in the allow-list<br/>
    /// False ==> The file must not be opened
    /// </returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces</exception>
    public bool IsReadable(string fullPath)
    {
        // Check
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentNullException(nameof(fullPath), $"The value of the parameter: {nameof(fullPath)} cannot be null or empty");

        // Refusing a relative path, which would depend on the current folder
        if (!Path.IsPathFullyQualified(fullPath))
        {
            // Function's return
            return false;
        }

        // Getting the part of the path inside the folder; a path the system cannot resolve (a null character, a length no
        // system accepts) is refused instead of failing
        string relativePath;
        try
        {
            relativePath = Path.GetRelativePath(RootPath, Path.GetFullPath(fullPath));
        }
        catch (ArgumentException)
        {
            // Function's return: the path holds a character that no path may hold
            return false;
        }
        catch (PathTooLongException)
        {
            // Function's return: the path is longer than the system accepts
            return false;
        }

        // Refusing a path outside the folder, which starts with two dots or stays absolute. The allow-list below would
        // refuse such a path too: this check is a second barrier, kept in case the list grows
        string[] segments = relativePath.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        if (Path.IsPathRooted(relativePath) || segments.Length == 0 || segments[0] == "." || segments[0] == "..")
        {
            // Function's return
            return false;
        }

        // Function's return
        return IsInAllowList(segments);
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
