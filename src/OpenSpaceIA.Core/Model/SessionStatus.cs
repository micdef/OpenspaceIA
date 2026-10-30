/*
 *	@file	src/OpenSpaceIA.Core/Model/SessionStatus.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Status of a main session of Claude Code (the chief)
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
/// Status of a main session of Claude Code (the chief), as Claude Code gives it
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the input of the state of the chief (doc/analyse/claude-model-decisions.md, decision 1).</para>
/// <para><b>Behavior:</b> a fact read from the files, not a computed state: the state rules turn it into a state.</para>
/// </remarks>
public enum SessionStatus
{
    /// <summary>The session is working on a turn</summary>
    Busy,

    /// <summary>The session waits for the user: a question or a permission prompt</summary>
    Waiting,

    /// <summary>The session has finished its turn and waits for nothing</summary>
    Idle,
}
