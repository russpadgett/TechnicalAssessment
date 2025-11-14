using SignalBooster.Models;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Interface for device-specific extractors that follow the Strategy pattern.
/// Allows extension for new device types without modifying existing code (Open/Closed Principle).
/// </summary>
public interface IDeviceSpecificExtractor
{
    /// <summary>
    /// The device type this extractor handles (e.g., "CPAP", "Oxygen Tank").
    /// </summary>
    string DeviceType { get; }

    /// <summary>
    /// Extracts device-specific information from the note text.
    /// </summary>
    /// <param name="noteText">The text content of the physician's note.</param>
    /// <param name="result">The DME extraction result to populate with device-specific data.</param>
    void ExtractDeviceSpecificFields(string noteText, DmeExtractionResult result);
}

