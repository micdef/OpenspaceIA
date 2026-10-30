/*
 *	@file	test/OpenSpaceIA.Architecture.Tests/CoreDependencyTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Architecture rules of the Core: it depends on nothing
 */

// Using
using System.Text.RegularExpressions;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Architecture.Tests;

/// <summary>
/// Tests used to check that the Core depends on nothing but the .NET base library
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the Core knows neither the user interface nor the format of the files (src/README.md); a dependency
/// added by mistake breaks the CI before it can be merged.</para>
/// <para><b>Behavior:</b> two levels, read by <see cref="DependencyReader"/>. The project file of the Core is read, so that
/// a reference is refused even before it is used; the namespace of every type used by the assembly of the Core is checked,
/// the global namespace included, and the rule names the type at fault. A control proves that the types used really include
/// external ones.</para>
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
    /// Types used by the assembly of the Core, read once for every rule
    /// </summary>
    private static readonly List<(string TypeName, string NamespaceName)> _typesUsed = DependencyReader.GetTypesUsed(typeof(Project).Assembly);

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

    // #N/A

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
        List<string> namespaces = _typesUsed.Select(used => used.NamespaceName).Distinct().ToList();

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
        List<string> forbidden = _typesUsed
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
        List<string> references = DependencyReader.GetProjectReferences("OpenSpaceIA.Core");

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
