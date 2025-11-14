namespace SignalBooster.Services;

/// <summary>
/// Service for reading physician notes from files.
/// </summary>
public interface IFileReader
{
    /// <summary>
    /// Reads the content of a physician note file.
    /// </summary>
    /// <param name="filePath">Path to the physician note file.</param>
    /// <returns>The content of the file as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file.</exception>
    Task<string> ReadFileAsync(string filePath);
}

