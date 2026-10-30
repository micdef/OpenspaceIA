/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/PlanTaskTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the PlanTask model
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
/// Tests of the PlanTask model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class PlanTaskTests
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
    /// A task keeps the content and the status it was given
    /// </summary>
    /// <param name="status">Status given to the task</param>
    [Theory]
    [InlineData(PlanTaskStatus.Pending)]
    [InlineData(PlanTaskStatus.InProgress)]
    [InlineData(PlanTaskStatus.Completed)]
    public void PlanTaskKeepsItsContentAndStatus(PlanTaskStatus status)
    {
        // Building the task
        PlanTask task = new PlanTask("Write the unit tests", status);

        // Checking the properties
        task.Content.ShouldBe("Write the unit tests");
        task.Status.ShouldBe(status);
    }

    /// <summary>
    /// A task content cannot be null, empty or white spaces
    /// </summary>
    /// <param name="content">[Can be null] Blank content that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PlanTaskContentCannotBeBlank(string? content)
    {
        // Building the task with a blank content
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new PlanTask(content!, PlanTaskStatus.Pending));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("content");
    }

    /// <summary>
    /// A task status must be a value defined in the enumeration
    /// </summary>
    [Fact]
    public void PlanTaskStatusMustBeDefined()
    {
        // Building the task with an undefined status
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new PlanTask("Write the unit tests", (PlanTaskStatus)42));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("status");
    }

    /// <summary>
    /// Two tasks built with the same arguments are equal
    /// </summary>
    [Fact]
    public void PlanTasksWithTheSameArgumentsAreEqual()
    {
        // Building two tasks with the same arguments
        PlanTask first = new PlanTask("Write the unit tests", PlanTaskStatus.InProgress);
        PlanTask second = new PlanTask("Write the unit tests", PlanTaskStatus.InProgress);

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two tasks with different statuses are not equal
    /// </summary>
    [Fact]
    public void PlanTasksWithDifferentStatusesAreNotEqual()
    {
        // Building two tasks with different statuses
        PlanTask first = new PlanTask("Write the unit tests", PlanTaskStatus.Pending);
        PlanTask second = new PlanTask("Write the unit tests", PlanTaskStatus.Completed);

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
