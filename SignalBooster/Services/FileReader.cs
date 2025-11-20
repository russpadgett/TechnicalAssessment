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

        // Resolve relative paths relative to the application content root (where appsettings.json is)
        // This ensures the file can be found regardless of the current working directory
        var resolvedPath = ResolveFilePath(filePath);
        
        if (!File.Exists(resolvedPath))
        {
            _logger.LogError("File not found: {FilePath} (resolved from: {OriginalPath})", resolvedPath, filePath);
            throw new FileNotFoundException($"Physician note file not found: {resolvedPath}", resolvedPath);
        }

        try
        {
            _logger.LogInformation("Reading physician note from: {FilePath}", resolvedPath);
            var rawContent = await File.ReadAllTextAsync(resolvedPath);
            _logger.LogDebug("Successfully read {Length} characters from file", rawContent.Length);

            // Parse the content using the appropriate format parser
            var parser = _parserRegistry.GetParser(rawContent);
            var noteText = parser.ExtractNoteText(rawContent);
            
            _logger.LogDebug("Extracted note text using {ParserType} parser", parser.GetType().Name);
            return noteText;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when reading file: {FilePath}", resolvedPath);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error occurred while reading file: {FilePath}", resolvedPath);
            throw;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error parsing file content: {FilePath}", resolvedPath);
            throw;
        }
    }

    /// <summary>
    /// Resolves a file path, handling relative paths by searching relative to the application content root.
    /// This ensures files can be found regardless of the current working directory (which differs between
    /// Visual Studio, Cursor, and command-line execution).
    /// </summary>
    private static string ResolveFilePath(string filePath)
    {
        // If the path is absolute, use it as-is
        if (Path.IsPathRooted(filePath))
        {
            return filePath;
        }

        // For relative paths, try multiple locations:
        // 1. Current working directory (for compatibility)
        var currentDirPath = Path.GetFullPath(filePath);
        if (File.Exists(currentDirPath))
        {
            return currentDirPath;
        }

        // 2. Application base directory (where the DLL is, typically bin/Debug/net8.0)
        var appBaseDir = AppContext.BaseDirectory;
        var appBasePath = Path.GetFullPath(Path.Combine(appBaseDir, filePath));
        if (File.Exists(appBasePath))
        {
            return appBasePath;
        }

        // 3. Content root (where appsettings.json is - navigate up from bin/Debug/net8.0)
        // Walk up the directory tree from the application base directory to find appsettings.json
        var directory = new DirectoryInfo(appBaseDir);
        while (directory != null)
        {
            var appSettingsPath = Path.Combine(directory.FullName, "appsettings.json");
            
            // Found the content root (where appsettings.json is located)
            if (File.Exists(appSettingsPath))
            {
                var resolvedPath = Path.GetFullPath(Path.Combine(directory.FullName, filePath));
                if (File.Exists(resolvedPath))
                {
                    return resolvedPath;
                }
            }
            
            directory = directory.Parent;
        }

        // If not found in any location, return the path as-is (will fail with FileNotFoundException)
        // This preserves the original behavior and allows better error messages
        return filePath;
    }
}

