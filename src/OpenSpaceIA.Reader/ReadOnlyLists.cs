/*
 *	@file	src/OpenSpaceIA.Reader/ReadOnlyLists.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Copy of a list into a read-only list, for the results of the reader
 */

// Using
using System.Collections.ObjectModel;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to copy a list into a read-only list, for the results of the reader
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a result of the reader cannot be changed after it is built, even by the code that gave its
/// lists.</para>
/// <para><b>Behavior:</b> the copy is a new list that cannot be changed, even through a cast; a null list or a null
/// element is refused.</para>
/// </remarks>
internal static class ReadOnlyLists
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
    /// Method used to copy a list into a read-only list, refusing a null element
    /// </summary>
    /// <typeparam name="T">Type of the elements</typeparam>
    /// <param name="items">[Can be null] List to copy</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter, for the exceptions</param>
    /// <returns>A read-only copy of the list</returns>
    /// <exception cref="ArgumentNullException">If the list is null</exception>
    /// <exception cref="ArgumentException">If the list holds a null element</exception>
    public static ReadOnlyCollection<T> CopyWithoutNull<T>(IReadOnlyList<T>? items, string paramName)
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
