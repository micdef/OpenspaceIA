/*
 *	@file	src/OpenSpaceIA.Core/Model/PlanTaskStatus.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Status of a task of a task list
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
/// Status of a task of the task list of a session
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the task cards show what is done, what is in progress and what is left.</para>
/// <para><b>Behavior:</b> the status of the last update of the task list (doc/analyse/claude-model-decisions.md, decision 8).</para>
/// </remarks>
public enum PlanTaskStatus
{
    /// <summary>The task is not started</summary>
    Pending,

    /// <summary>The task is being done</summary>
    InProgress,

    /// <summary>The task is done</summary>
    Completed,
}
