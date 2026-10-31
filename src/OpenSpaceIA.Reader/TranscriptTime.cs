/*
 *	@file	src/OpenSpaceIA.Reader/TranscriptTime.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the time of a record of a transcript
 */

// Using
using System.Globalization;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the time of a record of a transcript
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> every period of the office comes from these times; a time that depends on the machine, or that is
/// in the future, would give a wrong period without anyone seeing it.</para>
/// <para><b>Behavior:</b> a time must be a full ISO 8601 date and time, with or without fractions of a second, followed by
/// <c>Z</c> or an offset; it is turned into UTC. A time later than the latest time accepted is not true, and is refused.</para>
/// </remarks>
internal static class TranscriptTime
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Formats of a time accepted: a full date and time, with fractions of a second or not, then <c>Z</c> or an offset
    /// </summary>
    private static readonly string[] _formats = new string[]
    {
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz",
    };

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
    /// Method used to read the time of a record
    /// </summary>
    /// <param name="text">[Can be null] Text of the time</param>
    /// <param name="latestAcceptedUtc">Latest time accepted</param>
    /// <param name="time">The time, in UTC; the default value when it cannot be read</param>
    /// <returns>
    /// True  ==> The text is a full ISO time with its offset, not later than the latest time accepted<br/>
    /// False ==> The text is null, of another form, or later than the latest time accepted
    /// </returns>
    public static bool TryRead(string? text, DateTimeOffset latestAcceptedUtc, out DateTimeOffset time)
    {
        // Reading a full ISO time with its offset, not later than the latest time accepted
        if (text is null
            || !DateTimeOffset.TryParseExact(text, _formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out time)
            || time > latestAcceptedUtc)
        {
            time = default;

            // Function's return
            return false;
        }

        // Turning the time into UTC
        time = time.ToUniversalTime();

        // Function's return
        return true;
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
