using SignalBooster.Models;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Extracts common fields that apply to all DME types (patient info, provider, etc.).
/// </summary>
public interface ICommonFieldExtractor
{
    /// <summary>
    /// Extracts common fields from the physician note.
    /// </summary>
    /// <param name="noteText">The text content of the physician's note.</param>
    /// <returns>A DmeExtractionResult with common fields populated.</returns>
    DmeExtractionResult ExtractCommonFields(string noteText);
}

