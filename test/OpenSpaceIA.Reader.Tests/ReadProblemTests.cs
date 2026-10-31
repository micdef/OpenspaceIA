/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ReadProblemTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of a problem met while the files of Claude Code are read
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of a problem met while the files of Claude Code are read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a problem always says which file and why.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class ReadProblemTests
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
    /// A problem keeps its file and its reason
    /// </summary>
    [Fact]
    public void ProblemKeepsItsFileAndItsReason()
    {
        // Building the problem
        ReadProblem problem = new ReadProblem("/invented/agents/a.md", "invented reason");

        // Checking the values
        problem.FilePath.ShouldBe("/invented/agents/a.md");
        problem.Reason.ShouldBe("invented reason");
        problem.ShouldBe(new ReadProblem("/invented/agents/a.md", "invented reason"));
    }

    /// <summary>
    /// The file of a problem cannot be null, empty or white spaces
    /// </summary>
    /// <param name="filePath">[Can be null] Blank file that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FilePathCannotBeBlank(string? filePath)
    {
        // Building the problem with a blank file
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ReadProblem(filePath!, "invented reason"));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("filePath");
    }

    /// <summary>
    /// The reason of a problem cannot be null, empty or white spaces
    /// </summary>
    /// <param name="reason">[Can be null] Blank reason that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReasonCannotBeBlank(string? reason)
    {
        // Building the problem with a blank reason
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ReadProblem("/invented/agents/a.md", reason!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("reason");
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
