/*
 *	@file	src/OpenSpaceIA.Core/Model/ModelGuard.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Checks shared by the constructors of the model
 */

// Using
using System.Collections.ObjectModel;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Model;

/// <summary>
/// Service used to check the values given to the constructors of the model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> every type of the model refuses an invalid value when it is built, with the same exceptions and the
/// same messages, so that an invalid object can never exist.</para>
/// <para><b>Behavior:</b> each method checks one value and throws, or returns the value to store. It has no state.</para>
/// </remarks>
internal static class ModelGuard
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
    /// Method used to check an object that is required
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> a session or a run without project, or a snapshot without machine, cannot be placed in the office.</para>
    /// <para><b>Behavior:</b> the object is returned as given.</para>
    /// </remarks>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <param name="value">[Can be null] Object to check</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter of the caller, given in the exception</param>
    /// <returns>The object, unchanged</returns>
    /// <exception cref="ArgumentNullException">If the object is null</exception>
    public static T RequireNotNull<T>(T? value, string paramName)
        where T : class
    {
        // Check
        if (value is null)
            throw new ArgumentNullException(paramName, $"The value of the parameter: {paramName} cannot be null");

        // Function's return
        return value;
    }

    /// <summary>
    /// Method used to check a text that is required
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> an identifier or a name without text would show an empty card on the screens.</para>
    /// <para><b>Behavior:</b> the text is returned as given, without trimming.</para>
    /// </remarks>
    /// <param name="value">[Can be null] Text to check</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter of the caller, given in the exception</param>
    /// <returns>The text, unchanged</returns>
    /// <exception cref="ArgumentNullException">If the text is null, empty or white spaces</exception>
    public static string RequireText(string? value, string paramName)
    {
        // Check
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(paramName, $"The value of the parameter: {paramName} cannot be null or empty");

        // Function's return
        return value;
    }

    /// <summary>
    /// Method used to normalize a text that is optional
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> a missing text has only one form, null, so that the screens test one case only.</para>
    /// <para><b>Behavior:</b> a text that is null, empty or white spaces gives null; any other text is returned as given,
    /// without trimming.</para>
    /// </remarks>
    /// <param name="value">[Can be null] Text to normalize</param>
    /// <returns>
    /// Null       ==> The text is null, empty or white spaces<br/>
    /// The text   ==> In any other case
    /// </returns>
    public static string? OptionalText(string? value)
    {
        // Function's return
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Method used to check that a date is expressed in UTC
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> all times of the core are compared in UTC (doc/analyse/state-definitions.md, § 5); a local
    /// time would shift the periods of work by the offset.</para>
    /// <para><b>Behavior:</b> a date whose offset is not zero is refused; it is never converted.</para>
    /// </remarks>
    /// <param name="value">Date to check</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter of the caller, given in the exception</param>
    /// <returns>The date, unchanged</returns>
    /// <exception cref="ArgumentException">If the offset of the date is not zero</exception>
    public static DateTimeOffset RequireUtc(DateTimeOffset value, string paramName)
    {
        // Check
        if (value.Offset != TimeSpan.Zero)
            throw new ArgumentException($"The value of the parameter: {paramName} must be expressed in UTC (offset zero)", paramName);

        // Function's return
        return value;
    }

    /// <summary>
    /// Method used to check that a value is defined in its enumeration
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> a cast can give any number to an enumeration; such a value has no meaning in the model.</para>
    /// <para><b>Behavior:</b> the value is returned when it is one of the named values of the enumeration.</para>
    /// </remarks>
    /// <typeparam name="TEnum">Enumeration of the value</typeparam>
    /// <param name="value">Value to check</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter of the caller, given in the exception</param>
    /// <returns>The value, unchanged</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the value is not defined in the enumeration</exception>
    public static TEnum RequireDefined<TEnum>(TEnum value, string paramName)
        where TEnum : struct, Enum
    {
        // Check
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(paramName, value, $"The value of the parameter: {paramName} is not defined in {typeof(TEnum).Name}");

        // Function's return
        return value;
    }

    /// <summary>
    /// Method used to copy a list into a read-only list without null element
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the model is immutable: a list kept by the caller must not change an object after it is built.</para>
    /// <para><b>Behavior:</b> the elements are copied, in the same order, into a new read-only list; the caller cannot cast
    /// it back to a list it can change.</para>
    /// </remarks>
    /// <typeparam name="T">Type of the elements</typeparam>
    /// <param name="items">[Can be null] List to copy</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter of the caller, given in the exception</param>
    /// <returns>A read-only copy of the list</returns>
    /// <exception cref="ArgumentNullException">If the list is null</exception>
    /// <exception cref="ArgumentException">If the list contains a null element</exception>
    public static IReadOnlyList<T> CopyWithoutNull<T>(IReadOnlyList<T>? items, string paramName)
        where T : class
    {
        // Check
        if (items is null)
            throw new ArgumentNullException(paramName, $"The value of the parameter: {paramName} cannot be null");

        // Copying the elements, refusing a null one
        List<T> copy = new List<T>(items.Count);
        foreach (T? item in items)
        {
            if (item is null)
                throw new ArgumentException($"The list of the parameter: {paramName} cannot contain a null element", paramName);

            copy.Add(item);
        }

        // Function's return
        return new ReadOnlyCollection<T>(copy);
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
