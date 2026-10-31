/*
 *	@file	src/OpenSpaceIA.Reader/FileReadFailure.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Why a file of Claude Code could not be read
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
/// Why a file of Claude Code could not be read
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> a missing, broken or locked file never stops the reader (doc/analyse/roadmap.md, 0.4.0): each
/// failure has its own reason, so that the caller can report it instead of losing it.</para>
/// <para><b>Behavior:</b> one value per kind of failure; the details are in the text of the result.</para>
/// </remarks>
internal enum FileReadFailure
{
    /// <summary>The file is not in the allow-list of the reader: it was not opened, nor even looked for</summary>
    NotAllowed,

    /// <summary>The file, or one of its folders, does not exist</summary>
    NotFound,

    /// <summary>The system refused to open the file, or the path is a folder</summary>
    AccessDenied,

    /// <summary>The file is larger than the size the reader accepts</summary>
    TooLarge,

    /// <summary>The file could not be read: locked by another program, or an error of the disk</summary>
    Unreadable,
}
