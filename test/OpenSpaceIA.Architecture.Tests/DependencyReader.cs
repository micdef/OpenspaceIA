/*
 *	@file	test/OpenSpaceIA.Architecture.Tests/DependencyReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reads what a project of the solution depends on: its project file and the types its code uses
 */

// Using
using System.Reflection;
using System.Xml.Linq;
using ArchUnitNET.Loader;

// Static using
// #N/A

// Named using
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

// Namespace
namespace OpenSpaceIA.Architecture.Tests;

/// <summary>
/// Service used to read what a project of the solution depends on
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> every rule of dependency reads the same two things, the project file and the types used by the
/// compiled assembly; they are read in one place, so that every rule reads them the same way.</para>
/// <para><b>Behavior:</b> the types used are read by ArchUnitNET (method bodies, generic arguments and attributes included),
/// directly from the dependencies of each type rather than through its fluent syntax: that syntax only matches the types it
/// has loaded, and the types of System are forwarded to an assembly the loader does not read, so a fluent control could not
/// prove that they are seen.</para>
/// </remarks>
internal static class DependencyReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the solution file, which marks the root of the repository
    /// </summary>
    private const string SolutionFileName = "OpenSpaceIA.slnx";

    #endregion

    #region Variables

    /// <summary>
    /// Elements of a project file that make it depend on something
    /// </summary>
    private static readonly string[] _referenceElements = new string[] { "ProjectReference", "PackageReference", "Reference", "FrameworkReference", "COMReference" };

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
    /// Method used to find the root of the repository from the folder of the tests
    /// </summary>
    /// <returns>The full path of the folder that holds the solution file</returns>
    /// <exception cref="FileNotFoundException">If no folder above the tests holds the solution file</exception>
    private static string FindRepositoryRoot()
    {
        // Going up from the folder of the tests to the folder that holds the solution file
        DirectoryInfo? folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, SolutionFileName)))
            folder = folder.Parent;

        if (folder is null)
            throw new FileNotFoundException($"The solution file {SolutionFileName} was not found above {AppContext.BaseDirectory}");

        // Function's return
        return folder.FullName;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to list the types used by the code of an assembly
    /// </summary>
    /// <param name="assembly">[Cannot be null] Compiled assembly of a project of the solution</param>
    /// <returns>The full names of the types used by any type of the assembly, each with the namespace it lives in</returns>
    public static List<(string TypeName, string NamespaceName)> GetTypesUsed(Assembly assembly)
    {
        // Loading the assembly with its dependencies
        ArchitectureModel architecture = new ArchLoader().LoadAssembliesIncludingDependencies(assembly).Build();
        string assemblyName = assembly.FullName!;

        // Function's return: every type used by any type of the assembly, whatever its namespace
        return architecture.Types
            .Where(type => type.Assembly.FullName == assemblyName)
            .SelectMany(type => type.Dependencies)
            .Select(dependency => (TypeName: dependency.Target.FullName, NamespaceName: dependency.Target.Namespace.FullName))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Method used to list the references of the project file of a source project
    /// </summary>
    /// <param name="projectName">[Cannot be null] Name of the project, which is also the name of its folder under src/</param>
    /// <returns>Each reference as its element name and its target, for example "ProjectReference ..\OpenSpaceIA.Core\OpenSpaceIA.Core.csproj"</returns>
    /// <exception cref="FileNotFoundException">If the solution file or the project file is not found</exception>
    public static List<string> GetProjectReferences(string projectName)
    {
        // Reading the project file
        XDocument project = XDocument.Load(Path.Combine(FindRepositoryRoot(), "src", projectName, projectName + ".csproj"));

        // Function's return
        return project.Descendants()
            .Where(element => _referenceElements.Contains(element.Name.LocalName))
            .Select(element => $"{element.Name.LocalName} {(string?)element.Attribute("Include")}")
            .ToList();
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
