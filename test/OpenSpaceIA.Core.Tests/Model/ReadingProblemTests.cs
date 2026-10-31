/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/ReadingProblemTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of a problem met while the activity was read
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
/// Tests of a problem met while the activity was read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a problem always says where and why.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class ReadingProblemTests
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
    /// A problem keeps its source and its reason; two problems with the same values are equal
    /// </summary>
    [Fact]
    public void ReadingProblemKeepsItsValues()
    {
        // Building the problem
        ReadingProblem problem = new ReadingProblem("/invented/agents/a.md", "invented reason");

        // Checking the values
        problem.Source.ShouldBe("/invented/agents/a.md");
        problem.Reason.ShouldBe("invented reason");
        problem.ShouldBe(new ReadingProblem("/invented/agents/a.md", "invented reason"));
    }

    /// <summary>
    /// The source of a problem cannot be null, empty or white spaces
    /// </summary>
    /// <param name="source">[Can be null] Blank source that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadingProblemSourceCannotBeBlank(string? source)
    {
        // Building the problem with a blank source
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ReadingProblem(source!, "invented reason"));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("source");
    }

    /// <summary>
    /// The reason of a problem cannot be null, empty or white spaces
    /// </summary>
    /// <param name="reason">[Can be null] Blank reason that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadingProblemReasonCannotBeBlank(string? reason)
    {
        // Building the problem with a blank reason
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ReadingProblem("/invented/agents/a.md", reason!));

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
