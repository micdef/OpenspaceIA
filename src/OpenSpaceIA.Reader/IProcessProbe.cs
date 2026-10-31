/*
 *	@file	src/OpenSpaceIA.Reader/IProcessProbe.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Port used to know whether the process of a session is still running
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
/// Port used to know whether the process of a session of Claude Code is still running
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the way to ask the system differs between Windows and Linux, and the tests need invented processes;
/// this is the only reason for this port.</para>
/// <para><b>Behavior:</b> never throws because of the state of a process (missing, ended, hidden): the answer says so; only a
/// wrong argument throws.</para>
/// </remarks>
internal interface IProcessProbe
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
    /// Method used to know whether the process of a session is still running
    /// </summary>
    /// <param name="processId">ID of the process, greater than 0</param>
    /// <param name="processStart">[Cannot be null] Start time of the process written by Claude Code (<c>procStart</c>):
    /// digits only</param>
    /// <param name="detail">[Can be null] Why the answer is unknown; null otherwise</param>
    /// <returns>
    /// Alive   ==> A process with this ID runs and started at this time<br/>
    /// Closed  ==> No process has this ID, or it started at another time<br/>
    /// Unknown ==> The system cannot say
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If the ID is not greater than 0</exception>
    /// <exception cref="ArgumentException">If the start time is null, empty, or holds something else than digits</exception>
    public ProcessLiveness Check(int processId, string processStart, out string? detail);

    #endregion
}
