/*
 *	@file	test/OpenSpaceIA.Core.Tests/Model/MachineTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the Machine model
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Tests.Model;

/// <summary>
/// Tests of the Machine model
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves the contract of the type before its implementation exists.</para>
/// <para><b>Behavior:</b> fixed dates and invented data only; no clock, no file, no state shared between tests.</para>
/// </remarks>
public sealed class MachineTests
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
    /// A machine keeps the name it was given
    /// </summary>
    [Fact]
    public void MachineKeepsItsName()
    {
        // Building the machine
        Machine machine = new Machine("workstation-01");

        // Checking the name
        machine.Name.ShouldBe("workstation-01");
    }

    /// <summary>
    /// A machine name cannot be null, empty or white spaces
    /// </summary>
    /// <param name="name">[Can be null] Blank name that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MachineNameCannotBeBlank(string? name)
    {
        // Building the machine with a blank name
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new Machine(name!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("name");
    }

    /// <summary>
    /// Two machines built with the same name are equal
    /// </summary>
    [Fact]
    public void MachinesWithTheSameNameAreEqual()
    {
        // Building two machines with the same name
        Machine first = new Machine("workstation-01");
        Machine second = new Machine("workstation-01");

        // Checking the value equality
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Two machines with different names are not equal
    /// </summary>
    [Fact]
    public void MachinesWithDifferentNamesAreNotEqual()
    {
        // Building two machines with different names
        Machine first = new Machine("workstation-01");
        Machine second = new Machine("workstation-02");

        // Checking the value equality
        first.ShouldNotBe(second);
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
