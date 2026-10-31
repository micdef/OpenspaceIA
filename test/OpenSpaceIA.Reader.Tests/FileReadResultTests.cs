/*
 *	@file	test/OpenSpaceIA.Reader.Tests/FileReadResultTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the result of a file reading
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
/// Tests of the result of a file reading
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> invented data only; no file, no clock, no state shared between tests.</para>
/// </remarks>
public sealed class FileReadResultTests
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
    /// A successful result carries the content and no failure
    /// </summary>
    [Fact]
    public void SuccessCarriesTheContentAndNoFailure()
    {
        // Building the result
        FileReadResult result = FileReadResult.Success("hello");

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("hello");
        result.Failure.ShouldBeNull();
        result.Detail.ShouldBeNull();
    }

    /// <summary>
    /// An empty content is a valid success
    /// </summary>
    [Fact]
    public void SuccessAcceptsAnEmptyContent()
    {
        // Building the result
        FileReadResult result = FileReadResult.Success(string.Empty);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A successful result cannot be built without content
    /// </summary>
    [Fact]
    public void SuccessCannotHaveANullContent()
    {
        // Building the result with a null content
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => FileReadResult.Success(null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("content");
    }

    /// <summary>
    /// A failed result carries the failure and its detail, and no content
    /// </summary>
    /// <param name="failureValue">Value of the failure that must be kept as is (an integer, because the enumeration is internal)</param>
    [Theory]
    [InlineData((int)FileReadFailure.NotAllowed)]
    [InlineData((int)FileReadFailure.NotFound)]
    [InlineData((int)FileReadFailure.AccessDenied)]
    [InlineData((int)FileReadFailure.TooLarge)]
    [InlineData((int)FileReadFailure.Unreadable)]
    public void FailedCarriesTheFailureAndItsDetailWithoutContent(int failureValue)
    {
        // Building the result
        FileReadFailure failure = (FileReadFailure)failureValue;
        FileReadResult result = FileReadResult.Failed(failure, "invented detail");

        // Checking the result
        result.IsSuccess.ShouldBeFalse();
        result.Failure.ShouldBe(failure);
        result.Detail.ShouldBe("invented detail");
        result.Content.ShouldBeNull();
    }

    /// <summary>
    /// A failure that is not part of the enumeration is refused
    /// </summary>
    [Fact]
    public void FailedRefusesAnUndefinedFailure()
    {
        // Building the result with a value outside the enumeration
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => FileReadResult.Failed((FileReadFailure)999, "invented detail"));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("failure");
    }

    /// <summary>
    /// The detail of a failure cannot be null, empty or white spaces
    /// </summary>
    /// <param name="detail">[Can be null] Blank detail that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FailedDetailCannotBeBlank(string? detail)
    {
        // Building the result with a blank detail
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(
            () => FileReadResult.Failed(FileReadFailure.NotFound, detail!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("detail");
    }

    /// <summary>
    /// Two results built from the same values are equal
    /// </summary>
    [Fact]
    public void ResultsWithTheSameValuesAreEqual()
    {
        // Building the results
        FileReadResult firstSuccess = FileReadResult.Success("a");
        FileReadResult secondSuccess = FileReadResult.Success("a");
        FileReadResult firstFailure = FileReadResult.Failed(FileReadFailure.TooLarge, "big");
        FileReadResult secondFailure = FileReadResult.Failed(FileReadFailure.TooLarge, "big");

        // Checking the value equality
        firstSuccess.ShouldBe(secondSuccess);
        firstSuccess.GetHashCode().ShouldBe(secondSuccess.GetHashCode());
        firstFailure.ShouldBe(secondFailure);
        firstSuccess.ShouldNotBe(firstFailure);
    }

    /// <summary>
    /// The text of a result never shows the content of the file, only its length, so that a log never leaks it
    /// </summary>
    [Fact]
    public void TextOfAResultHidesTheContent()
    {
        // Building a successful result with an invented secret
        FileReadResult result = FileReadResult.Success("invented-secret-value");

        // Checking the text of the result
        string text = result.ToString();
        text.ShouldNotContain("invented-secret-value");
        text.ShouldContain("21");
    }

    /// <summary>
    /// The text of a failed result shows its reason and its details, and says that there is no content
    /// </summary>
    [Fact]
    public void TextOfAFailedResultShowsItsReason()
    {
        // Building a failed result
        FileReadResult result = FileReadResult.Failed(FileReadFailure.TooLarge, "invented detail");

        // Checking the text of the result
        result.ToString().ShouldBe("FileReadResult { IsSuccess = False, ContentLength = none, Failure = TooLarge, Detail = invented detail }");
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
