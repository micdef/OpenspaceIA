/*
 *	@file	src/OpenSpaceIA.Reader/ClaudeFileReader.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Safe reading of a small text file of Claude Code
 */

// Using
using System.Text;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read a small text file of Claude Code, safely
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the reader must never disturb Claude Code, never write in its folder, and never stop on a bad file
/// (doc/analyse/roadmap.md, 0.4.0). This is the only place where a file of Claude Code is opened.</para>
/// <para><b>Behavior:</b> a file outside the allow-list of <see cref="ClaudeFolder"/> is refused before any access to the
/// disk, and so is a file reached through a junction or a symbolic link below the folder (the folder itself may be one). An
/// allowed file is opened read-only, with full sharing, so that Claude Code can keep writing, renaming or deleting it; its
/// size is limited, and its text is read as UTF-8 (a byte order mark is removed, invalid bytes become the replacement
/// character). A missing, refused, too large or locked file gives a failed result with its reason, the type of the exception,
/// its code and the message of the system; an error of programming (an exception of another kind) is not caught. The links
/// are checked just before the file is opened: a link created between the check and the opening is not seen. Two known
/// limits: a hard link is not a link for the system, so a hard link inside the folder to a forbidden file is read (creating
/// one needs the right to write in the folder, which already gives its secrets); and every reparse point is refused, so a
/// folder kept online only by a cloud service would be refused too (not verified). Large transcripts are not read by this
/// class.</para>
/// </remarks>
internal sealed class ClaudeFileReader
{
    #region Members

    #region Constants

    /// <summary>
    /// Default size limit of a file read in one go: 1 MiB, far above a session file or an agent definition
    /// </summary>
    public const long DefaultMaximumBytes = 1048576;

    /// <summary>
    /// Size of the blocks read from the file
    /// </summary>
    private const int BlockSize = 81920;

    #endregion

    #region Variables

    /// <summary>
    /// The byte order mark of UTF-8, which is not part of the text
    /// </summary>
    private static readonly byte[] _utf8ByteOrderMark = new byte[] { 0xEF, 0xBB, 0xBF };

    /// <summary>
    /// UTF-8 without byte order mark, that replaces invalid bytes instead of failing
    /// </summary>
    private static readonly UTF8Encoding _utf8 = new UTF8Encoding(false, false);

    /// <summary>
    /// Folder of Claude Code, with its allow-list
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

    /// <summary>
    /// Size limit of a file, in bytes (the byte order mark included); a larger file is refused
    /// </summary>
    public long MaximumBytes { get; }

    /// <summary>
    /// Folder of Claude Code read by this reader, with its allow-list
    /// </summary>
    public ClaudeFolder Folder => _folder;

    #endregion

    #region Delegates

    // #N/A

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor, with the default size limit
    /// </summary>
    /// <param name="folder">[Cannot be null] Folder of Claude Code, with its allow-list</param>
    /// <exception cref="ArgumentNullException">If the folder is null</exception>
    public ClaudeFileReader(ClaudeFolder folder)
        : this(folder, DefaultMaximumBytes)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="folder">[Cannot be null] Folder of Claude Code, with its allow-list</param>
    /// <param name="maximumBytes">Size limit of a file, in bytes; from 1 to the largest size of an array</param>
    /// <exception cref="ArgumentNullException">If the folder is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the size limit is less than 1, or too large for one array</exception>
    public ClaudeFileReader(ClaudeFolder folder, long maximumBytes)
    {
        // Checks
        if (folder is null)
            throw new ArgumentNullException(nameof(folder), $"The value of the parameter: {nameof(folder)} cannot be null");
        if (maximumBytes < 1 || maximumBytes >= Array.MaxLength)
            throw new ArgumentOutOfRangeException(nameof(maximumBytes), maximumBytes, $"The value of the parameter: {nameof(maximumBytes)} must be from 1 to {Array.MaxLength - 1}");

        // Setting the values
        _folder = folder;
        MaximumBytes = maximumBytes;
    }

    #endregion

    #region Destructors

    // #N/A

    #endregion

    #region Methods

    #region Privates

    /// <summary>
    /// Method used to read the bytes of an open file, up to one byte more than the size limit
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> knows that a file is too large without reading it all, even when it grows while it is read.</para>
    /// <para><b>Behavior:</b> reads block by block until the end of the file, or until one byte more than the limit is read;
    /// the memory used is never more than the limit plus one byte, plus one block.</para>
    /// </remarks>
    /// <param name="stream">[Cannot be null] File open for reading</param>
    /// <returns>The bytes read: at most the size limit plus one</returns>
    /// <exception cref="IOException">If the disk fails while the file is read (caught by the caller)</exception>
    private byte[] ReadAtMostLimitPlusOne(Stream stream)
    {
        // Reading block by block until the end of the file, or one byte past the limit
        long limit = MaximumBytes + 1;
        using MemoryStream content = new MemoryStream();
        byte[] block = new byte[BlockSize];
        while (content.Length < limit)
        {
            int read = stream.Read(block, 0, (int)Math.Min(BlockSize, limit - content.Length));
            if (read == 0)
                break;

            content.Write(block, 0, read);
        }

        // Function's return
        return content.ToArray();
    }

