using Microsoft.Extensions.Logging;
using SignalBooster.Models;
using System.Text.RegularExpressions;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Extracts CPAP-specific information from physician notes.
/// Follows Single Responsibility Principle by handling only CPAP device extraction.
/// </summary>
public class CpapExtractor : IDeviceSpecificExtractor
{
    private readonly ILogger<CpapExtractor> _logger;

    public CpapExtractor(ILogger<CpapExtractor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string DeviceType => "CPAP";

    public void ExtractDeviceSpecificFields(string noteText, DmeExtractionResult result)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _logger.LogWarning("Empty note text provided for CPAP extraction");
            return;
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        _logger.LogDebug("Extracting CPAP-specific fields");

        result.MaskType = ExtractMaskType(noteText);
        result.AddOns = ExtractAddOns(noteText);
        result.Qualifier = ExtractQualifier(noteText);
    }

    private string? ExtractMaskType(string noteText)
    {
        if (noteText.Contains("full face", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected full face mask type");
            return "full face";
        }
        
        if (noteText.Contains("nasal pillow", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected nasal pillow mask type");
            return "nasal pillow";
        }
        
        if (noteText.Contains("nasal", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected nasal mask type");
            return "nasal";
        }

        return null;
    }

    private List<string>? ExtractAddOns(string noteText)
    {
        var addOns = new List<string>();

        if (noteText.Contains("humidifier", StringComparison.OrdinalIgnoreCase))
        {
            var humidifierType = "humidifier";
            if (noteText.Contains("heated humidifier", StringComparison.OrdinalIgnoreCase))
            {
                humidifierType = "heated humidifier";
            }
            addOns.Add(humidifierType);
            _logger.LogDebug("Detected add-on: {AddOn}", humidifierType);
        }

        return addOns.Count > 0 ? addOns : null;
    }

    private string? ExtractQualifier(string noteText)
    {
        // Look for AHI (Apnea-Hypopnea Index) qualifiers
        var ahiPattern = @"AHI\s*[>:]\s*(\d+)";
        var match = Regex.Match(noteText, ahiPattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var qualifier = match.Value;
            _logger.LogDebug("Detected qualifier: {Qualifier}", qualifier);
            return qualifier;
        }

        return null;
    }
}

