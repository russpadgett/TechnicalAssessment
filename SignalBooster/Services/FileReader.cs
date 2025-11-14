using Microsoft.Extensions.Logging;
using SignalBooster.Services.InputFormats;

namespace SignalBooster.Services;

/// <summary>
/// Implementation of IFileReader for reading physician notes from the file system.
/// Supports multiple input formats (plain text, JSON-wrapped).
/// </summary>
public class FileReader : IFileReader
{
    private readonly ILogger<FileReader> _logger;
    private readonly InputFormatParserRegistry _parserRegistry;

    public FileReader(
        ILogger<FileReader> logger,
        InputFormatParserRegistry parserRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("File path is null or empty");
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException($"Physician note file not found: {filePath}", filePath);
        }

        try
        {
            _logger.LogInformation("Reading physician note from: {FilePath}", filePath);
            var rawContent = await File.ReadAllTextAsync(filePath);
            _logger.LogDebug("Successfully read {Length} characters from file", rawContent.Length);

            // Parse the content using the appropriate format parser
            var parser = _parserRegistry.GetParser(rawContent);
            var noteText = parser.ExtractNoteText(rawContent);
            
            _logger.LogDebug("Extracted note text using {ParserType} parser", parser.GetType().Name);
            return noteText;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when reading file: {FilePath}", filePath);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error occurred while reading file: {FilePath}", filePath);
            throw;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error parsing file content: {FilePath}", filePath);
            throw;
        }
    }
}