    /// <summary>
    /// Method used to know whether a file is reached through a junction or a symbolic link below the folder
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> the allow-list reads names only; a link inside the folder could lead to a forbidden folder
    /// (memory, backups) or out of the folder.</para>
    /// <para><b>Behavior:</b> looks at each folder below the folder of Claude Code, then at the file itself; the folder of
    /// Claude Code may be a link, since the user chose it.</para>
    /// </remarks>
    /// <param name="fullPath">[Cannot be null] Full path of a file already in the allow-list</param>
    /// <returns>
    /// True  ==> A folder below the folder of Claude Code, or the file, is a junction or a symbolic link<br/>
    /// False ==> The file is reached without any link
    /// </returns>
    /// <exception cref="FileNotFoundException">If the file does not exist (caught by the caller)</exception>
    /// <exception cref="DirectoryNotFoundException">If one of its folders does not exist (caught by the caller)</exception>
    /// <exception cref="UnauthorizedAccessException">If the system refuses to show the attributes (caught by the caller)</exception>
    private bool IsReachedThroughALink(string fullPath)
    {
        // Looking at each level below the folder of Claude Code, down to the file
        string current = _folder.RootPath;
        string relativePath = Path.GetRelativePath(_folder.RootPath, Path.GetFullPath(fullPath));
        foreach (string segment in relativePath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                // Function's return
                return true;
            }
        }

        // Function's return
        return false;
    }

    /// <summary>
    /// Method used to describe an exception of the system for a failed result
    /// </summary>
    /// <param name="exception">[Cannot be null] Exception raised while the file was opened or read</param>
    /// <returns>The type of the exception, its code and its message, so that a lock can be told from another failure</returns>
    private static string Describe(Exception exception)
    {
        // Function's return
        return $"{exception.GetType().Name} (0x{exception.HResult:X8}): {exception.Message}";
    }

    /// <summary>
    /// Method used to turn the bytes of a file into text
    /// </summary>
    /// <param name="bytes">[Cannot be null] Bytes of the file</param>
    /// <returns>The text, without the byte order mark of UTF-8</returns>
    private static string DecodeUtf8(byte[] bytes)
    {
        // Leaving out the byte order mark
        ReadOnlySpan<byte> text = bytes;
        if (text.StartsWith(_utf8ByteOrderMark))
            text = text.Slice(_utf8ByteOrderMark.Length);

        // Function's return
        return _utf8.GetString(text);
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read a small text file of Claude Code
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> reads a session file or an agent definition without ever disturbing Claude Code.</para>
    /// <para><b>Behavior:</b> a file outside the allow-list is refused without touching the disk. An allowed file is checked
    /// for links, opened read-only with full sharing and closed before the method returns; nothing is ever written.</para>
    /// </remarks>
    /// <param name="fullPath">[Cannot be null] Full path of the file</param>
    /// <returns>
    /// Success      ==> The text of the file<br/>
    /// NotAllowed   ==> The file is not in the allow-list, or it is reached through a junction or a symbolic link<br/>
    /// NotFound     ==> The file or one of its folders does not exist<br/>
    /// AccessDenied ==> The system refused the access, or the path is a folder<br/>
    /// TooLarge     ==> The file is larger than the size limit<br/>
    /// Unreadable   ==> The file is locked by another program, or the disk failed
    /// </returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces</exception>
    public FileReadResult ReadText(string fullPath)
    {
        // Check
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentNullException(nameof(fullPath), $"The value of the parameter: {nameof(fullPath)} cannot be null or empty");

        // Refusing a file outside the allow-list, before any access to the disk
        if (!_folder.IsReadable(fullPath))
        {
            // Function's return
            return FileReadResult.Failed(FileReadFailure.NotAllowed, $"The file is not in the allow-list of the reader: {fullPath}");
        }

        try
        {
            // Refusing a file reached through a junction or a symbolic link, which could lead to a forbidden place
            if (IsReachedThroughALink(fullPath))
            {
                // Function's return
                return FileReadResult.Failed(FileReadFailure.NotAllowed, $"The file is reached through a junction or a symbolic link: {fullPath}");
            }

            // Opening the file read-only, letting Claude Code read, write, rename and delete it meanwhile
            using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            byte[] bytes = ReadAtMostLimitPlusOne(stream);

            // Refusing a file larger than the limit
            if (bytes.Length > MaximumBytes)
            {
                // Function's return
                return FileReadResult.Failed(FileReadFailure.TooLarge, $"The file is larger than {MaximumBytes} bytes: {fullPath}");
            }

            // Function's return
            return FileReadResult.Success(DecodeUtf8(bytes));
        }
        catch (FileNotFoundException exception)
        {
            // Function's return
            return FileReadResult.Failed(FileReadFailure.NotFound, Describe(exception));
        }
        catch (DirectoryNotFoundException exception)
        {
            // Function's return
            return FileReadResult.Failed(FileReadFailure.NotFound, Describe(exception));
        }
        catch (UnauthorizedAccessException exception)
        {
            // Function's return
            return FileReadResult.Failed(FileReadFailure.AccessDenied, Describe(exception));
        }
        catch (IOException exception)
        {
            // Function's return: a locked file, or an error of the disk
            return FileReadResult.Failed(FileReadFailure.Unreadable, Describe(exception));
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
