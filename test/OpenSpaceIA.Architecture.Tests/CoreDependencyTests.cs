/*
 *	@file	test/OpenSpaceIA.Architecture.Tests/CoreDependencyTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Architecture rules of the Core: it depends on nothing
 */

// Using
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ArchUnitNET.Loader;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

// Namespace
namespace OpenSpaceIA.Architecture.Tests;

/// <summary>
/// Tests used to check that the Core depends on nothing but the .NET base library
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the Core knows neither the user interface nor the format of the files (src/README.md); a dependency
/// added by mistake breaks the CI before it can be merged.</para>
/// <para><b>Behavior:</b> two levels. The project file of the Core is read, so that a reference is refused even before it is
/// used. The compiled assembly is read by ArchUnitNET, which sees every type used by the code (method bodies, generic
/// arguments and attributes included); the rule checks the namespace of each type used by any type of the assembly, the
/// global namespace included. The rule reads the types used directly rather than through the fluent syntax of ArchUnitNET,
/// which only matches the types it has loaded: the types of System are forwarded to an assembly the loader does not read,
/// so a fluent control could not prove that they are seen. Reading them directly, a control can, and the rule names the
/// type at fault.</para>
/// </remarks>
public sealed class CoreDependencyTests
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Namespaces the Core may depend on: its own, and System (the .NET base library). A package whose namespace also starts
    /// with System would pass this rule: the rule on the project file refuses it
    /// </summary>
    private static readonly Regex _allowedNamespaces = new Regex(@"^(OpenSpaceIA\.Core|System)(\..*)?$", RegexOptions.CultureInvariant);

    /// <summary>
    /// Compiled assembly of the Core with its dependencies, loaded once for every rule
    /// </summary>
    private static readonly ArchitectureModel _coreArchitecture = LoadCoreArchitecture();

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
    /// Method used to load the compiled assembly of the Core with its dependencies
    /// </summary>
    /// <returns>The architecture model of the Core</returns>
    private static ArchitectureModel LoadCoreArchitecture()
    {
        // Getting the assembly from one of its types
        Assembly coreAssembly = typeof(Project).Assembly;

        // Function's return
        return new ArchLoader().LoadAssembliesIncludingDependencies(coreAssembly).Build();
    }

    /// <summary>
    /// Method used to list the types used by the types of the Core
    /// </summary>
    /// <returns>The full names of the types used, each with the namespace it lives in</returns>
    private static List<(string TypeName, string NamespaceName)> GetTypesUsedByTheCore()
    {
        // Function's return: every type used by any type of the assembly of the Core, whatever its namespace
        string coreAssemblyName = typeof(Project).Assembly.FullName!;
        return _coreArchitecture.Types
            .Where(type => type.Assembly.FullName == coreAssemblyName)
            .SelectMany(type => type.Dependencies)
            .Select(dependency => (TypeName: dependency.Target.FullName, NamespaceName: dependency.Target.Namespace.FullName))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Method used to find the project file of the Core from the folder of the tests
    /// </summary>
    /// <returns>The full path of OpenSpaceIA.Core.csproj</returns>
    /// <exception cref="FileNotFoundException">If no folder above the tests holds the solution file</exception>
    private static string FindCoreProjectFile()
    {
        // Going up from the folder of the tests to the root of the repository, which holds the solution file
        DirectoryInfo? folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "OpenSpaceIA.slnx")))
            folder = folder.Parent;

        if (folder is null)
            throw new FileNotFoundException($"The solution file OpenSpaceIA.slnx was not found above {AppContext.BaseDirectory}");

        // Function's return
        return Path.Combine(folder.FullName, "src", "OpenSpaceIA.Core", "OpenSpaceIA.Core.csproj");
    }

    #endregion

    #region Publics

    /// <summary>
    /// The types used by the Core include external types (control of the type rule)
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the type rule below only works if the types outside the Core are seen. This control fails if
    /// they ever stop being seen, instead of letting the type rule pass blindly.</para>
    /// <para><b>Behavior:</b> the activity snapshot holds read-only lists, so System.Collections.Generic must be among the
    /// namespaces used.</para>
    /// </remarks>
    [Fact]
    public void TypesUsedByTheCoreIncludeExternalTypes()
    {
        // Listing the namespaces used
        List<string> namespaces = GetTypesUsedByTheCore().Select(used => used.NamespaceName).Distinct().ToList();

        // Checking that a namespace that is used for sure is seen
        namespaces.ShouldContain("System.Collections.Generic");
    }

    /// <summary>
    /// The types of the Core use only the Core itself and the .NET base library
    /// </summary>
    [Fact]
    public void CoreTypesDependOnNothingElse()
    {
        // Listing the types used that live outside the allowed namespaces
        List<string> forbidden = GetTypesUsedByTheCore()
            .Where(used => !_allowedNamespaces.IsMatch(used.NamespaceName))
            .Select(used => used.TypeName)
            .ToList();

        // Checking that there is none
        forbidden.ShouldBeEmpty("the Core knows neither the user interface nor the format of the files");
    }

    /// <summary>
    /// The project file of the Core references no project, no package, no assembly and no framework
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> refuses a reference as soon as it is added, even before a type uses it.</para>
    /// <para><b>Behavior:</b> the analyzers given to every project by Directory.Packages.props are not in this file: they
    /// check the code at build time and are not dependencies of the Core.</para>
    /// </remarks>
    [Fact]
    public void CoreProjectFileHasNoReference()
    {
        // Reading the references of the project file
        XDocument project = XDocument.Load(FindCoreProjectFile());
        List<string> references = project.Descendants()
            .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference" or "Reference" or "FrameworkReference" or "COMReference")
            .Select(element => $"{element.Name.LocalName} {(string?)element.Attribute("Include")}")
            .ToList();

        // Checking that there is none
        references.ShouldBeEmpty("OpenSpaceIA.Core must depend on nothing");
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
