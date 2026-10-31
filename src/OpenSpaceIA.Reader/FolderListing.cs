/*
 *	@file	src/OpenSpaceIA.Reader/FolderListing.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Listing of a folder of Claude Code, where every failure is a problem
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
/// Service used to list a folder of Claude Code (the agents, the sessions), where every failure is a problem
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> each reading of a folder must tell a folder that does not exist (nothing to read, which is not a
/// problem) from a folder that cannot be listed (a problem that must be seen), the same way on Windows and on Linux.</para>
/// <para><b>Behavior:</b> reads names and attributes only, never the content of a file. When nothing exists at the place
/// of the folder, the list is empty without a problem; anything else that cannot be listed (a file, a link to a missing
/// target, a refused access, a folder removed meanwhile) gives an empty list and a problem. The entries are sorted in the
/// ordinal order of their names, so that the result does not depend on the system.</para>
/// </remarks>
internal static class FolderListing
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

    #endregion

    #region Publics

    /// <summary>
    /// Method used to describe an exception of the system for a problem
    /// </summary>
    /// <param name="exception">[Cannot be null] Exception raised by the system</param>
    /// <returns>The type of the exception, its code and its message</returns>
    public static string Describe(Exception exception)
    {
        // Function's return
        return $"{exception.GetType().Name} (0x{exception.HResult:X8}): {exception.Message}";
    }

    /// <summary>
    /// Method used to know whether attributes are those of a link
    /// </summary>
    /// <param name="attributes">Attributes of an entry</param>
    /// <returns>
    /// True  ==> The entry is a junction, a symbolic link or another reparse point<br/>
    /// False ==> The entry is a plain file or folder
    /// </returns>
    public static bool IsLink(FileAttributes attributes)
    {
        // Function's return
        return (attributes & FileAttributes.ReparsePoint) != 0;
    }

    /// <summary>
    /// Method used to list the entries of a folder, in the ordinal order of their names
    /// </summary>
    /// <param name="folder">[Cannot be null] Full path of the folder</param>
    /// <param name="problems">[Cannot be null] Problems, where a folder that cannot be listed is added</param>
    /// <returns>The files and the folders; empty when the folder is missing or cannot be listed</returns>
    public static List<FileSystemInfo> ListEntries(string folder, ICollection<ReadProblem> problems)
    {
        // Reporting an entry that exists but is not a folder (a file; on Linux, a link to a missing target)
        if (!Directory.Exists(folder))
        {
            if (Exists(folder))
                problems.Add(new ReadProblem(folder, "The entry cannot be listed: it is not a folder (a file, or a link to a missing target), so nothing is read in it"));

            // Function's return
            return new List<FileSystemInfo>();
        }

        try
        {
            // Listing the entries, and sorting them so that the result does not depend on the system
            List<FileSystemInfo> entries = new DirectoryInfo(folder).EnumerateFileSystemInfos().ToList();
            entries.Sort((first, second) => string.CompareOrdinal(first.Name, second.Name));

            // Function's return
            return entries;
        }
        catch (DirectoryNotFoundException exception)
        {
            problems.Add(new ReadProblem(folder, $"The folder cannot be listed (a link to a missing target, or a folder removed meanwhile): {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
        }
        catch (UnauthorizedAccessException exception)
        {
            problems.Add(new ReadProblem(folder, $"The folder cannot be listed: {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
        }
        catch (IOException exception)
        {
            problems.Add(new ReadProblem(folder, $"The folder cannot be listed: {Describe(exception)}"));

            // Function's return
            return new List<FileSystemInfo>();
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
