/*
 *	@file	src/OpenSpaceIA.Core/States/TimeRange.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A range of time used inside the state rules
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
/// A range of time, from a start to an end, used inside the state rules
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the rules cut the periods to a window and merge them; a range keeps only the times, without the
/// project and the kind that no longer matter at that step.</para>
/// <para><b>Behavior:</b> internal to the core and immutable. It is built only by the rules, from values already checked,
/// so it does not check them again.</para>
/// </remarks>
internal readonly record struct TimeRange
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
    /// Start of the range, in UTC
    /// </summary>
    public DateTimeOffset StartUtc { get; }

    /// <summary>
    /// End of the range, in UTC
    /// </summary>
    public DateTimeOffset EndUtc { get; }

    /// <summary>
    /// Length of the range
    /// </summary>
    public TimeSpan Duration => EndUtc - StartUtc;

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="startUtc">Start of the range, in UTC</param>
    /// <param name="endUtc">End of the range, in UTC, not earlier than the start</param>
    public TimeRange(DateTimeOffset startUtc, DateTimeOffset endUtc)
    {
        // Setting the values
        StartUtc = startUtc;
        EndUtc = endUtc;
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
