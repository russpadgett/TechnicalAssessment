namespace SignalBooster.Services.InputFormats;

/// <summary>
/// Parser for different input formats (plain text, JSON-wrapped, etc.).
/// </summary>
public interface IInputFormatParser
{
    /// <summary>
    /// Determines if this parser can handle the given input.
    /// </summary>
    /// <param name="input">The raw input content.</param>
    /// <returns>True if this parser can handle the input, false otherwise.</returns>
    bool CanParse(string input);

    /// <summary>
    /// Extracts the physician note text from the input format.
    /// </summary>
    /// <param name="input">The raw input content.</param>
    /// <returns>The extracted physician note text.</returns>
    /// <exception cref="FormatException">Thrown when the input format is invalid.</exception>
    string ExtractNoteText(string input);
}

