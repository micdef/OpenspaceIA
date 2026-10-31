/*
 *	@file	src/OpenSpaceIA.Reader/AgentDefinitionParser.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the front matter of an agent definition of Claude Code
 */

// Using
using OpenSpaceIA.Core.Model;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the front matter of an agent definition of Claude Code (a file <c>agents/&lt;file&gt;.md</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> the office needs three values of a definition: its name, its description and its model. A full YAML
/// parser would be a package of a third party for three keys; this one reads only the simple forms that Claude Code writes
/// and that the definitions of the author use (doc/analyse/spike-claude-files.md, § 4), and reports every other form instead
/// of guessing its value.</para>
/// <para><b>Behavior:</b> the front matter is the block between a first line <c>---</c> and the next line <c>---</c> (spaces
/// or tabs may follow them); the text after it is never read. Inside, only the keys <c>name</c>, <c>description</c> and
/// <c>model</c> are read: at the beginning of the line, case-sensitive as in Claude Code, followed by a colon and then a
/// space, a tab or the end of the line; every other line is left out. A value is read on one line only, in one of three
/// forms: plain (a comment after a space or a tab and a hash is left out), between double quotes without escape, or between
/// single quotes (two single quotes stand for one); a value between quotes can be empty, and is then no value. Any other form
/// is not read: an empty plain value, a value continued on the next indented lines, a block (<c>&gt;</c>, <c>|</c>), a quote
/// not closed or followed by text, an escape, a value that starts with a sign of YAML (<c>[ { &amp; * ! % @ #</c> or a back
/// quote), and the null values <c>~</c> and <c>null</c>. The name is required and follows the rules of Claude Code (at most
/// 256 characters, no colon), without spaces around it: without a name that can be read, the file is skipped, as Claude
/// Code skips it. A description or a model that is not read is left empty. A key read twice makes the file ambiguous: it is
/// skipped. Every file skipped and every value not read adds a problem; the content of the file is never copied in a
/// problem.</para>
/// </remarks>
internal static class AgentDefinitionParser
{
    #region Members

    #region Constants

    /// <summary>
    /// Line that opens and closes the front matter
    /// </summary>
    private const string Delimiter = "---";

    /// <summary>
    /// Key of the name of the agent
    /// </summary>
    private const string NameKey = "name";

    /// <summary>
    /// Key of the description of the agent
    /// </summary>
    private const string DescriptionKey = "description";

    /// <summary>
    /// Key of the model asked by the agent
    /// </summary>
    private const string ModelKey = "model";

    /// <summary>
    /// Longest name accepted by Claude Code
    /// </summary>
    private const int MaximumNameLength = 256;

    /// <summary>
    /// Signs that start a plain value that YAML does not read as plain text
    /// </summary>
    private const string YamlSigns = "[{&*!%@#`";

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
    /// Method used to know whether a line is the delimiter of the front matter
    /// </summary>
    /// <param name="line">[Cannot be null] Line of the file</param>
    /// <returns>
    /// True  ==> The line is <c>---</c>, with spaces or tabs after it at most<br/>
    /// False ==> The line is anything else
    /// </returns>
    private static bool IsDelimiter(string line)
    {
        // Function's return
        return string.Equals(line.TrimEnd(' ', '\t'), Delimiter, StringComparison.Ordinal);
    }

    /// <summary>
    /// Method used to know whether a line continues the value of the line before it
    /// </summary>
    /// <param name="line">[Cannot be null] Line of the front matter</param>
    /// <returns>
    /// True  ==> The line starts with a space or a tab, and is not blank<br/>
    /// False ==> The line starts a new key, a comment, or is blank
    /// </returns>
    private static bool IsContinuation(string line)
    {
        // Function's return
        return line.Length > 0 && (line[0] == ' ' || line[0] == '\t') && line.Trim(' ', '\t').Length > 0;
    }

    /// <summary>
    /// Method used to read the value written after the colon of a key
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> reads the simple forms of YAML, and says when the form is not one of them instead of guessing.</para>
    /// <para><b>Behavior:</b> see the forms read and not read in the remarks of the class.</para>
    /// </remarks>
    /// <param name="raw">[Cannot be null] Text after the colon</param>
    /// <param name="value">[Can be null] The value read; null when the form is not read</param>
    /// <returns>
    /// True  ==> The value is read (it is empty only between quotes)<br/>
    /// False ==> The value is written in a form that is not read
    /// </returns>
    private static bool TryReadValue(string raw, out string? value)
    {
        // Refusing an empty value, a block, a value that starts with a sign of YAML, and the null values
        value = null;
        string text = raw.Trim(' ', '\t');
        if (text.Length == 0 || text[0] == '>' || text[0] == '|' || YamlSigns.Contains(text[0], StringComparison.Ordinal)
            || text == "~" || text == "null")
        {
            // Function's return
            return false;
        }

        // Reading a value between double quotes, without escape
        if (text[0] == '"')
        {
            string inside = text.Length >= 2 && text[^1] == '"' ? text.Substring(1, text.Length - 2) : "\"";
            if (inside.Contains('"', StringComparison.Ordinal) || inside.Contains('\\', StringComparison.Ordinal))
            {
                // Function's return
                return false;
            }

            value = inside;

            // Function's return
            return true;
        }

        // Reading a value between single quotes, where two single quotes stand for one
        if (text[0] == '\'')
        {
            string inside = text.Length >= 2 && text[^1] == '\'' ? text.Substring(1, text.Length - 2) : "'";
            if (inside.Replace("''", string.Empty, StringComparison.Ordinal).Contains('\'', StringComparison.Ordinal))
            {
                // Function's return
                return false;
            }

            value = inside.Replace("''", "'", StringComparison.Ordinal);

            // Function's return
            return true;
        }

        // Reading a plain value, without the comment that follows a space or a tab and a hash
        int comment = text.IndexOf(" #", StringComparison.Ordinal);
        int tabComment = text.IndexOf("\t#", StringComparison.Ordinal);
        if (tabComment >= 0 && (comment < 0 || tabComment < comment))
            comment = tabComment;
        value = comment >= 0 ? text.Substring(0, comment).TrimEnd(' ', '\t') : text;

        // Function's return
        return true;
    }

    /// <summary>
    /// Method used to read an optional value (the description or the model), and to report it when its form is not read
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problem</param>
    /// <param name="values">[Cannot be null] Raw values of the keys read on one line</param>
    /// <param name="continued">[Cannot be null] Keys whose value continues on the next lines</param>
    /// <param name="key">[Cannot be null] Key of the value</param>
    /// <param name="problems">[Cannot be null] Problems, where a value not read is added</param>
    /// <returns>[Can be null] The value; null when the key is missing or its form is not read</returns>
    private static string? ReadOptionalValue(string filePath, Dictionary<string, string> values, HashSet<string> continued, string key, ICollection<ReadProblem> problems)
    {
        // Leaving out a missing key
        if (!values.TryGetValue(key, out string? raw))
        {
            // Function's return
            return null;
        }

        // Reporting a value that is not read
        string? value = null;
        if (continued.Contains(key) || !TryReadValue(raw, out value))
        {
            problems.Add(new ReadProblem(filePath, $"The value of the key: {key} is written in a form that is not read (empty, on several lines, or a form of YAML other than a simple text): it is left empty"));

            // Function's return
            return null;
        }

        // Function's return
        return value;
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the agent type defined by the text of a definition
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problems</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where every file skipped and every value not read is added</param>
    /// <returns>[Can be null] The agent type; null when the file is skipped</returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces, or if the text or the problems are null</exception>
    public static AgentType? Parse(string filePath, string text, ICollection<ReadProblem> problems)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), $"The value of the parameter: {nameof(filePath)} cannot be null or empty");
        if (text is null)
            throw new ArgumentNullException(nameof(text), $"The value of the parameter: {nameof(text)} cannot be null");
        if (problems is null)
            throw new ArgumentNullException(nameof(problems), $"The value of the parameter: {nameof(problems)} cannot be null");

        // Splitting the text into lines, without the carriage returns
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
            lines[index] = lines[index].TrimEnd('\r');

        // Skipping a file that does not start with a front matter
        if (!IsDelimiter(lines[0]))
        {
            problems.Add(new ReadProblem(filePath, "The file does not start with a front matter (a first line ---): it is skipped"));

            // Function's return
            return null;
        }

        // Finding the end of the front matter
        int end = Array.FindIndex(lines, 1, IsDelimiter);
        if (end < 0)
        {
            problems.Add(new ReadProblem(filePath, "The front matter is never closed (no second line ---): the file is skipped"));

            // Function's return
            return null;
        }

        // Keeping the raw values of the keys read, the keys continued on the next lines, and skipping a key given twice
        Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
        HashSet<string> continued = new HashSet<string>(StringComparer.Ordinal);
        string? lastKey = null;
        for (int index = 1; index < end; index++)
        {
            string line = lines[index];
            if (IsContinuation(line))
            {
                if (lastKey is not null)
                    continued.Add(lastKey);
                continue;
            }

            lastKey = null;
            int colon = line.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0 || (colon + 1 < line.Length && line[colon + 1] != ' ' && line[colon + 1] != '\t'))
                continue;

            string key = line.Substring(0, colon);
            if (key != NameKey && key != DescriptionKey && key != ModelKey)
                continue;

            if (!values.TryAdd(key, line.Substring(colon + 1)))
            {
                problems.Add(new ReadProblem(filePath, $"The key: {key} is given twice in the front matter: the file is skipped"));

                // Function's return
                return null;
            }

            lastKey = key;
        }

        // Skipping a file without a name that can be read
        string? name = null;
        if (!values.TryGetValue(NameKey, out string? rawName) || continued.Contains(NameKey) || !TryReadValue(rawName, out name) || string.IsNullOrWhiteSpace(name))
        {
            problems.Add(new ReadProblem(filePath, "The front matter has no name that can be read: the file is skipped, as Claude Code skips it"));

            // Function's return
            return null;
        }

        // Skipping a file whose name Claude Code refuses, or whose name has spaces around it
        if (name.Length > MaximumNameLength || name.Contains(':', StringComparison.Ordinal) || name != name.Trim())
        {
            problems.Add(new ReadProblem(filePath, $"The name is refused (more than {MaximumNameLength} characters, a colon, or spaces around it): the file is skipped"));

            // Function's return
            return null;
        }

        // Reading the description and the model, which are optional
        string? description = ReadOptionalValue(filePath, values, continued, DescriptionKey, problems);
        string? model = ReadOptionalValue(filePath, values, continued, ModelKey, problems);

        // Function's return
        return new AgentType(name, description, model);
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
