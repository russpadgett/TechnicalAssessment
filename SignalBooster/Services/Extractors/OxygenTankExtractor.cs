using Microsoft.Extensions.Logging;
using SignalBooster.Models;
using System.Text.RegularExpressions;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Extracts Oxygen Tank-specific information from physician notes.
/// Follows Single Responsibility Principle by handling only Oxygen Tank device extraction.
/// </summary>
public class OxygenTankExtractor : IDeviceSpecificExtractor
{
    private readonly ILogger<OxygenTankExtractor> _logger;

    public OxygenTankExtractor(ILogger<OxygenTankExtractor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string DeviceType => "Oxygen Tank";

    public void ExtractDeviceSpecificFields(string noteText, DmeExtractionResult result)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _logger.LogWarning("Empty note text provided for Oxygen Tank extraction");
            return;
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        _logger.LogDebug("Extracting Oxygen Tank-specific fields");

        result.Liters = ExtractLiters(noteText);
        result.Usage = ExtractUsage(noteText);
    }

    private string? ExtractLiters(string noteText)
    {
        // Match patterns like "2 L", "2L", "2.5 L", "2.5L"
        var litersPattern = @"(\d+(?:\.\d+)?)\s*L";
        var match = Regex.Match(noteText, litersPattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var liters = match.Groups[1].Value + " L";
            _logger.LogDebug("Extracted liters: {Liters}", liters);
            return liters;
        }

        return null;
    }

    private string? ExtractUsage(string noteText)
    {
        var hasSleep = noteText.Contains("sleep", StringComparison.OrdinalIgnoreCase);
        var hasExertion = noteText.Contains("exertion", StringComparison.OrdinalIgnoreCase);

        if (hasSleep && hasExertion)
        {
            _logger.LogDebug("Detected usage: sleep and exertion");
            return "sleep and exertion";
        }
        
        if (hasSleep)
        {
            _logger.LogDebug("Detected usage: sleep");
            return "sleep";
        }
        
        if (hasExertion)
        {
            _logger.LogDebug("Detected usage: exertion");
            return "exertion";
        }

        return null;
    }
}

