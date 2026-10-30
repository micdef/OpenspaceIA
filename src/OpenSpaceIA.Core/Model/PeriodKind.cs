/*
 *	@file	src/OpenSpaceIA.Core/Model/PeriodKind.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Kind of a period of activity of an agent
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
/// Kind of a period of activity of an agent
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> waiting for the user is not resting (doc/analyse/state-definitions.md, § 4), and it is not working
/// either (§ 3): the rules must tell them apart.</para>
/// <para><b>Behavior:</b> only the chief has periods of waiting; the runs of the sub-agents only give periods of work.</para>
/// </remarks>
public enum PeriodKind
{
    /// <summary>The agent works</summary>
    Working,

    /// <summary>The agent waits for the user</summary>
    Waiting,
}
