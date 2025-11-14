using Microsoft.Extensions.Logging;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Registry implementation that maps device types to their specific extractors.
/// Follows Open/Closed Principle - new device types can be added by registering new extractors
/// without modifying this class.
/// </summary>
public class DeviceExtractorRegistry : IDeviceExtractorRegistry
{
    private readonly Dictionary<string, IDeviceSpecificExtractor> _extractors;
    private readonly ILogger<DeviceExtractorRegistry> _logger;

    public DeviceExtractorRegistry(
        IEnumerable<IDeviceSpecificExtractor> extractors,
        ILogger<DeviceExtractorRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _extractors = extractors?.ToDictionary(e => e.DeviceType, e => e) 
            ?? throw new ArgumentNullException(nameof(extractors));

        _logger.LogInformation("Registered {Count} device-specific extractors", _extractors.Count);
    }

    public IDeviceSpecificExtractor? GetExtractor(string deviceType)
    {
        if (string.IsNullOrWhiteSpace(deviceType))
        {
            return null;
        }

        if (_extractors.TryGetValue(deviceType, out var extractor))
        {
            _logger.LogDebug("Found extractor for device type: {DeviceType}", deviceType);
            return extractor;
        }

        _logger.LogDebug("No extractor found for device type: {DeviceType}", deviceType);
        return null;
    }
}

