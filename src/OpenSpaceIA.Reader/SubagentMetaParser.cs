/*
 *	@file	src/OpenSpaceIA.Reader/SubagentMetaParser.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Reading of the description of a sub-agent run
 */

// Using
using System.Text.Json;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Reader;

/// <summary>
/// Service used to read the description of a sub-agent run (<c>agent-&lt;id&gt;.meta.json</c>)
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> keeps the agent type of the run, whether it is finished, and the model asked
/// (doc/analyse/spike-claude-files.md, § 3).</para>
/// <para><b>Behavior:</b> the text must be one JSON object with a non-blank text <c>agentType</c>: without it, the
/// description is refused and reported, since the run cannot be placed at a desk. <c>finished</c> (a boolean) and
/// <c>model</c> (a text) are optional; a value of another kind is left out and reported. The other keys are left out. A text
/// with an escaped half of a character is not a valid text. A problem never holds a value of the file.</para>
/// </remarks>
internal static class SubagentMetaParser
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
    /// Method used to read the description from the object of its file
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problems</param>
    /// <param name="root">Object of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where a description refused or a value left out is added</param>
    /// <returns>[Can be null] The description; null when it has no agent type</returns>
    /// <exception cref="InvalidOperationException">If a text holds an escaped half of a character (caught by the caller)</exception>
    private static SubagentMeta? ReadMeta(string filePath, JsonElement root, ICollection<ReadProblem> problems)
    {
        // Reading the agent type, which is required
        string? agentType = root.TryGetProperty("agentType", out JsonElement agentTypeValue) && agentTypeValue.ValueKind == JsonValueKind.String
            ? agentTypeValue.GetString()
            : null;
        if (string.IsNullOrWhiteSpace(agentType))
        {
            problems.Add(new ReadProblem(filePath, "The key: agentType is missing or is not a text: the run cannot be placed at a desk, so it is left out"));

            // Function's return
            return null;
        }

        // Reading whether the run is finished
        bool isFinished = false;
        if (root.TryGetProperty("finished", out JsonElement finishedValue))
        {
            if (finishedValue.ValueKind == JsonValueKind.True || finishedValue.ValueKind == JsonValueKind.False)
                isFinished = finishedValue.GetBoolean();
            else
                problems.Add(new ReadProblem(filePath, "The value of the key: finished is not a boolean: it is left out"));
        }

        // Reading the model asked; a text with an escaped half of a character is not a text
        string? model = null;
        if (root.TryGetProperty("model", out JsonElement modelValue))
        {
            try
            {
                model = modelValue.ValueKind == JsonValueKind.String ? modelValue.GetString() : null;
            }
            catch (InvalidOperationException)
            {
                model = null;
            }
            if (model is null)
                problems.Add(new ReadProblem(filePath, "The value of the key: model is not a text: it is left out"));
        }

        // Function's return
        return new SubagentMeta(agentType, isFinished, model);
    }

    #endregion

    #region Publics

    /// <summary>
    /// Method used to read the description of a run from the text of its file
    /// </summary>
    /// <param name="filePath">[Cannot be null] Full path of the file, for the problems</param>
    /// <param name="text">[Cannot be null] Text of the file</param>
    /// <param name="problems">[Cannot be null] Problems, where a description refused or a value left out is added</param>
    /// <returns>[Can be null] The description; null when the file cannot give one</returns>
    /// <exception cref="ArgumentNullException">If the path is null, empty or white spaces, or if the text or the problems are null</exception>
    public static SubagentMeta? Parse(string filePath, string text, ICollection<ReadProblem> problems)
    {
        // Checks
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), $"The value of the parameter: {nameof(filePath)} cannot be null or empty");
        if (text is null)
            throw new ArgumentNullException(nameof(text), $"The value of the parameter: {nameof(text)} cannot be null");
        if (problems is null)
            throw new ArgumentNullException(nameof(problems), $"The value of the parameter: {nameof(problems)} cannot be null");

        try
        {
            // Reading the text as one JSON object
            using JsonDocument document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                problems.Add(new ReadProblem(filePath, "The description is not a JSON object: the run is left out"));

                // Function's return
                return null;
            }

            // Function's return
            return ReadMeta(filePath, document.RootElement, problems);
        }
        catch (InvalidOperationException)
        {
            problems.Add(new ReadProblem(filePath, "The key: agentType holds an escaped half of a character, which is not a valid text: the run is left out"));

            // Function's return
            return null;
        }
        catch (JsonException exception)
        {
            problems.Add(new ReadProblem(filePath, $"The description is not valid JSON (line {exception.LineNumber}, byte {exception.BytePositionInLine}): the run is left out"));

            // Function's return
            return null;
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
