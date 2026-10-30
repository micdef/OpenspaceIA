/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/ProjectTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the Project model
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Tests.Model;

/// <summary>
/// Tests of the Project model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class ProjectTests
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

    // #N/A

    #endregion

    #region Publics

    /// <summary>
    /// A project path loses its trailing separators, so that one folder is always one project
    /// </summary>
    /// <param name="path">Path given to the project</param>
    /// <param name="expectedPath">Path that the project must keep</param>
    [Theory]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha", "C:\\Users\\dev\\Projects\\Alpha")]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha\\", "C:\\Users\\dev\\Projects\\Alpha")]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha\\/\\", "C:\\Users\\dev\\Projects\\Alpha")]
    [InlineData("/home/dev/projects/beta/", "/home/dev/projects/beta")]
    [InlineData("\\\\server\\share\\", "\\\\server\\share")]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha ", "C:\\Users\\dev\\Projects\\Alpha ")]
    public void ProjectPathLosesItsTrailingSeparators(string path, string expectedPath)
    {
        // Building the project
        Project project = new Project(path);

        // Checking the path
        project.Path.ShouldBe(expectedPath);
    }

    /// <summary>
    /// The root of a drive or of the file system keeps its separator: without it, it would be another path
    /// </summary>
    /// <param name="path">Path of a root</param>
    /// <param name="expectedPath">Path that the project must keep</param>
    [Theory]
    [InlineData("C:\\", "C:\\")]
    [InlineData("C:\\\\", "C:\\")]
    [InlineData("C:/", "C:/")]
    [InlineData("/", "/")]
    [InlineData("//", "//")]
    public void ProjectRootPathKeepsItsSeparator(string path, string expectedPath)
    {
        // Building the project
        Project project = new Project(path);

        // Checking the path
        project.Path.ShouldBe(expectedPath);
    }

    /// <summary>
    /// The same folder given with or without a trailing separator is the same project
    /// </summary>
    [Fact]
    public void ProjectsOfTheSameFolderWithAndWithoutTrailingSeparatorAreEqual()
    {
        // Building the same folder twice
        Project first = new Project("C:\\Users\\dev\\Projects\\Alpha");
        Project second = new Project("C:\\Users\\dev\\Projects\\Alpha\\");

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// The project name is the last segment of the path, whatever the separator, and trailing separators are ignored
    /// </summary>
    /// <param name="path">Path of the project</param>
    /// <param name="expectedName">Name that must be deduced from the path</param>
    [Theory]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha", "Alpha")]
    [InlineData("/home/dev/projects/beta/", "beta")]
    [InlineData("/home/dev/projects/beta", "beta")]
    [InlineData("C:\\Users\\dev\\Projects\\Alpha\\\\", "Alpha")]
    [InlineData("/home/dev\\mixed/Gamma", "Gamma")]
    [InlineData("C:\\", "C:")]
    [InlineData("C:\\My Projects\\Delta Space", "Delta Space")]
    [InlineData("/home/dev/projets/été-日本", "été-日本")]
    [InlineData("relative", "relative")]
    public void ProjectNameIsTheLastSegmentOfThePath(string path, string expectedName)
    {
        // Building the project
        Project project = new Project(path);

        // Checking the deduced name
        project.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// A path made only of separators gives a name equal to the path
    /// </summary>
    /// <param name="path">Path made only of separators</param>
    [Theory]
    [InlineData("/")]
    [InlineData("//")]
    [InlineData("\\")]
    [InlineData("\\/\\")]
    public void ProjectNameOfASeparatorOnlyPathIsThePath(string path)
    {
        // Building the project
        Project project = new Project(path);

        // Checking the deduced name
        project.Name.ShouldBe(path);
    }

    /// <summary>
    /// A project path cannot be null, empty or white spaces
    /// </summary>
    /// <param name="path">[Can be null] Blank path that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ProjectPathCannotBeBlank(string? path)
    {
        // Building the project with a blank path
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new Project(path!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("path");
    }

    /// <summary>
    /// Two projects built with the same path are equal
    /// </summary>
    [Fact]
    public void ProjectsWithTheSamePathAreEqual()
    {
        // Building two projects with the same path
        Project first = new Project("/home/dev/projects/beta");
        Project second = new Project("/home/dev/projects/beta");

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two projects with different paths are not equal
    /// </summary>
    [Fact]
    public void ProjectsWithDifferentPathsAreNotEqual()
    {
        // Building two projects with different paths
        Project first = new Project("/home/dev/projects/beta");
        Project second = new Project("/home/dev/projects/gamma");

        // Checking the value equality
        first.ShouldNotBe(second);
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
