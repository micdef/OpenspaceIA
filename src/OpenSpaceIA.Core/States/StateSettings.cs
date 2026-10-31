/*
 *	@file	src/OpenSpaceIA.Core/States/StateSettings.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Settings of the rules "overloaded" and "under-used"
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.States;

/// <summary>
/// Settings of the rules "overloaded" and "under-used"
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> every threshold is a default that the user can change (doc/analyse/state-definitions.md, § 7); the
/// rules receive the settings as an input, until the desktop application reads them from its configuration.</para>
/// <para><b>Behavior:</b> immutable; a value that would make a rule meaningless (an empty window, a share outside 0 to 1) is
/// refused when the settings are built.</para>
/// </remarks>
public sealed record StateSettings
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

    /// <summary>
    /// The default settings: a window of 2 hours, 2 projects and a break of 10 minutes for "overloaded"; a window of 7 days
    /// and 95 % of idle time during the active time of the chief for "under-used" (calibrated on a real week, 2026-10-31)
    /// </summary>
    public static StateSettings Default { get; } = new StateSettings(TimeSpan.FromHours(2), 2, TimeSpan.FromMinutes(10), TimeSpan.FromDays(7), 0.95);

    /// <summary>
    /// Window looked at by "overloaded", ending now
    /// </summary>
    public TimeSpan OverloadedWindow { get; }

    /// <summary>
    /// Minimum number of different projects worked on in the window to be overloaded
    /// </summary>
    public int OverloadedMinimumProjects { get; }

    /// <summary>
    /// Break that ends "never stops": the longest break must be shorter to be overloaded
    /// </summary>
    public TimeSpan OverloadedMaximumBreak { get; }

    /// <summary>
    /// Window looked at by "under-used", ending now
    /// </summary>
    public TimeSpan UnderUsedWindow { get; }

    /// <summary>
    /// Minimum share of idle time during the active time of the chief in the window to be under-used, from 0 to 1 (0.95 is
    /// 95 %)
    /// </summary>
    public double UnderUsedIdleShare { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="overloadedWindow">Window looked at by "overloaded"; greater than zero</param>
    /// <param name="overloadedMinimumProjects">Minimum number of different projects to be overloaded; at least 1</param>
    /// <param name="overloadedMaximumBreak">Break that ends "never stops"; greater than zero</param>
    /// <param name="underUsedWindow">Window looked at by "under-used"; greater than zero</param>
    /// <param name="underUsedIdleShare">Minimum share of idle time during the active time of the chief to be under-used; from 0
    /// to 1</param>
    /// <exception cref="ArgumentOutOfRangeException">If a window or the break is not greater than zero, if the minimum number of
    /// projects is less than 1, or if the share is not a number from 0 to 1</exception>
    public StateSettings(TimeSpan overloadedWindow, int overloadedMinimumProjects, TimeSpan overloadedMaximumBreak, TimeSpan underUsedWindow,
        double underUsedIdleShare)
    {
        // Checks
        if (overloadedWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(overloadedWindow), overloadedWindow, $"The value of the parameter: {nameof(overloadedWindow)} must be greater than zero");
        if (overloadedMinimumProjects < 1)
            throw new ArgumentOutOfRangeException(nameof(overloadedMinimumProjects), overloadedMinimumProjects, $"The value of the parameter: {nameof(overloadedMinimumProjects)} must be at least 1");
        if (overloadedMaximumBreak <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(overloadedMaximumBreak), overloadedMaximumBreak, $"The value of the parameter: {nameof(overloadedMaximumBreak)} must be greater than zero");
        if (underUsedWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(underUsedWindow), underUsedWindow, $"The value of the parameter: {nameof(underUsedWindow)} must be greater than zero");
        if (double.IsNaN(underUsedIdleShare) || underUsedIdleShare < 0 || underUsedIdleShare > 1)
            throw new ArgumentOutOfRangeException(nameof(underUsedIdleShare), underUsedIdleShare, $"The value of the parameter: {nameof(underUsedIdleShare)} must be a number from 0 to 1");

        // Setting the values
        OverloadedWindow = overloadedWindow;
        OverloadedMinimumProjects = overloadedMinimumProjects;
        OverloadedMaximumBreak = overloadedMaximumBreak;
        UnderUsedWindow = underUsedWindow;
        UnderUsedIdleShare = underUsedIdleShare;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    // #N/A

    #endregion

    #region Publics

    // #N/A

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
