/*
 *	@file	test/OpenSpaceIA.Reader.Tests/ClaudeFileReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the read-only reading of the files of Claude Code
 */

// Using
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the read-only reading of the files of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that the Reader reads only what the allow-list permits, never writes, and turns every
/// failure into a result instead of an exception.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. Invented data only; no state shared between tests.</para>
/// </remarks>
public sealed class ClaudeFileReaderTests : IDisposable
{
    #region Members

    #region Constants

    // #N/A

    #endregion

    #region Variables

    /// <summary>
    /// Unique folder of the instance, parent of the invented .claude folder; deleted by Dispose
    /// </summary>
    private readonly string _workFolder;

    /// <summary>
    /// Invented .claude folder of the instance
    /// </summary>
    private readonly string _root;

    /// <summary>
    /// Folder of the invented .claude folder
    /// </summary>
    private readonly ClaudeFolder _folder;

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

    /// <summary>
    /// Constructor creating the invented .claude folder of the instance
    /// </summary>
    public ClaudeFileReaderTests()
    {
        // Creating the folders
        _workFolder = Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(_workFolder, ".claude");
        Directory.CreateDirectory(_root);

        // Building the folder under test
        _folder = new ClaudeFolder(_root);
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to write a file under the invented root, creating its folders
    /// </summary>
    /// <param name="relativePath">[Cannot be null] Path of the file, relative to the root</param>
    /// <param name="content">[Cannot be null] Bytes of the file</param>
    /// <returns>The full path of the file</returns>
    private string WriteFile(string relativePath, byte[] content)
    {
        // Creating the folders and the file
        string fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, content);

        // Function's return
        return fullPath;
    }

    /// <summary>
    /// Method used to write a text file in UTF-8 without BOM under the invented root
    /// </summary>
    /// <param name="relativePath">[Cannot be null] Path of the file, relative to the root</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <returns>The full path of the file</returns>
    private string WriteText(string relativePath, string text)
    {
        // Function's return
        return WriteFile(relativePath, new UTF8Encoding(false).GetBytes(text));
    }

    /// <summary>
    /// Method used to describe everything under the invented root: folders, files, attributes, dates, sizes and contents
    /// </summary>
    /// <returns>One line per folder or file, in a stable order</returns>
    private List<string> Snapshot()
    {
        // Listing the entries of the tree
        List<string> lines = new List<string>();
        foreach (string entry in Directory.EnumerateFileSystemEntries(_root, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(_root, entry);
            lines.Add(Directory.Exists(entry)
                ? $"D {relative} {File.GetAttributes(entry)}"
                : $"F {relative} {File.GetAttributes(entry)} {File.GetCreationTimeUtc(entry).Ticks} {File.GetLastWriteTimeUtc(entry).Ticks} "
                    + $"{new FileInfo(entry).Length} {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry)))}");
        }
        lines.Sort(StringComparer.Ordinal);

        // Function's return
        return lines;
    }

    #endregion

    #region Publics

    /// <summary>
    /// The folder is required
    /// </summary>
    [Fact]
    public void FolderCannotBeNull()
    {
        // Building the reader without folder
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new ClaudeFileReader(null!));
        ArgumentNullException exceptionWithLimit = Should.Throw<ArgumentNullException>(() => new ClaudeFileReader(null!, 10));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("folder");
        exceptionWithLimit.ParamName.ShouldBe("folder");
    }

    /// <summary>
    /// The maximum size must be positive
    /// </summary>
    /// <param name="maximumBytes">Size that must be refused</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void MaximumBytesMustBePositive(long maximumBytes)
    {
        // Building the reader with a size that is not positive
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(
            () => new ClaudeFileReader(_folder, maximumBytes));

        // Checking the exact type and the parameter in fault
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
        exception.ParamName.ShouldBe("maximumBytes");
    }

    /// <summary>
    /// Without explicit limit, a file is limited to one MiB
    /// </summary>
    [Fact]
    public void DefaultMaximumIsOneMebibyte()
    {
        // Building the reader without limit
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Checking the limit
        ClaudeFileReader.DefaultMaximumBytes.ShouldBe(1048576L);
        reader.MaximumBytes.ShouldBe(ClaudeFileReader.DefaultMaximumBytes);
    }

    /// <summary>
    /// The folder given to the reader is the one it reads
    /// </summary>
    [Fact]
    public void FolderIsKept()
    {
        // Building the reader
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Checking the folder
        reader.Folder.ShouldBeSameAs(_folder);
    }

