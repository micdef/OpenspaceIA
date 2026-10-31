/*
 *	@file	src/OpenSpaceIA.Reader/AgentDefinitionReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the agent definitions of Claude Code
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the agent definitions of the user, in the folder <c>agents</c> of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> gives the core one agent type per definition of the user (doc/analyse/roadmap.md, 0.4.0). The
/// built-in agents (Explore, Plan…) have no file: they are known from their runs, not from here.</para>
/// <para><b>Behavior:</b> lists the top of the agents folder, in the ordinal order of the names (the listing reads names
/// only), and reads each <c>.md</c> file with <see cref="ClaudeFileReader"/>, so that the allow-list, the links and the size
/// limit apply to every file read; a file with another extension (a backup <c>.md.bak-…</c>, a note) is not a definition and
/// is left out without a problem, except an extension <c>.md</c> written in another case, which is reported. Claude Code
/// also reads the subfolders of this folder: they are not read yet, and each one is reported. A file that cannot be read or
/// parsed is reported and does not stop the others. Two definitions with the same name: the first one in the order of the
/// names is kept and the other is reported (Claude Code keeps one of them in an order it does not document). When nothing
/// exists at the place of the agents folder, there is no agent and no problem, since a user may have no agent of their own
/// (a missing folder of Claude Code is the concern of the reading of the activity); anything else that cannot be listed (a
/// file, a link to a missing target, a refused access) is reported. Known limit: a named pipe called <c>x.md</c> would block
/// the reading (creating one needs the right to write in the folder). Nothing is ever written.</para>
/// </remarks>
internal sealed class AgentDefinitionReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the folder of the definitions, in the folder of Claude Code
    /// </summary>
    private const string AgentsFolderName = "agents";

    /// <summary>
    /// Extension of a definition
    /// </summary>
    private const string DefinitionExtension = ".md";

    #endregion

    #region Variables

    /// <summary>
    /// Reader of the files of Claude Code
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
    /// Constructor
    /// </summary>
    /// <param name="fileReader">[Cannot be null] Reader of the files of Claude Code</param>
    /// <exception cref="ArgumentNullException">If the reader of the files is null</exception>
    public AgentDefinitionReader(ClaudeFileReader fileReader)
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
    /// Method used to list the entries at the top of the agents folder, in the ordinal order of their names
    /// </summary>
    /// <param name="agentsFolder">[Cannot be null] Full path of the agents folder</param>
    /// <param name="problems">[Cannot be null] Problems, where a folder that cannot be listed is added</param>
    /// <returns>The files and the folders; empty when the folder is missing or cannot be listed</returns>
    private static List<FileSystemInfo> ListEntries(string agentsFolder, List<ReadProblem> problems)
    {
        // Reporting an agents entry that exists but is not a folder (a file; on Linux, a link to a missing target)
        if (!Directory.Exists(agentsFolder))
        {
            if (Exists(agentsFolder))
                problems.Add(new ReadProblem(agentsFolder, "The agents entry cannot be listed: it is not a folder (a file, or a link to a missing target), so no definition is read"));

            // Function's return
            return new List<FileSystemInfo>();
        }

        try
        {
            // Listing the entries, and sorting them so that the result does not depend on the system
            List<FileSystemInfo> entries = new DirectoryInfo(agentsFolder).EnumerateFileSystemInfos().ToList();
            entries.Sort((first, second) => string.CompareOrdinal(first.Name, second.Name));

            // Function's return
            return entries;
        }
        catch (DirectoryNotFoundException exception)
        {
            problems.Add(new ReadProblem(agentsFolder, $"The agents folder cannot be listed (a link to a missing target, or a folder removed meanwhile): {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
        }
        catch (UnauthorizedAccessException exception)
        {
            problems.Add(new ReadProblem(agentsFolder, $"The agents folder cannot be listed: {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
        }
        catch (IOException exception)
        {
            problems.Add(new ReadProblem(agentsFolder, $"The agents folder cannot be listed: {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
        }
    }

    /// <summary>
    /// Method used to know whether something exists at a path, whatever it is, without following a link
    /// </summary>
    /// <param name="path">[Cannot be null] Full path</param>
    /// <returns>
    /// True  ==> A file, a folder or a link (even to a missing target) exists at the path, or the system refuses to say<br/>
    /// False ==> Nothing exists at the path
    /// </returns>
    private static bool Exists(string path)
    {
        try
        {
            // Reading the attributes of the entry itself
            File.GetAttributes(path);

            // Function's return
            return true;
        }
        catch (FileNotFoundException)
        {
            // Function's return
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            // Function's return
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // Function's return: the system refuses to say, so the entry is reported rather than taken as missing
            return true;
        }
        catch (IOException)
        {
            // Function's return: the system cannot say, so the entry is reported rather than taken as missing
            return true;
        }
    }

    /// <summary>
    /// Method used to describe an exception of the system for a problem
    /// </summary>
    /// <param name="exception">[Cannot be null] Exception raised while the folder was listed</param>
    /// <returns>The type of the exception, its code and its message</returns>
    private static string Describe(Exception exception)
    {
        // Function's return
        return $"{exception.GetType().Name} (0x{exception.HResult:X8}): {exception.Message}";
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read every agent definition of the user
    /// </summary>
    /// <returns>The agent types read, and the problems met</returns>
    public AgentDefinitions Read()
    {
        // Listing the agents folder
        List<AgentType> agentTypes = new List<AgentType>();
        List<ReadProblem> problems = new List<ReadProblem>();
        Dictionary<string, string> filesByName = new Dictionary<string, string>(StringComparer.Ordinal);
        string agentsFolder = Path.Combine(_fileReader.Folder.RootPath, AgentsFolderName);
        foreach (FileSystemInfo entry in ListEntries(agentsFolder, problems))
        {
            // Reporting a subfolder, which is not read
            if (entry is DirectoryInfo)
            {
                problems.Add(new ReadProblem(entry.FullName, "A subfolder of the agents folder is not read: the definitions it holds are not shown"));
                continue;
            }

            // Leaving out a file that is not a definition, and reporting an extension written in another case
            if (!string.Equals(entry.Extension, DefinitionExtension, StringComparison.Ordinal))
            {
                if (string.Equals(entry.Extension, DefinitionExtension, StringComparison.OrdinalIgnoreCase))
                    problems.Add(new ReadProblem(entry.FullName, $"The extension is not written {DefinitionExtension} in lowercase: the file is not read"));
                continue;
            }

            // Reading the file, and reporting a failure
            FileReadResult result = _fileReader.ReadText(entry.FullName);
            if (!result.IsSuccess)
            {
                problems.Add(new ReadProblem(entry.FullName, $"The definition cannot be read ({result.Failure}): {result.Detail}"));
                continue;
            }

            // Parsing the definition, whose problems are added by the parser
            AgentType? agentType = AgentDefinitionParser.Parse(entry.FullName, result.Content!, problems);
            if (agentType is null)
                continue;

            // Keeping the first definition of a name, and reporting the others
            if (filesByName.TryGetValue(agentType.Name, out string? firstFile))
            {
                problems.Add(new ReadProblem(entry.FullName, $"The name: {agentType.Name} is already defined by {firstFile}: this definition is left out"));
                continue;
            }

            filesByName.Add(agentType.Name, entry.FullName);
            agentTypes.Add(agentType);
        }

        // Function's return
        return new AgentDefinitions(agentTypes, problems);
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
