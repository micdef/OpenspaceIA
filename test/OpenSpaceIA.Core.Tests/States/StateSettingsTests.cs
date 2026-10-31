/*
 *	@file	test/OpenSpaceIA.Core.Tests/States/StateSettingsTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the StateSettings rules
 */

// Using
using OpenSpaceIA.Core.States;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Tests.States;

/// <summary>
/// Tests of the StateSettings rules
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the rules before their implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class StateSettingsTests
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
    /// Method used to build settings from the five values
    /// </summary>
    /// <param name="overloadedWindow">Window of "overloaded"</param>
    /// <param name="overloadedMinimumProjects">Minimum number of projects of "overloaded"</param>
    /// <param name="overloadedMaximumBreak">Break that ends "never stops"</param>
    /// <param name="underUsedWindow">Window of "under-used"</param>
    /// <param name="underUsedIdleShare">Idle share of "under-used"</param>
    /// <returns>The built settings</returns>
    private static StateSettings Create(TimeSpan overloadedWindow, int overloadedMinimumProjects, TimeSpan overloadedMaximumBreak,
        TimeSpan underUsedWindow, double underUsedIdleShare)
    {
        // Function's return
        return new StateSettings(overloadedWindow, overloadedMinimumProjects, overloadedMaximumBreak, underUsedWindow, underUsedIdleShare);
    }

    #endregion

    #region Publics

    /// <summary>
    /// The default settings are the ones of doc/analyse/state-definitions.md, § 7
    /// </summary>
    [Fact]
    public void DefaultSettingsAreTheOnesOfTheDefinitions()
    {
        // Reading the default settings
        StateSettings settings = StateSettings.Default;

        // Checking the values
        settings.OverloadedWindow.ShouldBe(TimeSpan.FromHours(2));
        settings.OverloadedMinimumProjects.ShouldBe(2);
        settings.OverloadedMaximumBreak.ShouldBe(TimeSpan.FromMinutes(10));
        settings.UnderUsedWindow.ShouldBe(TimeSpan.FromDays(7));
        settings.UnderUsedIdleShare.ShouldBe(0.95);
    }

    /// <summary>
    /// The default settings are the same settings every time they are read
    /// </summary>
    [Fact]
    public void DefaultSettingsAreStable()
    {
        // Building the default values by hand
        StateSettings expected = Create(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), 0.95);

        // Checking the value equality
        StateSettings.Default.ShouldBe(StateSettings.Default);
        StateSettings.Default.ShouldBe(expected);
    }

    /// <summary>
    /// Valid settings keep every value they were given
    /// </summary>
    [Fact]
    public void SettingsKeepTheirValues()
    {
        // Building the settings with values that are not the default ones
        StateSettings settings = Create(TimeSpan.FromHours(1), 3, TimeSpan.FromMinutes(15), TimeSpan.FromDays(1), 0.5);

        // Checking the properties
        settings.OverloadedWindow.ShouldBe(TimeSpan.FromHours(1));
        settings.OverloadedMinimumProjects.ShouldBe(3);
        settings.OverloadedMaximumBreak.ShouldBe(TimeSpan.FromMinutes(15));
        settings.UnderUsedWindow.ShouldBe(TimeSpan.FromDays(1));
        settings.UnderUsedIdleShare.ShouldBe(0.5);
    }

    /// <summary>
    /// The smallest values allowed by the rules are accepted: one tick, one project, a share of 0 and a share of 1
    /// </summary>
    /// <param name="idleShare">Share on the edge of the allowed range</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    public void SettingsAcceptTheLimitsOfTheAllowedRanges(double idleShare)
    {
        // Building the settings on the limits
        StateSettings settings = Create(TimeSpan.FromTicks(1), 1, TimeSpan.FromTicks(1), TimeSpan.FromTicks(1), idleShare);

        // Checking the properties
        settings.OverloadedWindow.ShouldBe(TimeSpan.FromTicks(1));
        settings.OverloadedMinimumProjects.ShouldBe(1);
        settings.OverloadedMaximumBreak.ShouldBe(TimeSpan.FromTicks(1));
        settings.UnderUsedWindow.ShouldBe(TimeSpan.FromTicks(1));
        settings.UnderUsedIdleShare.ShouldBe(idleShare);
    }

    /// <summary>
    /// The window of "overloaded" must be longer than zero
    /// </summary>
    /// <param name="ticks">Zero or negative duration that must be refused</param>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-72000000000L)]
    public void OverloadedWindowMustBePositive(long ticks)
    {
        // Building the settings with a window that is not positive
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Create(TimeSpan.FromTicks(ticks), 2, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), 0.9));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("overloadedWindow");
    }

    /// <summary>
    /// The minimum number of projects of "overloaded" must be at least one
    /// </summary>
    /// <param name="minimumProjects">Number that must be refused</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void OverloadedMinimumProjectsMustBeAtLeastOne(int minimumProjects)
    {
        // Building the settings with a minimum that is lower than one
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Create(TimeSpan.FromHours(2), minimumProjects, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), 0.9));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("overloadedMinimumProjects");
    }

    /// <summary>
    /// The break that ends "never stops" must be longer than zero
    /// </summary>
    /// <param name="ticks">Zero or negative duration that must be refused</param>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-6000000000L)]
    public void OverloadedMaximumBreakMustBePositive(long ticks)
    {
        // Building the settings with a break that is not positive
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Create(TimeSpan.FromHours(2), 2, TimeSpan.FromTicks(ticks), TimeSpan.FromDays(7), 0.9));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("overloadedMaximumBreak");
    }

    /// <summary>
    /// The window of "under-used" must be longer than zero
    /// </summary>
    /// <param name="ticks">Zero or negative duration that must be refused</param>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-6048000000000L)]
    public void UnderUsedWindowMustBePositive(long ticks)
    {
        // Building the settings with a window that is not positive
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Create(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), TimeSpan.FromTicks(ticks), 0.9));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("underUsedWindow");
    }

    /// <summary>
    /// The idle share of "under-used" must be a number between 0 and 1
    /// </summary>
    /// <param name="idleShare">Value outside the range, or not a number, that must be refused</param>
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(90.0)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NaN)]
    public void UnderUsedIdleShareMustBeBetweenZeroAndOne(double idleShare)
    {
        // Building the settings with a share that is not allowed
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => Create(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), idleShare));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("underUsedIdleShare");
    }

    /// <summary>
    /// Two settings built with the same values are equal
    /// </summary>
    [Fact]
    public void SettingsWithTheSameValuesAreEqual()
    {
        // Building two settings with the same values
        StateSettings first = Create(TimeSpan.FromHours(1), 3, TimeSpan.FromMinutes(15), TimeSpan.FromDays(1), 0.5);
        StateSettings second = Create(TimeSpan.FromHours(1), 3, TimeSpan.FromMinutes(15), TimeSpan.FromDays(1), 0.5);

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two settings that differ by one value are not equal
    /// </summary>
    [Fact]
    public void SettingsWithOneDifferentValueAreNotEqual()
    {
        // Building settings that differ from the default ones by the idle share only
        StateSettings other = Create(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), 0.8);

        // Checking the value equality
        StateSettings.Default.ShouldNotBe(other);
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
