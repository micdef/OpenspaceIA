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
/// folder kept online only by a cloud service would be refused too (not verified). A small file is read in one go
/// (<see cref="ReadText"/>, limited in size); a transcript is read line by line (<see cref="ReadLines"/>, each line limited
/// in size).</para>
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
    /// Size limit, in bytes: of a file read in one go (the byte order mark included), and of each line of a file read line by
    /// line (without its line feed); a larger file is refused, a longer line is skipped
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
    /// Method used to split an open file into lines, given one by one, without ever holding more than one line in memory
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> reads a transcript of hundreds of megabytes with the memory of one line.</para>
    /// <para><b>Behavior:</b> a line ends with a line feed; a last line without one is given too (Claude Code may be writing
    /// it). A line whose bytes (without its line feed) are more than the size limit is skipped and counted, and its bytes are
    /// never kept beyond the limit. Each line given is decoded by <see cref="DecodeLine"/>.</para>
    /// </remarks>
    /// <param name="stream">[Cannot be null] File open for reading</param>
    /// <param name="onLine">[Cannot be null] Action given each line, in the order of the file</param>
    /// <param name="lineCount">Number of lines given</param>
    /// <param name="tooLongLineCount">Number of lines skipped because they were too long</param>
    /// <exception cref="IOException">If the disk fails while the file is read (caught by the caller)</exception>
    private void SplitIntoLines(Stream stream, Action<string> onLine, out int lineCount, out int tooLongLineCount)
    {
        // Reading block by block, and cutting each block at its line feeds
        lineCount = 0;
        tooLongLineCount = 0;
        using MemoryStream line = new MemoryStream();
        bool isTooLong = false;
        bool isFirstLine = true;
        byte[] block = new byte[BlockSize];
        int read;
        while ((read = stream.Read(block, 0, BlockSize)) > 0)
        {
            int start = 0;
            int end;
            while ((end = Array.IndexOf(block, (byte)'\n', start, read - start)) >= 0)
            {
                // Ending the line at its line feed, then starting the next one
                isTooLong = AppendToLine(line, block, start, end - start, isTooLong);
                EndLine(line, isTooLong, isFirstLine, onLine, ref lineCount, ref tooLongLineCount);
                isTooLong = false;
                isFirstLine = false;
                start = end + 1;
            }

            // Keeping the start of a line that goes on in the next block
            isTooLong = AppendToLine(line, block, start, read - start, isTooLong);
        }

        // Giving the last line, which has no line feed
        if (line.Length > 0 || isTooLong)
            EndLine(line, isTooLong, isFirstLine, onLine, ref lineCount, ref tooLongLineCount);
    }

    /// <summary>
    /// Method used to add bytes to the line being read, unless the line is already longer than the size limit
    /// </summary>
    /// <param name="line">[Cannot be null] Bytes of the line being read</param>
    /// <param name="block">[Cannot be null] Block read from the file</param>
    /// <param name="offset">Index of the first byte to add</param>
    /// <param name="count">Number of bytes to add</param>
    /// <param name="isTooLong">True when the line is already longer than the limit</param>
    /// <returns>True when the line is now longer than the limit; its bytes are then dropped</returns>
    private bool AppendToLine(MemoryStream line, byte[] block, int offset, int count, bool isTooLong)
    {
        // Dropping the bytes of a line that is longer than the limit
        if (isTooLong || line.Length + count > MaximumBytes)
        {
            line.SetLength(0);

            // Function's return
            return true;
        }

        // Adding the bytes
        line.Write(block, offset, count);

        // Function's return
        return false;
    }

    /// <summary>
    /// Method used to end the line being read: it is given, or counted as too long, then the line is emptied
    /// </summary>
    /// <param name="line">[Cannot be null] Bytes of the line being read</param>
    /// <param name="isTooLong">True when the line was longer than the limit</param>
    /// <param name="isFirstLine">True for the first line of the file, which may start with a byte order mark</param>
    /// <param name="onLine">[Cannot be null] Action given each line</param>
    /// <param name="lineCount">Number of lines given, increased when the line is given</param>
    /// <param name="tooLongLineCount">Number of lines skipped, increased when the line is too long</param>
    private static void EndLine(MemoryStream line, bool isTooLong, bool isFirstLine, Action<string> onLine, ref int lineCount, ref int tooLongLineCount)
    {
        // Counting a line that was too long, or giving the line
        if (isTooLong)
        {
            tooLongLineCount++;
        }
        else
        {
            onLine(DecodeLine(line.GetBuffer().AsSpan(0, (int)line.Length), isFirstLine));
            lineCount++;
        }

        // Emptying the line
        line.SetLength(0);
    }

    /// <summary>
    /// Method used to turn the bytes of a line into text
    /// </summary>
    /// <param name="bytes">Bytes of the line, without its line feed</param>
    /// <param name="isFirstLine">True for the first line of the file, whose byte order mark is removed</param>
    /// <returns>The text of the line, without its carriage return</returns>
    private static string DecodeLine(ReadOnlySpan<byte> bytes, bool isFirstLine)
    {
        // Leaving out the byte order mark of the file and the carriage return of the line
        if (isFirstLine && bytes.StartsWith(_utf8ByteOrderMark))
            bytes = bytes.Slice(_utf8ByteOrderMark.Length);
        if (bytes.Length > 0 && bytes[^1] == (byte)'\r')
            bytes = bytes.Slice(0, bytes.Length - 1);

        // Function's return
        return _utf8.GetString(bytes);
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

    /// <summary>
    /// Method used to read the lines of a file of Claude Code, one by one, whatever the size of the file
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> reads a transcript (up to hundreds of megabytes) without ever holding it in memory and without
    /// ever disturbing Claude Code.</para>
    /// <para><b>Behavior:</b> the same gates as <see cref="ReadText"/>: a file outside the allow-list is refused without
    /// touching the disk, a file reached through a link is refused, and an allowed file is opened read-only with full sharing.
    /// Here the size limit applies to each line, not to the file: a longer line is skipped and counted. Each line is given
    /// to the action as soon as it is read, without its line feed and its carriage return, decoded as UTF-8 (a byte order mark
    /// at the start of the file is removed, invalid bytes become the replacement character). The limit counts every byte of
    /// a line but its line feed, its carriage return included. An <see cref="IOException"/> thrown by the action is reported
    /// as a failure of the file, as if the disk had failed; any other exception of the action goes out.</para>
    /// </remarks>
    /// <param name="fullPath">[Cannot be null] Full path of the file</param>
    /// <param name="onLine">[Cannot be null] Action given each line, in the order of the file</param>
    /// <returns>
    /// Success      ==> The counts of the lines given and skipped<br/>
    /// NotAllowed   ==> The file is not in the allow-list, or it is reached through a junction or a symbolic link<br/>
    /// NotFound     ==> The file or one of its folders does not exist<br/>
    /// AccessDenied ==> The system refused the access, or the path is a folder<br/>
    /// Unreadable   ==> The file is locked by another program, or the disk failed (some lines may have been given)
    /// </returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces, or if the action is null</exception>
    public LineReadResult ReadLines(string fullPath, Action<string> onLine)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentNullException(nameof(fullPath), $"The value of the parameter: {nameof(fullPath)} cannot be null or empty");
        if (onLine is null)
            throw new ArgumentNullException(nameof(onLine), $"The value of the parameter: {nameof(onLine)} cannot be null");

        // Refusing a file outside the allow-list, before any access to the disk
        if (!_folder.IsReadable(fullPath))
        {
            // Function's return
            return LineReadResult.Failed(FileReadFailure.NotAllowed, $"The file is not in the allow-list of the reader: {fullPath}");
        }

        try
        {
            // Refusing a file reached through a junction or a symbolic link, which could lead to a forbidden place
            if (IsReachedThroughALink(fullPath))
            {
                // Function's return
                return LineReadResult.Failed(FileReadFailure.NotAllowed, $"The file is reached through a junction or a symbolic link: {fullPath}");
            }

            // Opening the file read-only, letting Claude Code read, write, rename and delete it meanwhile, and giving its lines
            using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            SplitIntoLines(stream, onLine, out int lineCount, out int tooLongLineCount);

            // Function's return
            return LineReadResult.Success(lineCount, tooLongLineCount);
        }
        catch (FileNotFoundException exception)
        {
            // Function's return
            return LineReadResult.Failed(FileReadFailure.NotFound, Describe(exception));
        }
        catch (DirectoryNotFoundException exception)
        {
            // Function's return
            return LineReadResult.Failed(FileReadFailure.NotFound, Describe(exception));
        }
        catch (UnauthorizedAccessException exception)
        {
            // Function's return
            return LineReadResult.Failed(FileReadFailure.AccessDenied, Describe(exception));
        }
        catch (IOException exception)
        {
            // Function's return: a locked file, or an error of the disk
            return LineReadResult.Failed(FileReadFailure.Unreadable, Describe(exception));
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
