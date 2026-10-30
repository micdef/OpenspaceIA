/*
 *	@file	src/OpenSpaceIA.Core/States/AgentState.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	State of an agent at a given moment
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
/// State of an agent at a given moment
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> places the character in the office: at its desk, at the coffee machine with the "!" bubble, or in
/// the lounge (doc/analyse/state-definitions.md, § 2).</para>
/// <para><b>Behavior:</b> exactly one state at a time: an agent is never working and idle together. "Overloaded" and
/// "under-used" are not states: they are shown on top of the state.</para>
/// </remarks>
public enum AgentState
{
    /// <summary>The agent works</summary>
    Working,

    /// <summary>The agent waits for the user: a question or a permission (the chief only)</summary>
    WaitingForYou,

    /// <summary>The agent does nothing</summary>
    Idle,
}
