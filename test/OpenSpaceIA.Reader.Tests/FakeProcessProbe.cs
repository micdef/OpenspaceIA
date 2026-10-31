/*
 *	@file	test/OpenSpaceIA.Reader.Tests/FakeProcessProbe.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Fake port that invents the processes of the sessions, for the tests
 */

// Using
// #N/A

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Fake port that gives the answer chosen by the test for each process ID, and remembers each question
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the tests of the readers of sessions invent their processes instead of depending on the processes
/// of the machine.</para>
/// <para><b>Behavior:</b> a process ID without an answer chosen is closed; an unknown answer gives a fixed detail.</para>
/// </remarks>
internal sealed class FakeProcessProbe : IProcessProbe
{
    #region Members

    #region Constants

    /// <summary>
    /// Detail given with an unknown answer
    /// </summary>
    public const string UnknownDetail = "invented reason of the doubt";

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
    /// Answer for each process ID; a process ID without an answer is closed
    /// </summary>
    public Dictionary<int, ProcessLiveness> Answers { get; } = new Dictionary<int, ProcessLiveness>();

    /// <summary>
    /// Questions asked, as "process ID, space, start time"
    /// </summary>
    public List<string> Questions { get; } = new List<string>();

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    // #N/A

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    // #N/A

    #endregion

    #region Publics

    /// <inheritdoc/>
    public ProcessLiveness Check(int processId, string processStart, out string? detail)
    {
        // Remembering the question, and giving the chosen answer
        Questions.Add($"{processId} {processStart}");
        ProcessLiveness answer = Answers.TryGetValue(processId, out ProcessLiveness chosen) ? chosen : ProcessLiveness.Closed;
        detail = answer == ProcessLiveness.Unknown ? UnknownDetail : null;

        // Function's return
        return answer;
    }

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
