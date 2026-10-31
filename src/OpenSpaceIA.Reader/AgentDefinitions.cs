/*
 *	@file	src/OpenSpaceIA.Reader/AgentDefinitions.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Result of the reading of the agent definitions: the agent types, and the problems met
 */

// Using
using System.Collections.ObjectModel;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Result of the reading of the agent definitions: the agent types read, and the problems met
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> gives the agent types to the core, and keeps every definition left out, so that the screens can say
/// that something is missing instead of showing an incomplete office as if it were complete.</para>
/// <para><b>Behavior:</b> immutable: the lists are copied when the result is built. Empty lists are valid (no agent of the
/// user, no problem). Value equality compares the lists by reference, as for the snapshot of the core: two results built
/// separately are never equal.</para>
/// </remarks>
internal sealed record AgentDefinitions
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
    /// Agent types read, in the order given by the reader
    /// </summary>
    public IReadOnlyList<AgentType> AgentTypes { get; }

    /// <summary>
    /// Problems met: definitions left out, and values that could not be read
    /// </summary>
    public IReadOnlyList<ReadProblem> Problems { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="agentTypes">[Cannot be null] Agent types read; no null element</param>
    /// <param name="problems">[Cannot be null] Problems met; no null element</param>
    /// <exception cref="ArgumentNullException">If a list is null</exception>
    /// <exception cref="ArgumentException">If a list holds a null element</exception>
    public AgentDefinitions(IReadOnlyList<AgentType> agentTypes, IReadOnlyList<ReadProblem> problems)
    {
        // Copying the lists
        AgentTypes = CopyWithoutNull(agentTypes, nameof(agentTypes));
        Problems = CopyWithoutNull(problems, nameof(problems));
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to copy a list into a read-only list, refusing a null element
    /// </summary>
    /// <typeparam name="T">Type of the elements</typeparam>
    /// <param name="items">[Can be null] List to copy</param>
    /// <param name="paramName">[Cannot be null] Name of the parameter, for the exceptions</param>
    /// <returns>A read-only copy of the list</returns>
    /// <exception cref="ArgumentNullException">If the list is null</exception>
    /// <exception cref="ArgumentException">If the list holds a null element</exception>
    private static ReadOnlyCollection<T> CopyWithoutNull<T>(IReadOnlyList<T>? items, string paramName)
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
