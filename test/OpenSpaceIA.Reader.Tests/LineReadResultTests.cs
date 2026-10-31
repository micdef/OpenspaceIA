/*
 *	@file	test/OpenSpaceIA.Reader.Tests/LineReadResultTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the result of a reading of the lines of a file
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
/// Tests of the result of a reading of the lines of a file
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that a result is always either a success with its counts, or a failure with its reason.</para>
/// <para><b>Behavior:</b> invented data only; no file, no state shared between tests.</para>
/// </remarks>
public sealed class LineReadResultTests
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

    // #N/A

    #endregion

    #region Publics

    /// <summary>
    /// A successful result carries its counts and no failure
    /// </summary>
    [Fact]
    public void SuccessCarriesItsCounts()
    {
        // Building the result
        LineReadResult result = LineReadResult.Success(12, 3, 4096);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.LineCount.ShouldBe(12);
        result.TooLongLineCount.ShouldBe(3);
        result.EndOffset.ShouldBe(4096);
        result.Failure.ShouldBeNull();
        result.Detail.ShouldBeNull();
    }

    /// <summary>
    /// A count cannot be negative
    /// </summary>
    [Fact]
    public void CountsCannotBeNegative()
    {
        // Building results with negative counts
        ArgumentOutOfRangeException lines = Should.Throw<ArgumentOutOfRangeException>(() => LineReadResult.Success(-1, 0, 0));
        ArgumentOutOfRangeException tooLong = Should.Throw<ArgumentOutOfRangeException>(() => LineReadResult.Success(0, -1, 0));
        ArgumentOutOfRangeException offset = Should.Throw<ArgumentOutOfRangeException>(() => LineReadResult.Success(0, 0, -1));

        // Checking the parameters in fault
        lines.ParamName.ShouldBe("lineCount");
        tooLong.ParamName.ShouldBe("tooLongLineCount");
        offset.ParamName.ShouldBe("endOffset");
    }

    /// <summary>
    /// A failed result carries its reason and its details, and no count
    /// </summary>
    [Fact]
    public void FailedCarriesItsReason()
    {
        // Building the result
        LineReadResult result = LineReadResult.Failed(FileReadFailure.NotFound, "invented detail");

        // Checking the result
        result.IsSuccess.ShouldBeFalse();
        result.Failure.ShouldBe(FileReadFailure.NotFound);
        result.Detail.ShouldBe("invented detail");
        result.LineCount.ShouldBe(0);
        result.TooLongLineCount.ShouldBe(0);
    }

    /// <summary>
    /// A failure outside the enumeration, or a blank detail, is refused
    /// </summary>
    [Fact]
    public void FailedRefusesAnUndefinedFailureAndABlankDetail()
    {
        // Building failed results with invalid values
        ArgumentOutOfRangeException undefined = Should.Throw<ArgumentOutOfRangeException>(() => LineReadResult.Failed((FileReadFailure)999, "invented detail"));
        ArgumentNullException blank = Should.Throw<ArgumentNullException>(() => LineReadResult.Failed(FileReadFailure.NotFound, " "));

        // Checking the exact types and the parameters in fault
        undefined.ShouldBeOfType<ArgumentOutOfRangeException>();
        undefined.ParamName.ShouldBe("failure");
        blank.ParamName.ShouldBe("detail");
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
