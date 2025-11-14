namespace SignalBooster.Services;

/// <summary>
/// Orchestrates the complete DME extraction and submission workflow.
/// Follows Single Responsibility Principle by coordinating the workflow without
/// containing the business logic of individual steps.
/// </summary>
public interface IDmeProcessingService
{
    /// <summary>
    /// Processes a physician note: reads the file, extracts DME information, and submits to API.
    /// </summary>
    /// <param name="inputFilePath">Path to the physician note file.</param>
    /// <param name="apiEndpoint">The API endpoint URL for submission.</param>
    /// <returns>True if processing was successful, false otherwise.</returns>
    Task<bool> ProcessPhysicianNoteAsync(string inputFilePath, string apiEndpoint);
}

