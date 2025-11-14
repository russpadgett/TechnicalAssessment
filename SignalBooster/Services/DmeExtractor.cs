using Microsoft.Extensions.Logging;
using SignalBooster.Models;
using SignalBooster.Services.Extractors;

namespace SignalBooster.Services;

/// <summary>
/// Orchestrates the extraction of DME information by coordinating common field extraction
/// and device-specific extraction. Follows Single Responsibility Principle by delegating
/// to specialized extractors rather than containing all extraction logic.
/// </summary>
public class DmeExtractor : IDmeExtractor
{
    private readonly ICommonFieldExtractor _commonFieldExtractor;
    private readonly IDeviceExtractorRegistry _deviceExtractorRegistry;
    private readonly ILogger<DmeExtractor> _logger;

    public DmeExtractor(
        ICommonFieldExtractor commonFieldExtractor,
        IDeviceExtractorRegistry deviceExtractorRegistry,
        ILogger<DmeExtractor> logger)
    {
        _commonFieldExtractor = commonFieldExtractor ?? throw new ArgumentNullException(nameof(commonFieldExtractor));
        _deviceExtractorRegistry = deviceExtractorRegistry ?? throw new ArgumentNullException(nameof(deviceExtractorRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DmeExtractionResult ExtractDmeInfo(string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _logger.LogWarning("Empty or null note text provided for extraction");
            return new DmeExtractionResult();
        }

        _logger.LogInformation("Extracting DME information from physician note");

        // Extract common fields (device type, patient info, provider, etc.)
        var result = _commonFieldExtractor.ExtractCommonFields(noteText);

        // Extract device-specific fields using the appropriate strategy
        var deviceSpecificExtractor = _deviceExtractorRegistry.GetExtractor(result.Device);
        if (deviceSpecificExtractor != null)
        {
            _logger.LogDebug("Applying device-specific extraction for: {DeviceType}", result.Device);
            deviceSpecificExtractor.ExtractDeviceSpecificFields(noteText, result);
        }
        else if (result.Device != "Unknown")
        {
            _logger.LogWarning("No device-specific extractor found for device type: {DeviceType}", result.Device);
        }

        _logger.LogInformation("Extraction complete. Device: {Device}, Provider: {Provider}", 
            result.Device, result.OrderingProvider);

        return result;
    }
}

