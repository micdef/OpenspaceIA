/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ClaudeFolderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the allow-list of the folder of Claude Code
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
/// Tests of the allow-list of the folder of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that the Reader can only reach the few kinds of files it needs, and never a secret.</para>
/// <para><b>Behavior:</b> the root is an invented folder that is never created: the class under test must not touch the disk.</para>
/// </remarks>
public sealed class ClaudeFolderTests
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
    /// Method used to build the path of an invented root that does not exist on the disk
    /// </summary>
    /// <returns>The full path of a folder named .claude, unique to the call</returns>
    private static string NewRoot()
    {
        // Function's return
        return Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"), ".claude");
    }

    #endregion

    #region Publics

    /// <summary>
    /// The root path cannot be null, empty or white spaces
    /// </summary>
    /// <param name="rootPath">[Can be null] Blank path that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RootPathCannotBeBlank(string? rootPath)
    {
        // Building the folder with a blank path
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ClaudeFolder(rootPath!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("rootPath");
    }

    /// <summary>
    /// The root path must be fully qualified
    /// </summary>
    /// <param name="rootPath">Relative path that must be refused</param>
    [Theory]
    [InlineData(@"relative\.claude")]
    [InlineData(".claude")]
    [InlineData(@"..\.claude")]
    public void RootPathMustBeFullyQualified(string rootPath)
    {
        // Building the folder with a relative path
        ArgumentException exception = Should.Throw<ArgumentException>(() => new ClaudeFolder(rootPath));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("rootPath");
    }

    /// <summary>
    /// The root path is kept as a normalized full path
    /// </summary>
    [Fact]
    public void RootPathIsTheNormalizedFullPath()
    {
        // Building the folder with a path that climbs and comes back
        string root = NewRoot();
        string given = Path.Combine(root, "sessions", "..");
        ClaudeFolder folder = new ClaudeFolder(given);

        // Checking the normalization
        folder.RootPath.ShouldBe(Path.GetFullPath(root));
    }

    /// <summary>
    /// The root path has no trailing separator
    /// </summary>
    /// <param name="separator">Trailing separator given with the path</param>
    [Theory]
    [InlineData('/')]
    [InlineData('\\')]
    public void RootPathHasNoTrailingSeparator(char separator)
    {
        // Skipping where the character is not a path separator (a backslash is a plain character outside Windows)
        Assert.SkipUnless(
            separator == Path.DirectorySeparatorChar || separator == Path.AltDirectorySeparatorChar,
            "The character is not a path separator on this system");

        // Building the folder with a trailing separator
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root + separator);

        // Checking the path
        folder.RootPath.ShouldBe(Path.GetFullPath(root));
    }

    /// <summary>
    /// The folder of the user is the .claude folder of the profile
    /// </summary>
    [Fact]
    public void FromUserProfileTargetsTheClaudeFolderOfTheProfile()
    {
        // Building the folder of the user
        ClaudeFolder folder = ClaudeFolder.FromUserProfile();

        // Checking the location
        string profile = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        Path.IsPathFullyQualified(folder.RootPath).ShouldBeTrue();
        Path.GetFileName(folder.RootPath).ShouldBe(".claude");
        Path.GetDirectoryName(folder.RootPath).ShouldBe(Path.TrimEndingDirectorySeparator(profile));
    }

    /// <summary>
    /// The path to check cannot be null, empty or white spaces
    /// </summary>
    /// <param name="fullPath">[Can be null] Blank path that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CheckedPathCannotBeBlank(string? fullPath)
    {
        // Building the folder and checking a blank path
        ClaudeFolder folder = new ClaudeFolder(NewRoot());
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => folder.IsReadable(fullPath!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("fullPath");
    }

    /// <summary>
    /// The files of the allow-list are readable, without any file existing on the disk
    /// </summary>
    /// <param name="relativePath">Path relative to the root, of a file that is allowed</param>
    [Theory]
    [InlineData("sessions/123.json")]
    [InlineData("sessions/a1b2-c3d4.json")]
    [InlineData("agents/helper.md")]
    [InlineData("projects/C--p/abc.jsonl")]
    [InlineData("projects/C--p/sub/agent-1.jsonl")]
    [InlineData("projects/C--p/a/b/c/deep.jsonl")]
    [InlineData("projects/C--p/memory.jsonl")]
    [InlineData("projects/C--p/memory2/x.jsonl")]
    [InlineData("sessions/../agents/helper.md")]
    public void AllowedFilesAreReadable(string relativePath)
    {
        // Building the folder and the path of a file that does not exist
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);
        string fullPath = Path.Combine(root, relativePath);

        // Checking the file and that the disk was not touched
        folder.IsReadable(fullPath).ShouldBeTrue();
        Directory.Exists(root).ShouldBeFalse();
    }

    /// <summary>
    /// Both separators are accepted in the path to check
    /// </summary>
    [Fact]
    public void BothSeparatorsAreAccepted()
    {
        // Skipping where the backslash is not a separator
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The backslash is a path separator on Windows only");

        // Building the folder and mixing the separators
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking the mixed paths
        folder.IsReadable(root + "/sessions/1.json").ShouldBeTrue();
        folder.IsReadable(root + @"\sessions/1.json").ShouldBeTrue();
        folder.IsReadable(root + @"/projects\C--p/sub\x.jsonl").ShouldBeTrue();
        folder.IsReadable(root + @"/sessions\1.json.key").ShouldBeFalse();
        folder.IsReadable(root + @"\projects\C--p\memory/MEMORY.md").ShouldBeFalse();
    }

    /// <summary>
    /// Everything that is not in the allow-list is refused
    /// </summary>
    /// <param name="relativePath">Path relative to the root, of a file that must never be read</param>
    [Theory]
    [InlineData(".credentials.json")]
    [InlineData("settings.json")]
    [InlineData("sessions/123.0a1b2c.key")]
    [InlineData("sessions/123.json.key")]
    [InlineData("sessions/sub/123.json")]
    [InlineData("sessions/123.txt")]
    [InlineData("sessions/123.JSON")]
    [InlineData("Sessions/1.json")]
    [InlineData("backups/x.json")]
    [InlineData("telemetry/x.json")]
    [InlineData("uploads/x.md")]
    [InlineData("projects/C--p/memory/MEMORY.md")]
    [InlineData("projects/C--p/memory/x.jsonl")]
    [InlineData("projects/C--p/sub/memory/x.jsonl")]
    [InlineData("projects/a.jsonl")]
    [InlineData("projects/C--p/notes.md")]
    [InlineData("projects/C--p/x.JSONL")]
    [InlineData("agents/sub/x.md")]
    [InlineData("agents/x.MD")]
    [InlineData("agents/x.txt")]
    [InlineData("agents/x.json")]
    [InlineData("sessions/../.credentials.json")]
    [InlineData("projects/C--p/Memory/x.jsonl")]
    [InlineData("projects/C--p/MEMORY/x.jsonl")]
    [InlineData("projects/memory/x.jsonl")]
    [InlineData("projects/Memory/x.jsonl")]
    [InlineData("sessions/.json")]
    [InlineData("agents/.md")]
    [InlineData("projects/C--p/.jsonl")]
    [InlineData("projects/C--p/memory::$INDEX_ALLOCATION/m.jsonl")]
    [InlineData("projects/C--p/a:b/x.jsonl")]
    [InlineData("sessions/1.json:secret")]
    [InlineData("agents/x.md::$DATA")]
    public void ForbiddenFilesAreNotReadable(string relativePath)
    {
        // Building the folder and the path of the forbidden file
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking the refusal
        folder.IsReadable(Path.Combine(root, relativePath)).ShouldBeFalse();
    }

    /// <summary>
    /// A relative path is refused, even when it looks like an allowed file: it would depend on the current folder
    /// </summary>
    /// <param name="relativePath">Relative path that must be refused</param>
    [Theory]
    [InlineData("sessions/1.json")]
    [InlineData(@"agents\x.md")]
    [InlineData(".claude/sessions/1.json")]
    public void RelativePathsAreNotReadable(string relativePath)
    {
        // Building the folder
        ClaudeFolder folder = new ClaudeFolder(NewRoot());

        // Checking the refusal
        folder.IsReadable(relativePath).ShouldBeFalse();
    }

    /// <summary>
    /// A path that the system cannot even resolve is refused, without an exception
    /// </summary>
    [Fact]
    public void UnresolvablePathsAreRefusedWithoutException()
    {
        // Building the folder
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking a path with a null character and a path longer than any system accepts
        folder.IsReadable(Path.Combine(root, "sessions", "1\0.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "projects", new string('a', 40000), "x.jsonl")).ShouldBeFalse();
    }

    /// <summary>
    /// The root of a drive, or of the file system, is refused as the folder of Claude Code: it would open its whole disk
    /// </summary>
    [Fact]
    public void RootOfTheDiskCannotBeTheFolder()
    {
        // Building the folder on the root of the disk of the temporary folder
        string diskRoot = Path.GetPathRoot(Path.GetTempPath())!;
        ArgumentException exception = Should.Throw<ArgumentException>(() => new ClaudeFolder(diskRoot));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("rootPath");
    }

    /// <summary>
    /// A root written as a device path is refused: it would bypass the refusal of the root of a disk
    /// </summary>
    /// <param name="rootPath">Device path that must be refused</param>
    [Theory]
    [InlineData(@"\\?\C:\")]
    [InlineData(@"\\?\C:\Users\dev\.claude")]
    [InlineData(@"\\.\C:\Users\dev\.claude")]
    [InlineData(@"\\?\UNC\localhost\C$")]
    public void DevicePathCannotBeTheFolder(string rootPath)
    {
        // Skipping where device paths do not exist
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Device paths are a feature of Windows");

        // Building the folder on a device path
        ArgumentException exception = Should.Throw<ArgumentException>(() => new ClaudeFolder(rootPath));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentException>();
        exception.ParamName.ShouldBe("rootPath");
    }

    /// <summary>
    /// A relative path is refused even when, resolved from the current folder of the process, it would land inside the root
    /// </summary>
    [Fact]
    public void RelativePathsAreNotReadableEvenWhenTheyResolveInsideTheRoot()
    {
        // Building a root under the current folder, so that a relative path resolves inside it (the disk is not touched)
        ClaudeFolder folder = new ClaudeFolder(Path.Combine(Environment.CurrentDirectory, ".claude"));

        // Checking the refusal of the relative path, and the success of the same file given with its full path
        folder.IsReadable(Path.Combine(".claude", "sessions", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(folder.RootPath, "sessions", "1.json")).ShouldBeTrue();
    }

    /// <summary>
    /// The root itself and its first-level folders are not readable
    /// </summary>
    [Fact]
    public void TheRootAndItsFoldersAreNotReadable()
    {
        // Building the folder
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking the root (with and without trailing separator) and the folders of the allow-list
        folder.IsReadable(root).ShouldBeFalse();
        folder.IsReadable(root + Path.DirectorySeparatorChar).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "sessions")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "agents")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "projects")).ShouldBeFalse();
    }

    /// <summary>
    /// A path outside the root is refused, even when it looks like an allowed file
    /// </summary>
    [Fact]
    public void PathsOutsideTheRootAreNotReadable()
    {
        // Building the folder and the places around it
        string root = NewRoot();
        string parent = Path.GetDirectoryName(root)!;
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking the parent, a sibling and a sibling that starts with the same text
        folder.IsReadable(Path.Combine(parent, "sessions", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(parent, ".claude-other", "sessions", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(parent, ".claude-evil", "sessions", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root + "-evil", "agents", "x.md")).ShouldBeFalse();
    }

    /// <summary>
    /// A path that climbs out of the root with two dots is refused
    /// </summary>
    [Fact]
    public void PathsClimbingOutOfTheRootAreNotReadable()
    {
        // Building the folder
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking the paths that leave the root and come back to look like allowed files
        folder.IsReadable(Path.Combine(root, "..", "x", "sessions", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "..", ".claude", "sessions", "..", ".credentials.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "sessions", "..", "..", "sessions", "1.json")).ShouldBeFalse();
    }

    /// <summary>
    /// The names are compared with their case, so a lookalike name never opens the allow-list
    /// </summary>
    [Fact]
    public void NamesAreComparedWithTheCase()
    {
        // Building the folder
        string root = NewRoot();
        ClaudeFolder folder = new ClaudeFolder(root);

        // Checking that only the exact case is allowed
        folder.IsReadable(Path.Combine(root, "sessions", "1.json")).ShouldBeTrue();
        folder.IsReadable(Path.Combine(root, "SESSIONS", "1.json")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "Agents", "x.md")).ShouldBeFalse();
        folder.IsReadable(Path.Combine(root, "Projects", "C--p", "x.jsonl")).ShouldBeFalse();
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
