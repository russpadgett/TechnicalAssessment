using Microsoft.Extensions.Logging;
using SignalBooster.Models;
using System.Text.RegularExpressions;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// Extracts common fields from physician notes that apply to all device types.
/// Follows Single Responsibility Principle by focusing only on common field extraction.
/// </summary>
public class CommonFieldExtractor : ICommonFieldExtractor
{
    private readonly ILogger<CommonFieldExtractor> _logger;

    public CommonFieldExtractor(ILogger<CommonFieldExtractor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DmeExtractionResult ExtractCommonFields(string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _logger.LogWarning("Empty or null note text provided for common field extraction");
            return new DmeExtractionResult();
        }

        _logger.LogDebug("Extracting common fields from physician note");

        return new DmeExtractionResult
        {
            Device = ExtractDeviceType(noteText),
            OrderingProvider = ExtractOrderingProvider(noteText),
            PatientName = ExtractPatientName(noteText),
            DateOfBirth = ExtractDateOfBirth(noteText),
            Diagnosis = ExtractDiagnosis(noteText)
        };
    }

    private string ExtractDeviceType(string noteText)
    {
        if (noteText.Contains("CPAP", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected CPAP device");
            return "CPAP";
        }
        
        if (noteText.Contains("oxygen", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected Oxygen Tank device");
            return "Oxygen Tank";
        }
        
        if (noteText.Contains("wheelchair", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Detected Wheelchair device");
            return "Wheelchair";
        }

        _logger.LogWarning("No recognized device type found in note");
        return "Unknown";
    }

    /// <summary>
    /// Extracts the ordering provider name from the physician note.
    /// Uses multiple regex patterns with fallback logic to handle various formats.
    /// 
    /// Limitations of pattern-based extraction:
    /// - May miss providers with unusual name formats (hyphenated, special characters)
    /// - May not handle all international name formats
    /// - Requires specific text patterns to be present
    /// 
    /// For higher accuracy with varied formats, consider using LLM-based extraction
    /// by setting ExtractionMethod to "LLM" in appsettings.json.
    /// </summary>
    private string ExtractOrderingProvider(string noteText)
    {
        // Multiple patterns to handle various formats and edge cases
        // Patterns are ordered by specificity (most specific first, fallbacks last)
        var patterns = new[]
        {
            // Standard format: "Ordering Physician: Dr. LastName" or "Ordered by Dr. LastName"
            @"(?:Ordering\s+Physician|Ordered\s+by|Physician)[:\s]+(Dr\.\s+[A-Za-z]+(?:\s+[A-Za-z]+)?(?:\s+[A-Z]\.)?(?:\s+[A-Za-z]+)?(?:\s*,\s*(?:MD|DO|DDS|DMD|PhD))?)",
            
            // Format with full name: "Dr. FirstName LastName"
            @"(?:Ordering\s+Physician|Ordered\s+by|Physician)[:\s]+(Dr\.\s+[A-Z][a-z]+\s+[A-Z][a-z]+(?:\s+[A-Z]\.)?(?:\s+[A-Za-z]+)?)",
            
            // Format without "Dr." prefix but with title: "Physician: John Smith, MD"
            @"(?:Ordering\s+Physician|Ordered\s+by|Physician)[:\s]+([A-Z][a-z]+(?:\s+[A-Z]\.)?\s+[A-Z][a-z]+(?:\s*,\s*(?:MD|DO|DDS|DMD|PhD))?)",
            
            // Fallback: Any "Dr. Name" pattern in the text
            @"\b(Dr\.\s+[A-Z][a-z]+(?:\s+[A-Z][a-z]+)?(?:\s+[A-Z]\.)?)",
            
            // Fallback: Name with medical title suffix
            @"([A-Z][a-z]+\s+[A-Z][a-z]+(?:\s+[A-Z]\.)?\s*,\s*(?:MD|DO|DDS|DMD|PhD))\b"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(noteText, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            if (match.Success)
            {
                var provider = match.Groups[1].Value.Trim();
                
                // Validate extracted provider (basic sanity check)
                if (provider.Length > 3 && provider.Length < 100)
                {
                    _logger.LogDebug("Extracted ordering provider using pattern: {Provider}", provider);
                    return provider;
                }
                else
                {
                    _logger.LogWarning("Extracted provider appears invalid (length: {Length}): {Provider}", 
                        provider.Length, provider);
                }
            }
        }

        _logger.LogWarning("No ordering provider found in note after trying {Count} patterns", patterns.Length);
        return "Unknown";
    }

    private string? ExtractPatientName(string noteText)
    {
        // Match until end of line or next field label
        var namePattern = @"Patient\s+Name[:\s]+([A-Za-z\s]+?)(?:\r?\n|$)";
        var match = Regex.Match(noteText, namePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        if (match.Success)
        {
            var name = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted patient name: {Name}", name);
            return name;
        }

        return null;
    }

    private string? ExtractDateOfBirth(string noteText)
    {
        var dobPattern = @"DOB[:\s]+(\d{2}/\d{2}/\d{4})";
        var match = Regex.Match(noteText, dobPattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var dob = match.Groups[1].Value;
            _logger.LogDebug("Extracted date of birth: {DOB}", dob);
            return dob;
        }

        return null;
    }

    private string? ExtractDiagnosis(string noteText)
    {
        // Match until end of line or next field label
        var diagnosisPattern = @"Diagnosis[:\s]+([A-Za-z\s]+?)(?:\r?\n|$)";
        var match = Regex.Match(noteText, diagnosisPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        if (match.Success)
        {
            var diagnosis = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted diagnosis: {Diagnosis}", diagnosis);
            return diagnosis;
        }

        return null;
    }
}