    /// <summary>
    /// An explicit limit is kept
    /// </summary>
    [Fact]
    public void ExplicitMaximumIsKept()
    {
        // Building the reader with a limit
        ClaudeFileReader reader = new ClaudeFileReader(_folder, 42);

        // Checking the limit
        reader.MaximumBytes.ShouldBe(42L);
    }

    /// <summary>
    /// A file of exactly the default limit is read, one byte more is refused
    /// </summary>
    [Fact]
    public void DefaultMaximumBoundaryIsExact()
    {
        // Writing files around the limit
        string atLimit = WriteFile("sessions/1.json", new byte[1048576]);
        string overLimit = WriteFile("sessions/2.json", new byte[1048577]);
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Checking the two files
        reader.ReadText(atLimit).IsSuccess.ShouldBeTrue();
        reader.ReadText(overLimit).Failure.ShouldBe(FileReadFailure.TooLarge);
    }

    /// <summary>
    /// The path to read cannot be null, empty or white spaces
    /// </summary>
    /// <param name="fullPath">[Can be null] Blank path that must be refused</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PathToReadCannotBeBlank(string? fullPath)
    {
        // Reading a blank path
        ClaudeFileReader reader = new ClaudeFileReader(_folder);
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => reader.ReadText(fullPath!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("fullPath");
    }

    /// <summary>
    /// A file that is not in the allow-list is refused even when it exists, and its content is never returned
    /// </summary>
    /// <param name="relativePath">Path relative to the root, of an existing file that must never be read</param>
    [Theory]
    [InlineData(".credentials.json")]
    [InlineData("settings.json")]
    [InlineData("sessions/123.0a1b2c.key")]
    [InlineData("sessions/123.json.key")]
    [InlineData("backups/x.json")]
    [InlineData("telemetry/x.json")]
    [InlineData("uploads/x.md")]
    [InlineData("projects/C--p/memory/MEMORY.md")]
    [InlineData("projects/C--p/memory/x.jsonl")]
    public void ForbiddenExistingFileIsNotAllowed(string relativePath)
    {
        // Writing a forbidden file that exists and has content
        string fullPath = WriteText(relativePath, "invented-secret-content");
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Reading it
        FileReadResult result = reader.ReadText(fullPath);

        // Checking the refusal and that nothing leaked
        result.IsSuccess.ShouldBeFalse();
        result.Failure.ShouldBe(FileReadFailure.NotAllowed);
        result.Content.ShouldBeNull();
        result.Detail.ShouldNotBeNullOrWhiteSpace();
        result.Detail!.ShouldNotContain("invented-secret-content");
    }

    /// <summary>
    /// An existing file outside the root is refused
    /// </summary>
    [Fact]
    public void ExistingFileOutsideTheRootIsNotAllowed()
    {
        // Writing a file next to the root, in a folder that looks like an allowed one
        string outside = Path.Combine(_workFolder, "sessions", "1.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outside)!);
        File.WriteAllText(outside, "invented-outside");
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Reading it
        FileReadResult result = reader.ReadText(outside);

        // Checking the refusal
        result.Failure.ShouldBe(FileReadFailure.NotAllowed);
        result.Content.ShouldBeNull();
    }

    /// <summary>
    /// A file that does not exist is reported as not found
    /// </summary>
    /// <param name="relativePath">Path relative to the root, of an allowed file that does not exist</param>
    [Theory]
    [InlineData("sessions/missing.json")]
    [InlineData("agents/missing.md")]
    [InlineData("projects/C--p/sub/missing.jsonl")]
    public void MissingFileIsNotFound(string relativePath)
    {
        // Reading an allowed file whose folder does not exist
        ClaudeFileReader reader = new ClaudeFileReader(_folder);
        FileReadResult missingFolder = reader.ReadText(Path.Combine(_root, relativePath));

        // Reading an allowed file in a folder that exists
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(_root, relativePath))!);
        FileReadResult missingFile = reader.ReadText(Path.Combine(_root, relativePath));

        // Checking both results
        missingFolder.Failure.ShouldBe(FileReadFailure.NotFound);
        missingFolder.Content.ShouldBeNull();
        missingFolder.Detail.ShouldNotBeNullOrWhiteSpace();
        missingFile.Failure.ShouldBe(FileReadFailure.NotFound);
        missingFile.Detail.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// A missing root is reported as not found
    /// </summary>
    [Fact]
    public void MissingRootIsNotFound()
    {
        // Reading from a root that does not exist
        ClaudeFolder absent = new ClaudeFolder(Path.Combine(_workFolder, "absent", ".claude"));
        ClaudeFileReader reader = new ClaudeFileReader(absent);
        FileReadResult result = reader.ReadText(Path.Combine(absent.RootPath, "sessions", "1.json"));

        // Checking the result
        result.Failure.ShouldBe(FileReadFailure.NotFound);
    }

    /// <summary>
    /// A folder where a file is expected is reported as access denied
    /// </summary>
    [Fact]
    public void FolderInPlaceOfAFileIsAccessDenied()
    {
        // Creating a folder named like an allowed file
        string fullPath = Path.Combine(_root, "sessions", "1.json");
        Directory.CreateDirectory(fullPath);
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Reading it
        FileReadResult result = reader.ReadText(fullPath);

        // Checking the result
        result.Failure.ShouldBe(FileReadFailure.AccessDenied);
        result.Content.ShouldBeNull();
        result.Detail.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// A file of exactly the maximum size is read, one byte more is too large
    /// </summary>
    [Fact]
    public void FileLargerThanTheMaximumIsTooLarge()
    {
        // Writing files around the limit of 10 bytes
        string atLimit = WriteFile("sessions/1.json", Encoding.ASCII.GetBytes("0123456789"));
        string overLimit = WriteFile("sessions/2.json", Encoding.ASCII.GetBytes("0123456789A"));
        ClaudeFileReader reader = new ClaudeFileReader(_folder, 10);

        // Reading both
        FileReadResult accepted = reader.ReadText(atLimit);
        FileReadResult refused = reader.ReadText(overLimit);

        // Checking the results
        accepted.IsSuccess.ShouldBeTrue();
        accepted.Content.ShouldBe("0123456789");
        refused.IsSuccess.ShouldBeFalse();
        refused.Failure.ShouldBe(FileReadFailure.TooLarge);
        refused.Content.ShouldBeNull();
        refused.Detail.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// The files of the three allowed kinds are read
    /// </summary>
    /// <param name="relativePath">Path relative to the root, of an allowed file</param>
    [Theory]
    [InlineData("sessions/123.json")]
    [InlineData("agents/helper.md")]
    [InlineData("projects/C--p/abc.jsonl")]
    [InlineData("projects/C--p/sub/agent-1.jsonl")]
    public void AllowedFileIsRead(string relativePath)
    {
        // Writing an allowed file
        string fullPath = WriteText(relativePath, "{\"id\":\"invented\"}");
        ClaudeFileReader reader = new ClaudeFileReader(_folder);

        // Reading it
        FileReadResult result = reader.ReadText(fullPath);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("{\"id\":\"invented\"}");
        result.Failure.ShouldBeNull();
        result.Detail.ShouldBeNull();
    }

    /// <summary>
    /// An empty file is read as an empty text
    /// </summary>
    [Fact]
    public void EmptyFileIsReadAsEmptyText()
    {
        // Writing and reading an empty file
        string fullPath = WriteFile("sessions/1.json", Array.Empty<byte>());
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe(string.Empty);
    }

    /// <summary>
    /// The accents and the other non-ASCII characters are read as UTF-8
    /// </summary>
    [Fact]
    public void Utf8TextIsReadCorrectly()
    {
        // Writing a text with accents, a dash and Japanese characters
        const string Text = "Michaël – 日本";
        string fullPath = WriteText("agents/helper.md", Text);

        // Reading it
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the content
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe(Text);
    }

    /// <summary>
    /// A UTF-8 byte order mark at the start is not part of the content
    /// </summary>
    [Fact]
    public void Utf8ByteOrderMarkIsNotPartOfTheContent()
    {
        // Writing a text preceded by the byte order mark
        byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
        byte[] body = new UTF8Encoding(false).GetBytes("Michaël");
        string fullPath = WriteFile("sessions/1.json", bom.Concat(body).ToArray());

        // Reading it
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the content
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("Michaël");
    }

    /// <summary>
    /// A file that another handle holds open for writing, with full sharing, can still be read
    /// </summary>
    [Fact]
    public void FileOpenedForWritingByAnotherHandleIsRead()
    {
        // Writing the file and keeping a writer open, as Claude Code may do
        string fullPath = WriteText("projects/C--p/live.jsonl", "line one");
        using FileStream writer = new FileStream(
            fullPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        writer.Flush(true);

        // Reading it
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the content
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("line one");
    }

    /// <summary>
    /// A file marked read-only is read
    /// </summary>
    [Fact]
    public void ReadOnlyFileIsRead()
    {
        // Writing the file and marking it read-only
        string fullPath = WriteText("sessions/1.json", "invented");
        File.SetAttributes(fullPath, FileAttributes.ReadOnly);

        try
        {
            // Reading it
            FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

            // Checking the content
            result.IsSuccess.ShouldBeTrue();
            result.Content.ShouldBe("invented");
        }
        finally
        {
            // Removing the attribute so that the cleanup can delete the file
            File.SetAttributes(fullPath, FileAttributes.Normal);
        }
    }

    /// <summary>
    /// A file locked by another handle that shares nothing is reported as unreadable
    /// </summary>
    [Fact]
    public void ExclusivelyLockedFileIsUnreadable()
    {
        // Skipping where the exclusive lock is not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The exclusive lock of FileShare.None is reliable on Windows only");

        // Writing the file and locking it
        string fullPath = WriteText("sessions/1.json", "invented");
        using FileStream locker = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // Reading it
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the result
        result.IsSuccess.ShouldBeFalse();
        result.Failure.ShouldBe(FileReadFailure.Unreadable);
        result.Content.ShouldBeNull();
        result.Detail.ShouldNotBeNullOrWhiteSpace();
        result.Detail.ShouldContain(nameof(IOException));
    }

    /// <summary>
    /// A file kept open by another handle that may delete it (as Claude Code may when it replaces a file) is still read
    /// </summary>
    [Fact]
    public void FileOpenedWithTheRightToDeleteByAnotherHandleIsRead()
    {
        // Skipping where the sharing modes are not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The sharing modes of a file are enforced on Windows only");

        // Writing the file and opening it with the right to delete it
        string fullPath = WriteText("sessions/1.json", "invented");
        using FileStream deleter = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.DeleteOnClose);

        // Reading it
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(fullPath);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("invented");
    }

    /// <summary>
    /// The file is closed before the reading returns, after a success as after a failure
    /// </summary>
    [Fact]
    public void FileIsClosedWhenTheReadingReturns()
    {
        // Writing a file that is read and a file that is too large
        string readFile = WriteText("sessions/1.json", "invented");
        string largeFile = WriteFile("sessions/2.json", new byte[64]);
        ClaudeFileReader reader = new ClaudeFileReader(_folder, 32);

        // Reading both
        reader.ReadText(readFile).IsSuccess.ShouldBeTrue();
        reader.ReadText(largeFile).Failure.ShouldBe(FileReadFailure.TooLarge);

        // Checking that each file can be opened again without any sharing, which needs every other handle to be closed
        using (FileStream reopened = new FileStream(readFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            reopened.CanWrite.ShouldBeTrue();
        using (FileStream reopened = new FileStream(largeFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            reopened.CanWrite.ShouldBeTrue();
    }

    /// <summary>
    /// A path that the system cannot resolve is refused as not allowed, without an exception
    /// </summary>
    [Fact]
    public void UnresolvablePathIsNotAllowed()
    {
        // Reading a path with a null character
        FileReadResult result = new ClaudeFileReader(_folder).ReadText(Path.Combine(_root, "sessions", "1\0.json"));

        // Checking the result
        result.Failure.ShouldBe(FileReadFailure.NotAllowed);
    }

    /// <summary>
    /// A path longer than any system accepts gives a failure, never a success nor an exception, on every system
    /// </summary>
    [Fact]
    public void PathLongerThanTheSystemAcceptsIsAFailure()
    {
        // Reading a path of 40 000 characters
        string longPath = Path.Combine(_root, "projects", new string('a', 40000), "x.jsonl");
        FileReadResult result = Should.NotThrow(() => new ClaudeFileReader(_folder).ReadText(longPath));

        // Checking the failure
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldBeNull();
    }

    /// <summary>
    /// A file reached through a junction inside the folder is refused: the junction could lead to a forbidden folder
    /// </summary>
    [Fact]
    public void FileThroughAJunctionIsNotAllowed()
    {
        // Skipping where junctions do not exist
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Junctions are a feature of NTFS");

        // Writing a file in the forbidden memory folder, and a junction next to it that leads there
        WriteText("projects/C--p/memory/m.jsonl", "invented-memory");
        string junction = Path.Combine(_root, "projects", "C--p", "link");
        using (Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{junction}\" \"{Path.Combine(_root, "projects", "C--p", "memory")}\"") { CreateNoWindow = true, UseShellExecute = false })!)
            mklink.WaitForExit();
        try
        {
            Directory.Exists(junction).ShouldBeTrue();

            // Reading the file through the junction
            FileReadResult result = new ClaudeFileReader(_folder).ReadText(Path.Combine(junction, "m.jsonl"));

            // Checking that it is refused and that nothing was read
            result.Failure.ShouldBe(FileReadFailure.NotAllowed);
            result.Content.ShouldBeNull();
        }
        finally
        {
            // Removing the junction itself, not what it leads to, so that the cleanup can delete the folder
            if (Directory.Exists(junction))
                Directory.Delete(junction);
        }
    }

    /// <summary>
    /// The size limit must fit in one array: the largest size of an array is refused, the size just below is accepted
    /// </summary>
    [Fact]
    public void MaximumBytesMustFitInOneArray()
    {
        // Building the reader with the largest size of an array
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new ClaudeFileReader(_folder, Array.MaxLength));

        // Checking the parameter in fault, and the size just below
        exception.ParamName.ShouldBe("maximumBytes");
        new ClaudeFileReader(_folder, Array.MaxLength - 1).MaximumBytes.ShouldBe(Array.MaxLength - 1);
    }

    /// <summary>
    /// Method used to read the lines of a file into a list
    /// </summary>
    /// <param name="reader">[Cannot be null] Reader under test</param>
    /// <param name="fullPath">[Cannot be null] Full path of the file</param>
    /// <param name="lines">The lines read, in their order</param>
    /// <returns>The result of the reading</returns>
    private static LineReadResult ReadLines(ClaudeFileReader reader, string fullPath, out List<string> lines)
    {
        // Reading the lines into a new list
        List<string> read = new List<string>();
        LineReadResult result = reader.ReadLines(fullPath, read.Add);
        lines = read;

        // Function's return
        return result;
    }

    /// <summary>
    /// The lines of a transcript are read in their order, without their line endings and without the byte order mark
    /// </summary>
    [Fact]
    public void LinesAreReadInTheirOrderWithoutEndings()
    {
        // Writing a transcript with a byte order mark, mixed line endings and an empty line
        string fullPath = WriteFile("projects/C--p/s.jsonl", new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("{\"a\":1}\r\n{\"b\":2}\n\n{\"c\":\"é\"}\n")).ToArray());

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBe(new List<string>() { "{\"a\":1}", "{\"b\":2}", string.Empty, "{\"c\":\"é\"}" });
        result.LineCount.ShouldBe(4);
        result.TooLongLineCount.ShouldBe(0);
    }

    /// <summary>
    /// A last line without a line feed (Claude Code may be writing it) is given too
    /// </summary>
    [Fact]
    public void LastLineWithoutALineFeedIsGiven()
    {
        // Writing a transcript whose last line is not ended
        string fullPath = WriteText("projects/C--p/s.jsonl", "{\"a\":1}\n{\"b\":");

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBe(new List<string>() { "{\"a\":1}", "{\"b\":" });
    }

    /// <summary>
    /// An empty transcript gives no line and is a success
    /// </summary>
    [Fact]
    public void EmptyTranscriptGivesNoLine()
    {
        // Writing an empty transcript
        string fullPath = WriteFile("projects/C--p/s.jsonl", Array.Empty<byte>());

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBeEmpty();
        result.LineCount.ShouldBe(0);
    }

    /// <summary>
    /// A line longer than the size limit is skipped and counted; the lines around it are read, so a file larger than the
    /// limit can be read
    /// </summary>
    [Fact]
    public void LineLongerThanTheLimitIsSkippedAndCounted()
    {
        // Writing a transcript of 200 bytes whose middle line has 100 bytes, read with a limit of 32 bytes per line
        string fullPath = WriteText("projects/C--p/s.jsonl", "short-1\n" + new string('x', 100) + "\nshort-2\n" + new string('y', 32) + "\n" + new string('z', 33));

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder, 32), fullPath, out List<string> lines);

        // Checking the result: a line of exactly the limit is read, one byte more is skipped
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBe(new List<string>() { "short-1", "short-2", new string('y', 32) });
        result.LineCount.ShouldBe(3);
        result.TooLongLineCount.ShouldBe(2);
    }

    /// <summary>
    /// The lines are cut right at the edges of the blocks read (81 920 bytes): a line across two blocks, a carriage return at
    /// the end of a block with its line feed in the next one, and a line feed at the end of a block
    /// </summary>
    [Fact]
    public void LinesAreCutRightAtTheEdgesOfTheBlocks()
    {
        // Writing a line across two blocks, then a line whose carriage return ends the second block, then a line feed that ends
        // the third block
        const int BlockSize = 81920;
        string acrossBlocks = new string('a', 100000);
        string crAtEdge = new string('b', (2 * BlockSize) - 1 - (acrossBlocks.Length + 1));
        string lfAtEdge = new string('c', (3 * BlockSize) - 1 - ((2 * BlockSize) + 1));
        string fullPath = WriteText("projects/C--p/s.jsonl", acrossBlocks + "\n" + crAtEdge + "\r\n" + lfAtEdge + "\nend\n");

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.Select(line => line.Length).ShouldBe(new List<int>() { acrossBlocks.Length, crAtEdge.Length, lfAtEdge.Length, 3 });
        lines[1].ShouldBe(crAtEdge);
        lines[3].ShouldBe("end");
    }

    /// <summary>
    /// A line longer than the limit across several blocks is skipped whole: no part of it leaks into the next line
    /// </summary>
    [Fact]
    public void LineTooLongAcrossBlocksIsSkippedWhole()
    {
        // Writing a line of 200 000 bytes, then a short line, read with a limit of 100 000 bytes per line
        string fullPath = WriteText("projects/C--p/s.jsonl", new string('x', 200000) + "\nshort\n");

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder, 100000), fullPath, out List<string> lines);

