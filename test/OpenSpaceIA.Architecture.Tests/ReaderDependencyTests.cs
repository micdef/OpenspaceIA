/*
 *	@file	test/OpenSpaceIA.Architecture.Tests/ReaderDependencyTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Architecture rules of the Reader: it depends on the Core only
 */

// Using
using System.Text.RegularExpressions;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Architecture.Tests;

/// <summary>
/// Tests used to check that the Reader depends on the Core and on the .NET base library only
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the Reader is the adapter of the files of Claude Code; it must never reach the user interface nor a
/// writer, and every package it would use must be a decision, not an accident.</para>
/// <para><b>Behavior:</b> the same two levels as for the Core, read by <see cref="DependencyReader"/>: the project file
/// references the Core only, and every type used by the assembly of the Reader lives in System, in the Core or in the Reader.
/// A control proves that the external types are seen.</para>
/// </remarks>
public sealed class ReaderDependencyTests
{
    #region Members

    #region Constants

    /// <summary>
    /// Name of the project of the Reader, which is also the name of its assembly and of its folder
    /// </summary>
    private const string ReaderProjectName = "OpenSpaceIA.Reader";

    #endregion

    #region Variables

    /// <summary>
    /// Namespaces the Reader may depend on: its own, the Core, and System (the .NET base library). A package whose namespace
    /// also starts with System would pass this rule: the rule on the project file refuses it
    /// </summary>
    private static readonly Regex _allowedNamespaces = new Regex(@"^(OpenSpaceIA\.Reader|OpenSpaceIA\.Core|System)(\..*)?$", RegexOptions.CultureInvariant);

    /// <summary>
    /// Types used by the assembly of the Reader, read once for every rule
    /// </summary>
    private static readonly List<(string TypeName, string NamespaceName)> _typesUsed = DependencyReader.GetTypesUsed(System.Reflection.Assembly.Load(ReaderProjectName));

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
    /// The types used by the Reader include external types (control of the type rule)
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the type rule below only works if the types outside the Reader are seen; this control fails if
    /// they ever stop being seen.</para>
    /// <para><b>Behavior:</b> the Reader opens files, so System.IO must be among the namespaces used.</para>
    /// </remarks>
    [Fact]
    public void TypesUsedByTheReaderIncludeExternalTypes()
    {
        // Listing the namespaces used
        List<string> namespaces = _typesUsed.Select(used => used.NamespaceName).Distinct().ToList();

        // Checking that a namespace that is used for sure is seen
        namespaces.ShouldContain("System.IO");
    }

    /// <summary>
    /// The types of the Reader use only the Reader itself, the Core and the .NET base library
    /// </summary>
    [Fact]
    public void ReaderTypesDependOnTheCoreOnly()
    {
        // Listing the types used that live outside the allowed namespaces
        List<string> forbidden = _typesUsed
            .Where(used => !_allowedNamespaces.IsMatch(used.NamespaceName))
            .Select(used => used.TypeName)
            .ToList();

        // Checking that there is none
        forbidden.ShouldBeEmpty("the Reader depends on the Core only");
    }

    /// <summary>
    /// The project file of the Reader references the Core and nothing else
    /// </summary>
    [Fact]
    public void ReaderProjectFileReferencesTheCoreOnly()
    {
        // Reading the references of the project file, with one kind of separator whatever was written
        List<string> references = DependencyReader.GetProjectReferences(ReaderProjectName)
            .Select(reference => reference.Replace('/', '\\'))
            .ToList();

        // Checking that the Core is the only one
        references.ShouldBe(new List<string> { @"ProjectReference ..\OpenSpaceIA.Core\OpenSpaceIA.Core.csproj" });
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
