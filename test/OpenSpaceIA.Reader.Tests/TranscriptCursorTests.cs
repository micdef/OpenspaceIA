/*
 *	@file	test/OpenSpaceIA.Reader.Tests/TranscriptCursorTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the cursor of the reading of a transcript
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
/// Tests of the cursor of the reading of a transcript
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that the cursor only moves forward, and starts again from nothing when the transcript is not
/// the one that was read.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class TranscriptCursorTests
{
    #region Members

    #region Constants

    // #N/A

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

    // #N/A

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

    /// <summary>
    /// Method used to build a cursor that has read up to a position, with one open call and lines skipped
    /// </summary>
    /// <returns>The cursor, at position 100</returns>
    private static TranscriptCursor<OpenToolCalls> ReadCursor()
    {
        // Moving a new cursor, with an open call
        TranscriptCursor<OpenToolCalls> cursor = new TranscriptCursor<OpenToolCalls>();
        cursor.Follower.Add("{\"type\":\"assistant\",\"message\":{\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"Read\"}]}}");
        cursor.MoveTo(100, 2);

        // Function's return
        return cursor;
    }

    #endregion

    #region Publics

    /// <summary>
    /// A new cursor is at the start, with nothing found
    /// </summary>
    [Fact]
    public void NewCursorIsAtTheStart()
    {
        // Building a cursor
        TranscriptCursor<OpenToolCalls> cursor = new TranscriptCursor<OpenToolCalls>();

        // Checking its state
        cursor.Offset.ShouldBe(0);
        cursor.Follower.Count.ShouldBe(0);
        cursor.TooLongLineCount.ShouldBe(0);
    }

    /// <summary>
    /// Moving the cursor keeps the new position and adds the lines skipped
    /// </summary>
    [Fact]
    public void MovingAddsTheLinesSkipped()
    {
        // Moving the cursor twice
        TranscriptCursor<OpenToolCalls> cursor = ReadCursor();
        cursor.MoveTo(100, 1);

        // Checking the position and the count
        cursor.Offset.ShouldBe(100);
        cursor.TooLongLineCount.ShouldBe(3);
    }

    /// <summary>
    /// A cursor never moves backward, and a negative count is refused
    /// </summary>
    [Fact]
    public void CursorNeverMovesBackward()
    {
        // Moving the cursor backward, then with a negative count
        TranscriptCursor<OpenToolCalls> cursor = ReadCursor();
        ArgumentOutOfRangeException backward = Should.Throw<ArgumentOutOfRangeException>(() => cursor.MoveTo(99, 0));
        ArgumentOutOfRangeException negative = Should.Throw<ArgumentOutOfRangeException>(() => cursor.MoveTo(100, -1));

        // Checking the parameters in fault
        backward.ParamName.ShouldBe("offset");
        negative.ParamName.ShouldBe("tooLongLineCount");
    }

    /// <summary>
    /// A transcript at least as long as the position keeps the cursor; a shorter one starts it again from nothing
    /// </summary>
    [Fact]
    public void ShorterTranscriptRestartsTheCursor()
    {
        // Checking the same size, then one byte less
        TranscriptCursor<OpenToolCalls> cursor = ReadCursor();
        cursor.RestartIfShorter(100).ShouldBeFalse();
        cursor.Offset.ShouldBe(100);
        cursor.RestartIfShorter(99).ShouldBeTrue();

        // Checking that nothing found is kept
        cursor.Offset.ShouldBe(0);
        cursor.Follower.Count.ShouldBe(0);
        cursor.TooLongLineCount.ShouldBe(0);
    }

    /// <summary>
    /// A restart forgets the position, the open calls and the lines skipped
    /// </summary>
    [Fact]
    public void RestartForgetsEverything()
    {
        // Restarting a cursor that has read
        TranscriptCursor<OpenToolCalls> cursor = ReadCursor();
        cursor.Restart();

        // Checking its state
        cursor.Offset.ShouldBe(0);
        cursor.Follower.Count.ShouldBe(0);
        cursor.TooLongLineCount.ShouldBe(0);
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