        // Checking the result
        lines.ShouldBe(new List<string>() { "short" });
        result.TooLongLineCount.ShouldBe(1);
    }

    /// <summary>
    /// Only the byte order mark at the start of the file is removed: one at the start of another line is kept
    /// </summary>
    [Fact]
    public void ByteOrderMarkOfAnotherLineIsKept()
    {
        // Writing a byte order mark at the start of the second line
        byte[] mark = new UTF8Encoding(true).GetPreamble();
        string fullPath = WriteFile("projects/C--p/s.jsonl", Encoding.UTF8.GetBytes("a\n").Concat(mark).Concat(Encoding.UTF8.GetBytes("b\n")).ToArray());

        // Reading the lines
        ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking that the mark of the second line is kept
        lines.ShouldBe(new List<string>() { "a", "﻿b" });
    }

    /// <summary>
    /// Invalid bytes become the replacement character instead of failing
    /// </summary>
    [Fact]
    public void InvalidBytesOfALineBecomeTheReplacementCharacter()
    {
        // Writing a line with a byte that is not valid UTF-8
        string fullPath = WriteFile("projects/C--p/s.jsonl", new byte[] { (byte)'a', 0xFF, (byte)'b', (byte)'\n' });

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBe(new List<string>() { "a�b" });
    }

    /// <summary>
    /// The lines of a file outside the allow-list are never read
    /// </summary>
    [Fact]
    public void LinesOfAForbiddenFileAreNotRead()
    {
        // Writing a transcript in the forbidden memory folder
        string fullPath = WriteText("projects/C--p/memory/m.jsonl", "invented-memory\n");

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking that nothing was read
        result.IsSuccess.ShouldBeFalse();
        result.Failure.ShouldBe(FileReadFailure.NotAllowed);
        lines.ShouldBeEmpty();
    }

    /// <summary>
    /// The lines of a missing file give a failure, not an exception
    /// </summary>
    [Fact]
    public void LinesOfAMissingFileAreNotFound()
    {
        // Reading a missing transcript
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), Path.Combine(_root, "projects", "C--p", "missing.jsonl"), out List<string> lines);

        // Checking the failure
        result.Failure.ShouldBe(FileReadFailure.NotFound);
        result.Detail.ShouldNotBeNullOrWhiteSpace();
        lines.ShouldBeEmpty();
    }

    /// <summary>
    /// The lines of a file reached through a junction are never read
    /// </summary>
    [Fact]
    public void LinesThroughAJunctionAreNotRead()
    {
        // Skipping where junctions do not exist
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Junctions are a feature of NTFS");

        // Writing a transcript in the forbidden memory folder, and a junction next to it that leads there
        WriteText("projects/C--p/memory/m.jsonl", "invented-memory\n");
        string junction = Path.Combine(_root, "projects", "C--p", "link");
        using (Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{junction}\" \"{Path.Combine(_root, "projects", "C--p", "memory")}\"") { CreateNoWindow = true, UseShellExecute = false })!)
        {
            mklink.WaitForExit();
            mklink.ExitCode.ShouldBe(0);
        }
        try
        {
            // Reading the lines through the junction
            LineReadResult result = ReadLines(new ClaudeFileReader(_folder), Path.Combine(junction, "m.jsonl"), out List<string> lines);

            // Checking that nothing was read
            result.Failure.ShouldBe(FileReadFailure.NotAllowed);
            lines.ShouldBeEmpty();
        }
        finally
        {
            // Removing the junction itself, not what it leads to
            if (Directory.Exists(junction))
                Directory.Delete(junction);
        }
    }

    /// <summary>
    /// A transcript that Claude Code keeps open for writing is still read
    /// </summary>
    [Fact]
    public void LinesOfAFileOpenForWritingAreRead()
    {
        // Writing a transcript and keeping it open for writing, as Claude Code does
        string fullPath = WriteText("projects/C--p/s.jsonl", "{\"a\":1}\n");
        using FileStream writer = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

        // Reading the lines
        LineReadResult result = ReadLines(new ClaudeFileReader(_folder), fullPath, out List<string> lines);

        // Checking the result
        result.IsSuccess.ShouldBeTrue();
        lines.ShouldBe(new List<string>() { "{\"a\":1}" });
    }

    /// <summary>
    /// The arguments of the reading of the lines are required
    /// </summary>
    [Fact]
    public void ArgumentsOfTheReadingOfTheLinesAreRequired()
    {
        // Reading without each argument
        ClaudeFileReader reader = new ClaudeFileReader(_folder);
        ArgumentNullException withoutPath = Should.Throw<ArgumentNullException>(() => reader.ReadLines(" ", _ => { }));
        ArgumentNullException withoutAction = Should.Throw<ArgumentNullException>(() => reader.ReadLines(Path.Combine(_root, "projects", "C--p", "s.jsonl"), null!));

        // Checking the parameters in fault
        withoutPath.ParamName.ShouldBe("fullPath");
        withoutAction.ParamName.ShouldBe("onLine");
    }

    /// <summary>
    /// The Reader writes nothing: files, folders, attributes, dates, sizes and contents stay as they were
    /// </summary>
    [Fact]
    public void ReaderWritesNothing()
    {
        // Preparing a tree with files that succeed and files that fail
        string good = WriteText("sessions/1.json", "invented");
        string goodAgent = WriteText("agents/helper.md", "invented");
        string forbidden = WriteText(".credentials.json", "invented-secret");
        string tooLarge = WriteFile("projects/C--p/big.jsonl", new byte[64]);
        Directory.CreateDirectory(Path.Combine(_root, "sessions", "folder.json"));
        File.SetLastWriteTimeUtc(good, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(goodAgent, new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc));
        List<string> before = Snapshot();
        ClaudeFileReader reader = new ClaudeFileReader(_folder, 32);

        // Reading several times, with every kind of outcome
        for (int turn = 0; turn < 3; turn++)
        {
            reader.ReadText(good);
            reader.ReadText(goodAgent);
            reader.ReadText(forbidden);
            reader.ReadText(tooLarge);
            reader.ReadText(Path.Combine(_root, "sessions", "folder.json"));
            reader.ReadText(Path.Combine(_root, "sessions", "missing.json"));
            reader.ReadText(Path.Combine(_root, "nothing", "missing.json"));
        }

        // Checking that nothing changed
        Snapshot().ShouldBe(before);
        Directory.Exists(Path.Combine(_root, "nothing")).ShouldBeFalse();
    }

    /// <summary>
    /// Method used to delete the invented folder of the instance
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> leaves no file behind in the temporary folder of the system.</para>
    /// <para><b>Behavior:</b> removes the read-only attribute of the files first, so that the deletion cannot be refused.</para>
    /// </remarks>
    public void Dispose()
    {
        // Removing the read-only attributes
        if (Directory.Exists(_workFolder))
        {
            foreach (string file in Directory.EnumerateFiles(_workFolder, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);

            // Deleting the folder
            Directory.Delete(_workFolder, true);
        }
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
