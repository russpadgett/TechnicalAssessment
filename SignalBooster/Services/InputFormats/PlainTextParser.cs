using Microsoft.Extensions.Logging;

namespace SignalBooster.Services.InputFormats;

/// <summary>
/// Parser for plain text input format (default format).
/// </summary>
public class PlainTextParser : IInputFormatParser
{
    private readonly ILogger<PlainTextParser> _logger;

    public PlainTextParser(ILogger<PlainTextParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanParse(string input)
    {
        // Plain text parser can always handle input (it's the fallback)
        return true;
    }

    public string ExtractNoteText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogWarning("Empty input provided to plain text parser");
            return string.Empty;
        }

        _logger.LogDebug("Parsing plain text input");
        return input.Trim();
    }
}

