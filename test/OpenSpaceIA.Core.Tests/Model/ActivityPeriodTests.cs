/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/ActivityPeriodTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the ActivityPeriod model
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
/// Tests of the ActivityPeriod model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class ActivityPeriodTests
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
    /// Method used to get the fixed UTC date shared by the tests
    /// </summary>
    /// <returns>A date whose offset is zero</returns>
    private static DateTimeOffset UtcDate()
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);
    }

    /// <summary>
    /// Method used to get a fixed date that is not expressed in UTC (it is later than the shared UTC date)
    /// </summary>
    /// <returns>A date whose offset is two hours</returns>
    private static DateTimeOffset NonUtcDate()
    {
        // Function's return
        return new DateTimeOffset(2026, 10, 30, 14, 0, 0, TimeSpan.FromHours(2));
    }

    /// <summary>
    /// Method used to get the invented project shared by the tests
    /// </summary>
    /// <returns>A valid project</returns>
    private static Project CreateProject()
    {
        // Function's return
        return new Project("/home/dev/projects/alpha");
    }

    #endregion

    #region Publics

    /// <summary>
    /// A valid period keeps every value it was given
    /// </summary>
    /// <param name="kind">Kind given to the period</param>
    [Theory]
    [InlineData(PeriodKind.Working)]
    [InlineData(PeriodKind.Waiting)]
    public void ActivityPeriodKeepsItsValues(PeriodKind kind)
    {
        // Building the period
        Project project = CreateProject();
        ActivityPeriod period = new ActivityPeriod(project, kind, UtcDate(), UtcDate().AddMinutes(30));

        // Checking the properties
        period.Project.ShouldBe(project);
        period.Kind.ShouldBe(kind);
        period.StartUtc.ShouldBe(UtcDate());
        period.EndUtc.ShouldBe(UtcDate().AddMinutes(30));
    }

    /// <summary>
    /// A period must have a project
    /// </summary>
    [Fact]
    public void ActivityPeriodProjectCannotBeNull()
    {
        // Building the period without project
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => new ActivityPeriod(null!, PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30)));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("project");
    }

    /// <summary>
    /// A period kind must be a value defined in the enumeration
    /// </summary>
    [Fact]
    public void ActivityPeriodKindMustBeDefined()
    {
        // Building the period with an undefined kind
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new ActivityPeriod(CreateProject(), (PeriodKind)42, UtcDate(), UtcDate().AddMinutes(30)));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("kind");
    }

    /// <summary>
    /// A period start must be expressed in UTC
    /// </summary>
    [Fact]
    public void ActivityPeriodStartMustBeUtc()
    {
        // Building the period with a start that is not in UTC
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivityPeriod(CreateProject(), PeriodKind.Working, NonUtcDate().AddHours(-6), UtcDate().AddHours(1)));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("startUtc");
    }

    /// <summary>
    /// A period end must be expressed in UTC
    /// </summary>
    [Fact]
    public void ActivityPeriodEndMustBeUtc()
    {
        // Building the period with an end that is not in UTC (it is later than the start)
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), NonUtcDate()));

        // Checking the exact type (Should.Throw also accepts a derived type) and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("endUtc");
    }

    /// <summary>
    /// A period cannot end before it starts
    /// </summary>
    [Fact]
    public void ActivityPeriodCannotEndBeforeItsStart()
    {
        // Building the period with an end one second before the start
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), UtcDate().AddSeconds(-1)));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("endUtc");
    }

    /// <summary>
    /// A period whose end is equal to its start is allowed
    /// </summary>
    [Fact]
    public void ActivityPeriodWithAnEndEqualToItsStartIsAllowed()
    {
        // Building the period with an end equal to the start
        ActivityPeriod period = new ActivityPeriod(CreateProject(), PeriodKind.Waiting, UtcDate(), UtcDate());

        // Checking the dates
        period.EndUtc.ShouldBe(period.StartUtc);
    }

    /// <summary>
    /// Two periods built with the same arguments are equal
    /// </summary>
    [Fact]
    public void ActivityPeriodsWithTheSameArgumentsAreEqual()
    {
        // Building two periods with the same arguments (the projects are two objects with the same path)
        ActivityPeriod first = new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30));
        ActivityPeriod second = new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30));

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two periods that differ by their kind are not equal
    /// </summary>
    [Fact]
    public void ActivityPeriodsWithDifferentKindsAreNotEqual()
    {
        // Building two periods that differ by the kind
        ActivityPeriod first = new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30));
        ActivityPeriod second = new ActivityPeriod(CreateProject(), PeriodKind.Waiting, UtcDate(), UtcDate().AddMinutes(30));

        // Checking the value equality
        first.ShouldNotBe(second);
    }

    /// <summary>
    /// Two periods that differ by their project are not equal
    /// </summary>
    [Fact]
    public void ActivityPeriodsWithDifferentProjectsAreNotEqual()
    {
        // Building two periods that differ by the project
        ActivityPeriod first = new ActivityPeriod(CreateProject(), PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30));
        ActivityPeriod second = new ActivityPeriod(new Project("/home/dev/projects/beta"), PeriodKind.Working, UtcDate(), UtcDate().AddMinutes(30));

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
