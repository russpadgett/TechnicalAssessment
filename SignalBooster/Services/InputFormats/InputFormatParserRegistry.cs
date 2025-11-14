using Microsoft.Extensions.Logging;

namespace SignalBooster.Services.InputFormats;

/// <summary>
/// Registry for input format parsers. Determines which parser to use based on input format.
/// </summary>
public class InputFormatParserRegistry
{
    private readonly List<IInputFormatParser> _parsers;
    private readonly ILogger<InputFormatParserRegistry> _logger;

    public InputFormatParserRegistry(
        IEnumerable<IInputFormatParser> parsers,
        ILogger<InputFormatParserRegistry> logger)
    {
        _parsers = parsers?.OrderByDescending(p => p is JsonWrappedParser ? 1 : 0).ToList() 
            ?? throw new ArgumentNullException(nameof(parsers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the appropriate parser for the given input.
    /// </summary>
    /// <param name="input">The raw input content.</param>
    /// <returns>The parser that can handle the input format.</returns>
    /// <exception cref="FormatException">Thrown when no parser can handle the input.</exception>
    public IInputFormatParser GetParser(string input)
    {
        // Try parsers in order (JSON first, then plain text as fallback)
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(input))
            {
                _logger.LogDebug("Selected parser: {ParserType}", parser.GetType().Name);
                return parser;
            }
        }

        _logger.LogError("No parser found for input format");
        throw new FormatException("No parser available for the input format");
    }
}

