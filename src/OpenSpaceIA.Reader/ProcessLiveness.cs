/*
 *	@file	src/OpenSpaceIA.Reader/ProcessLiveness.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Whether the process of a session of Claude Code is still running
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Whether the process of a session of Claude Code is still running
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a session file can stay behind after a crash, and its process ID can be given to another program
/// (doc/analyse/claude-model-decisions.md, decision 3).</para>
/// <para><b>Behavior:</b> a process is alive only when a process with this ID runs and started at the time written in the
/// session file.</para>
/// </remarks>
internal enum ProcessLiveness
{
    /// <summary>The process runs, and it started at the time of the session file: the session is open</summary>
    Alive,

    /// <summary>No process has this ID, or the process that has it started at another time: the session is closed</summary>
    Closed,

    /// <summary>The system cannot say (access refused, unknown system): the session is kept, and the doubt is reported</summary>
    Unknown,
}
