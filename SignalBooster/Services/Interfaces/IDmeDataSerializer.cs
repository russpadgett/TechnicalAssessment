using SignalBooster.Models;

namespace SignalBooster.Services;

/// <summary>
/// Serializes DME extraction results to JSON format for API submission.
/// </summary>
public interface IDmeDataSerializer
{
    /// <summary>
    /// Converts a DmeExtractionResult to a JSON string.
    /// </summary>
    /// <param name="result">The DME extraction result to serialize.</param>
    /// <returns>A JSON string representation of the result.</returns>
    string SerializeToJson(DmeExtractionResult result);
}

