namespace SignalBooster.Services.Extractors;

/// <summary>
/// Registry for device-specific extractors. Enables dependency injection and extensibility.
/// </summary>
public interface IDeviceExtractorRegistry
{
    /// <summary>
    /// Gets the appropriate device-specific extractor for the given device type.
    /// </summary>
    /// <param name="deviceType">The device type (e.g., "CPAP", "Oxygen Tank").</param>
    /// <returns>The device-specific extractor, or null if no extractor is registered for the device type.</returns>
    IDeviceSpecificExtractor? GetExtractor(string deviceType);
}

