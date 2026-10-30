/*
 *	@file	src/OpenSpaceIA.Core/Model/WaitReason.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Why a main session waits for the user
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
/// Why a main session waits for the user
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the "!" bubble at the coffee machine says what the chief needs (doc/analyse/claude-model-decisions.md,
/// decision 2).</para>
/// <para><b>Behavior:</b> deduced from the tool call left open in the transcript while the session is waiting.</para>
/// </remarks>
public enum WaitReason
{
    /// <summary>The session asks the user a question</summary>
    Question,

    /// <summary>The session asks the user for the permission to use a tool</summary>
    Permission,
}
