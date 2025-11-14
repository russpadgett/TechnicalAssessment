namespace SignalBooster.Models;

/// <summary>
/// Represents the extracted durable medical equipment information from a physician's note.
/// </summary>
public class DmeExtractionResult
{
    public string Device { get; set; } = "Unknown";
    public string? MaskType { get; set; }
    public List<string>? AddOns { get; set; }
    public string? Qualifier { get; set; }
    public string OrderingProvider { get; set; } = "Unknown";
    public string? Liters { get; set; }
    public string? Usage { get; set; }
    public string? PatientName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Diagnosis { get; set; }
}

