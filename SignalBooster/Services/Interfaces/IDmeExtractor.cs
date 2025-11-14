using SignalBooster.Models;

namespace SignalBooster.Services;

/// <summary>
/// Service for extracting durable medical equipment information from physician notes.
/// Orchestrates common field extraction and device-specific extraction.
/// </summary>
public interface IDmeExtractor
{
    /// <summary>
    /// Extracts DME information from a physician's note text.
    /// </summary>
    /// <param name="noteText">The text content of the physician's note.</param>
    /// <returns>A DmeExtractionResult containing the extracted information.</returns>
    DmeExtractionResult ExtractDmeInfo(string noteText);
}

