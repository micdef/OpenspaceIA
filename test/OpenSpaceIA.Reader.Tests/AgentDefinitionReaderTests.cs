/*
 *	@file	test/OpenSpaceIA.Reader.Tests/AgentDefinitionReaderTests.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Tests of the reading of the agent definitions of Claude Code
 */

// Using
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader.Tests;

/// <summary>
/// Tests of the reading of the agent definitions of Claude Code
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> proves that every definition of the agents folder is read, that a bad file never stops the reading
/// of the others, and that every file left out for a problem is reported.</para>
/// <para><b>Behavior:</b> each instance of the class works in its own invented folder under the temporary folder of the
/// system, and deletes it in <see cref="Dispose"/>. Invented data only; no state shared between tests.</para>
/// </remarks>
public sealed class AgentDefinitionReaderTests : IDisposable
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
    /// Reader of the files of the invented .claude folder
    /// </summary>
    private readonly ClaudeFileReader _fileReader;

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
    public AgentDefinitionReaderTests()
    {
        // Creating the folders
        _workFolder = Path.Combine(Path.GetTempPath(), "osia-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(_workFolder, ".claude");
        Directory.CreateDirectory(_root);

        // Building the reader of the files
        _fileReader = new ClaudeFileReader(new ClaudeFolder(_root));
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to write a text file in UTF-8 without BOM under the invented root, creating its folders
    /// </summary>
    /// <param name="relativePath">[Cannot be null] Path of the file, relative to the root</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <returns>The full path of the file</returns>
    private string WriteText(string relativePath, string text)
    {
        // Creating the folders and the file
        string fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new UTF8Encoding(false).GetBytes(text));

        // Function's return
        return fullPath;
    }

    /// <summary>
    /// Method used to write a valid agent definition in the agents folder
    /// </summary>
    /// <param name="fileName">[Cannot be null] Name of the file</param>
    /// <param name="name">[Cannot be null] Name of the agent</param>
    /// <returns>The full path of the file</returns>
    private string WriteDefinition(string fileName, string name)
    {
        // Function's return
        return WriteText(Path.Combine("agents", fileName), $"---\nname: {name}\ndescription: Invented {name}.\nmodel: sonnet\n---\nInvented prompt.\n");
    }

    /// <summary>
    /// Method used to list the names of the agent types read, in their order
    /// </summary>
    /// <param name="definitions">[Cannot be null] Result of the reading</param>
    /// <returns>The names, separated by a comma and a space</returns>
    private static string Names(AgentDefinitions definitions)
    {
        // Function's return
        return string.Join(", ", definitions.AgentTypes.Select(agentType => agentType.Name));
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
    /// The reader of the files is required
    /// </summary>
    [Fact]
    public void FileReaderCannotBeNull()
    {
        // Building the reader without reader of the files
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => new AgentDefinitionReader(null!));

        // Checking the parameter in fault
        exception.ParamName.ShouldBe("fileReader");
    }

    /// <summary>
    /// Without agents folder, there is no agent and no problem: a user may have no agent of their own
    /// </summary>
    [Fact]
    public void MissingAgentsFolderGivesNothing()
    {
        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        definitions.AgentTypes.ShouldBeEmpty();
        definitions.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// Every definition is read, in the ordinal order of the file names
    /// </summary>
    [Fact]
    public void EveryDefinitionIsReadInTheOrderOfTheFileNames()
    {
        // Writing the definitions, out of order, with file names that differ from the names of the agents
        WriteDefinition("b.md", "beta");
        WriteDefinition("a.md", "alpha");
        WriteDefinition("C.md", "gamma");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result: "C" comes before "a" in the ordinal order
        Names(definitions).ShouldBe("gamma, alpha, beta");
        definitions.AgentTypes[1].ShouldBe(new AgentType("alpha", "Invented alpha.", "sonnet"));
        definitions.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A file that does not end with .md (a backup, a note) is not a definition: it is left out without a problem
    /// </summary>
    [Fact]
    public void FilesThatAreNotDefinitionsAreLeftOutSilently()
    {
        // Writing a definition, a backup and a note
        WriteDefinition("helper.md", "helper");
        WriteDefinition("helper.md.bak-2026-10-29", "old-helper");
        WriteText("agents/notes.txt", "invented");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("helper");
        definitions.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// A file whose extension is .md in another case is not read, and it is reported so that it is not lost silently
    /// </summary>
    [Fact]
    public void ExtensionInAnotherCaseIsReported()
    {
        // Writing a definition and one with an extension in uppercase
        WriteDefinition("helper.md", "helper");
        string upper = WriteDefinition("UPPER.MD", "upper");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("helper");
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(upper);
        definitions.Problems[0].Reason.ShouldContain("lowercase");
    }

    /// <summary>
    /// A definition that starts with the byte order mark of UTF-8 is read: the mark is not part of the text
    /// </summary>
    [Fact]
    public void DefinitionWithAByteOrderMarkIsRead()
    {
        // Writing a definition with a byte order mark
        string fullPath = Path.Combine(_root, "agents", "helper.md");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("---\nname: helper\n---\n")).ToArray());

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("helper");
        definitions.Problems.ShouldBeEmpty();
    }

    /// <summary>
    /// An agents entry that is a file is reported, on every system, instead of being taken for a missing folder
    /// </summary>
    [Fact]
    public void AgentsEntryThatIsAFileIsReported()
    {
        // Writing a file at the place of the agents folder
        string agents = WriteText("agents", "invented");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        definitions.AgentTypes.ShouldBeEmpty();
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(agents);
        definitions.Problems[0].Reason.ShouldContain("cannot be listed: it is not a folder");
    }

    /// <summary>
    /// An agents link whose target is missing is reported, instead of being taken for a missing folder
    /// </summary>
    [Fact]
    public void AgentsLinkToAMissingTargetIsReported()
    {
        // Creating a link to a folder that does not exist: a junction on Windows (a symbolic link needs a privilege there)
        string agents = Path.Combine(_root, "agents");
        string missing = Path.Combine(_workFolder, "missing");
        if (OperatingSystem.IsWindows())
        {
            using Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{agents}\" \"{missing}\"") { CreateNoWindow = true, UseShellExecute = false })!;
            mklink.WaitForExit();
            mklink.ExitCode.ShouldBe(0);
        }
        else
        {
            Directory.CreateSymbolicLink(agents, missing);
        }
        try
        {
            // Reading the definitions
            AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

            // Checking the result
            definitions.AgentTypes.ShouldBeEmpty();
            definitions.Problems.Count.ShouldBe(1);
            definitions.Problems[0].FilePath.ShouldBe(agents);
            definitions.Problems[0].Reason.ShouldContain("cannot be listed");
            definitions.Problems[0].Reason.ShouldContain("link to a missing target");
        }
        finally
        {
            // Removing the link itself
            new DirectoryInfo(agents).Delete();
        }
    }

    /// <summary>
    /// A subfolder of the agents folder is not read, and it is reported so that its definitions are not lost silently
    /// </summary>
    [Fact]
    public void SubfolderIsReportedAndNotRead()
    {
        // Writing a definition at the top and one in a subfolder
        WriteDefinition("helper.md", "helper");
        WriteDefinition(Path.Combine("review", "reviewer.md"), "reviewer");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("helper");
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(Path.Combine(_root, "agents", "review"));
        definitions.Problems[0].Reason.ShouldContain("subfolder");
    }

    /// <summary>
    /// A broken definition is reported and does not stop the reading of the others
    /// </summary>
    [Fact]
    public void BrokenDefinitionDoesNotStopTheOthers()
    {
        // Writing a broken definition between two good ones
        WriteDefinition("a.md", "alpha");
        string broken = WriteText(Path.Combine("agents", "b.md"), "No front matter here.");
        WriteDefinition("c.md", "gamma");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("alpha, gamma");
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(broken);
    }

    /// <summary>
    /// A definition that cannot be read (here, too large) is reported with the reason of the failure
    /// </summary>
    [Fact]
    public void UnreadableDefinitionIsReportedWithItsReason()
    {
        // Writing a definition larger than the limit of a small reader, and a short one
        string large = WriteText(Path.Combine("agents", "large.md"), "---\nname: large\ndescription: " + new string('x', 200) + "\n---\n");
        WriteText("agents/small.md", "---\nname: small\n---\n");
        ClaudeFileReader smallReader = new ClaudeFileReader(new ClaudeFolder(_root), 64);

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(smallReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("small");
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(large);
        definitions.Problems[0].Reason.ShouldContain(nameof(FileReadFailure.TooLarge));
    }

    /// <summary>
    /// A definition locked by another program is reported, and the others are still read
    /// </summary>
    [Fact]
    public void LockedDefinitionIsReported()
    {
        // Skipping where the exclusive lock is not enforced
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The exclusive lock of FileShare.None is reliable on Windows only");

        // Writing two definitions and locking one
        string locked = WriteDefinition("a.md", "alpha");
        WriteDefinition("b.md", "beta");
        using FileStream locker = new FileStream(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        Names(definitions).ShouldBe("beta");
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(locked);
        definitions.Problems[0].Reason.ShouldContain(nameof(FileReadFailure.Unreadable));
    }

    /// <summary>
    /// Two definitions with the same name: the first in the order of the file names is kept, the other is reported
    /// </summary>
    [Fact]
    public void SecondDefinitionWithTheSameNameIsReported()
    {
        // Writing two definitions of the same agent
        string first = WriteDefinition("a.md", "helper");
        string second = WriteDefinition("b.md", "helper");

        // Reading the definitions
        AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

        // Checking the result
        definitions.AgentTypes.Count.ShouldBe(1);
        definitions.Problems.Count.ShouldBe(1);
        definitions.Problems[0].FilePath.ShouldBe(second);
        definitions.Problems[0].Reason.ShouldContain(first);
    }

    /// <summary>
    /// An agents folder that is a junction is refused file by file: nothing is read through it
    /// </summary>
    [Fact]
    public void AgentsFolderThroughAJunctionIsNotRead()
    {
        // Skipping where junctions do not exist
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Junctions are a feature of NTFS");

        // Writing a definition outside the folder, and an agents junction that leads to it
        string outside = Path.Combine(_workFolder, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "helper.md"), "---\nname: helper\n---\n");
        string junction = Path.Combine(_root, "agents");
        using (Process mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{junction}\" \"{outside}\"") { CreateNoWindow = true, UseShellExecute = false })!)
        {
            mklink.WaitForExit();
            mklink.ExitCode.ShouldBe(0);
        }
        try
        {
            Directory.Exists(junction).ShouldBeTrue();

            // Reading the definitions
            AgentDefinitions definitions = new AgentDefinitionReader(_fileReader).Read();

            // Checking that nothing was read, and that the refusal is reported
            definitions.AgentTypes.ShouldBeEmpty();
            definitions.Problems.Count.ShouldBe(1);
            definitions.Problems[0].Reason.ShouldContain(nameof(FileReadFailure.NotAllowed));
        }
        finally
        {
            // Removing the junction itself, not what it leads to, so that the cleanup can delete the folder
            if (Directory.Exists(junction))
                Directory.Delete(junction);
        }
    }

    /// <summary>
    /// The reader writes nothing, even in a folder whose files are read-only
    /// </summary>
    [Fact]
    public void ReaderWritesNothing()
    {
        // Preparing read-only definitions, a broken one, a backup and a subfolder
        string good = WriteDefinition("a.md", "alpha");
        string broken = WriteText("agents/b.md", "invented");
        WriteDefinition("a.md.bak", "old");
        WriteDefinition(Path.Combine("sub", "c.md"), "gamma");
        File.SetAttributes(good, FileAttributes.ReadOnly);
        File.SetAttributes(broken, FileAttributes.ReadOnly);
        List<string> before = Snapshot();
        AgentDefinitionReader reader = new AgentDefinitionReader(_fileReader);

        // Reading several times
        for (int turn = 0; turn < 3; turn++)
            reader.Read().AgentTypes.Count.ShouldBe(1);

        // Checking that nothing changed
        Snapshot().ShouldBe(before);
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
