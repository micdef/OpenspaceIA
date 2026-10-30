/*
 *	@file	src/OpenSpaceIA.Core/Model/ActivityPeriod.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	A period of work or of waiting of an agent, on a project
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Model;

/// <summary>
/// A period of work or of waiting of an agent, on a project
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> "overloaded" and "under-used" are computed from the periods of an agent over a window of time
/// (doc/analyse/state-definitions.md, § 3 to § 6), the chief and the other agents alike.</para>
/// <para><b>Behavior:</b> immutable; two periods with the same values are equal. The end is never earlier than the start;
/// both are equal for a period of a single instant. The reader builds the periods from the transcripts: the rules do not
/// know where they come from.</para>
/// </remarks>
public sealed record ActivityPeriod
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
    /// Project of the period
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Kind of the period: work or waiting
    /// </summary>
    public PeriodKind Kind { get; }

    /// <summary>
    /// Start of the period, in UTC
    /// </summary>
    public DateTimeOffset StartUtc { get; }

    /// <summary>
    /// End of the period, in UTC
    /// </summary>
    public DateTimeOffset EndUtc { get; }

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="project">[Cannot be null] Project of the period</param>
    /// <param name="kind">Kind of the period</param>
    /// <param name="startUtc">Start of the period, in UTC</param>
    /// <param name="endUtc">End of the period, in UTC</param>
    /// <exception cref="ArgumentNullException">If the project is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the kind is not defined in <see cref="PeriodKind"/>, or if the end is
    /// earlier than the start</exception>
    /// <exception cref="ArgumentException">If a time is not in UTC</exception>
    public ActivityPeriod(Project project, PeriodKind kind, DateTimeOffset startUtc, DateTimeOffset endUtc)
    {
        // Checks of the two times, which depend on each other
        ModelGuard.RequireUtc(startUtc, nameof(startUtc));
        ModelGuard.RequireUtc(endUtc, nameof(endUtc));
        if (endUtc < startUtc)
            throw new ArgumentOutOfRangeException(nameof(endUtc), endUtc, $"The value of the parameter: {nameof(endUtc)} cannot be earlier than {nameof(startUtc)}");

        // Setting the values
        Project = ModelGuard.RequireNotNull(project, nameof(project));
        Kind = ModelGuard.RequireDefined(kind, nameof(kind));
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
