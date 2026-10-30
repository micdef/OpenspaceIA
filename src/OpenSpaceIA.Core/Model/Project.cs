/*
 *	@file	src/OpenSpaceIA.Core/Model/Project.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A project worked on by Claude Code, known by its folder
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Model;

/// <summary>
/// A project worked on by Claude Code, known by the path of its folder
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the sessions and the runs of the agents are tied to a project; "overloaded" counts the projects an
/// agent works on.</para>
/// <para><b>Behavior:</b> immutable. The path is the working folder of the session (<c>cwd</c>), without its trailing
/// separators, so that one folder is always one project; the name is its last segment. Two projects with the same path are
/// equal; the comparison is ordinal (case sensitive), whatever the operating system.</para>
/// </remarks>
public sealed record Project
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Separators of the segments of a path: both are always accepted, because a path of a Windows machine can be read on
    /// Linux, and the other way round
    /// </summary>
    private static readonly char[] _separators = new char[] { '/', '\\' };

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
    /// Path of the folder of the project, without its trailing separators (a root keeps its separator)
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Name of the project: the last segment of its path, without the trailing separators
    /// </summary>
    public string Name { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">[Cannot be null] Path of the folder of the project</param>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces</exception>
    public Project(string path)
    {
        // Setting the normalized path and the name deduced from it
        Path = NormalizePath(ModelGuard.RequireText(path, nameof(path)));
        Name = GetName(Path);
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to remove the trailing separators of a path
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> <c>C:\Alpha</c> and <c>C:\Alpha\</c> are the same folder; without this, "overloaded" would count
    /// it as two projects.</para>
    /// <para><b>Behavior:</b> every trailing separator is removed, except for a root, which keeps one: a drive keeps the first
    /// separator after its colon (<c>C:\\</c> gives <c>C:\</c>, because <c>C:</c> alone is another path on Windows), and a path
    /// made only of separators (<c>/</c>) is kept as given. Nothing else is changed: no case, no white space.</para>
    /// </remarks>
    /// <param name="path">[Cannot be null] Path of the folder, already checked</param>
    /// <returns>The path without its trailing separators</returns>
    private static string NormalizePath(string path)
    {
        // Removing the trailing separators
        string trimmed = path.TrimEnd(_separators);

        // Keeping a path made only of separators as given
        if (trimmed.Length == 0)
        {
            // Function's return
            return path;
        }

        // Keeping the separator of the root of a drive
        if (trimmed.EndsWith(':') && trimmed.Length < path.Length)
        {
            // Function's return
            return path.Substring(0, trimmed.Length + 1);
        }

        // Function's return
        return trimmed;
    }

    /// <summary>
    /// Method used to deduce the name of a project from its path
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the screens show a short name, not a full path.</para>
    /// <para><b>Behavior:</b> the trailing separators are ignored, then the text after the last separator is the name
    /// (<c>C:\Users\dev\Alpha\</c> gives <c>Alpha</c>, <c>C:\</c> gives <c>C:</c>). A path made only of separators has no
    /// segment: its name is the path itself.</para>
    /// </remarks>
    /// <param name="path">[Cannot be null] Path of the folder, already normalized</param>
    /// <returns>The name of the project</returns>
    private static string GetName(string path)
    {
        // Ignoring the trailing separators
        string trimmed = path.TrimEnd(_separators);

        // Keeping the path itself when it has no segment
        if (trimmed.Length == 0)
        {
            // Function's return
            return path;
        }

        // Function's return
        return trimmed.Substring(trimmed.LastIndexOfAny(_separators) + 1);
    }

    #endregion

    #region Publics

    // #N/A

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
