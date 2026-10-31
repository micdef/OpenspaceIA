/*
 *	@file	test/OpenSpaceIA.Reader.Tests/FixedClock.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Clock that always gives the same time, for the tests
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
/// Clock that always gives the same time, for the tests
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> now is a parameter of every rule (doc/analyse/state-definitions.md, § 5): the tests fix it.</para>
/// <para><b>Behavior:</b> gives the time given to its constructor, in UTC.</para>
/// </remarks>
internal sealed class FixedClock : TimeProvider
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Time given by the clock
    /// </summary>
    private readonly DateTimeOffset _nowUtc;

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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="nowUtc">Time given by the clock, in UTC</param>
    public FixedClock(DateTimeOffset nowUtc)
    {
        // Setting the value
        _nowUtc = nowUtc;
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

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow()
    {
        // Function's return
        return _nowUtc;
    }

    #endregion

    #region Operators

    // #N/A

    #endregion

    #endregion
}
