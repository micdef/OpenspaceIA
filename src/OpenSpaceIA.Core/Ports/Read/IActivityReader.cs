/*
 *	@file	src/OpenSpaceIA.Core/Ports/Read/IActivityReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Read port: gives the activity of a machine to the core
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Ports.Read;

/// <summary>
/// Read port used to get the activity of a machine
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the core does not know where the activity comes from: the files of Claude Code on this machine,
/// or the hub that gathers several machines. Each source is an adapter of this port.</para>
/// <para><b>Behavior:</b> reading only. Read ports and write ports are separate, so that a part of the application that only
/// reads never gets a way to write.</para>
/// </remarks>
public interface IActivityReader
{
    #region Enumerations

    // #N/A

    #endregion

    #region Accessors/Properties

    #region Members

    // #N/A

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Method used to read the activity of the machine at this moment
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> gives the screens and the state rules a complete, coherent view of the machine.</para>
    /// <para><b>Behavior:</b> each call returns a new snapshot; it never changes the source it reads. Each adapter documents
    /// the errors of its own source (files, network).</para>
    /// </remarks>
    /// <param name="cancellationToken">Token used to stop the reading</param>
    /// <returns>The activity of the machine at the moment of the reading</returns>
    /// <exception cref="OperationCanceledException">If the reading is cancelled</exception>
    public Task<ActivitySnapshot> ReadSnapshotAsync(CancellationToken cancellationToken);

    #endregion
}
