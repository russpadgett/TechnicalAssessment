using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SignalBooster.Models;

namespace SignalBooster.Services;

/// <summary>
/// Serializes DME extraction results to JSON format for API submission.
/// </summary>
public class DmeDataSerializer : IDmeDataSerializer
{
    private readonly ILogger<DmeDataSerializer> _logger;

    public DmeDataSerializer(ILogger<DmeDataSerializer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts a DmeExtractionResult to a JSON string.
    /// </summary>
    /// <param name="result">The DME extraction result to serialize.</param>
    /// <returns>A JSON string representation of the result.</returns>
    public string SerializeToJson(DmeExtractionResult result)
    {
        if (result == null)
        {
            _logger.LogError("Cannot serialize null DME extraction result");
            throw new ArgumentNullException(nameof(result));
        }

        try
        {
            var jsonObject = new JObject
            {
                ["device"] = result.Device,
                ["ordering_provider"] = result.OrderingProvider
            };

            // Add optional fields only if they have values
            if (!string.IsNullOrWhiteSpace(result.MaskType))
            {
                jsonObject["mask_type"] = result.MaskType;
            }

            if (result.AddOns != null && result.AddOns.Count > 0)
            {
                jsonObject["add_ons"] = new JArray(result.AddOns);
            }

            if (!string.IsNullOrWhiteSpace(result.Qualifier))
            {
                jsonObject["qualifier"] = result.Qualifier;
            }

            if (!string.IsNullOrWhiteSpace(result.PatientName))
            {
                jsonObject["patient_name"] = result.PatientName;
            }

            if (!string.IsNullOrWhiteSpace(result.DateOfBirth))
            {
                jsonObject["dob"] = result.DateOfBirth;
            }

            if (!string.IsNullOrWhiteSpace(result.Diagnosis))
            {
                jsonObject["diagnosis"] = result.Diagnosis;
            }

            // Oxygen Tank specific fields
            if (result.Device == "Oxygen Tank")
            {
                if (!string.IsNullOrWhiteSpace(result.Liters))
                {
                    jsonObject["liters"] = result.Liters;
                }

                if (!string.IsNullOrWhiteSpace(result.Usage))
                {
                    jsonObject["usage"] = result.Usage;
                }
            }

            var jsonString = jsonObject.ToString();
            _logger.LogDebug("Serialized DME data to JSON: {Json}", jsonString);
            return jsonString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing DME extraction result to JSON");
            throw;
        }
    }
}

