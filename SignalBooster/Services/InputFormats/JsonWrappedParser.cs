using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace SignalBooster.Services.InputFormats;

/// <summary>
/// Parser for JSON-wrapped input format (e.g., {"data": "physician note text"}).
/// </summary>
public class JsonWrappedParser : IInputFormatParser
{
    private readonly ILogger<JsonWrappedParser> _logger;

    public JsonWrappedParser(ILogger<JsonWrappedParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanParse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        
        // Check if it looks like JSON (starts with { or [)
        if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
        {
            return false;
        }

        try
        {
            // Try to parse as JSON
            var json = JToken.Parse(trimmed);
            
            // Check if it has a "data" field (common pattern for wrapped notes)
            if (json is JObject obj && obj["data"] != null)
            {
                _logger.LogDebug("Detected JSON-wrapped format with 'data' field");
                return true;
            }

            // Check if it's a JSON object that might contain note text
            if (json is JObject)
            {
                _logger.LogDebug("Detected JSON object format");
                return true;
            }
        }
        catch
        {
            // Not valid JSON
            return false;
        }

        return false;
    }

    public string ExtractNoteText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new FormatException("Input cannot be null or empty");
        }

        try
        {
            var json = JToken.Parse(input.Trim());

            // Try to extract from "data" field first (most common pattern)
            if (json is JObject obj)
            {
                if (obj["data"] != null)
                {
                    var dataValue = obj["data"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dataValue))
                    {
                        _logger.LogDebug("Extracted note text from 'data' field");
                        return dataValue;
                    }
                }

                // Try other common field names
                var possibleFields = new[] { "note", "text", "content", "physicianNote", "physician_note" };
                foreach (var field in possibleFields)
                {
                    if (obj[field] != null)
                    {
                        var value = obj[field]?.ToString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            _logger.LogDebug("Extracted note text from '{Field}' field", field);
                            return value;
                        }
                    }
                }

                // If no specific field found, try to concatenate all string values
                var allText = string.Join("\n", obj.Properties()
                    .Where(p => p.Value?.Type == JTokenType.String)
                    .Select(p => p.Value?.ToString() ?? string.Empty));

                if (!string.IsNullOrWhiteSpace(allText))
                {
                    _logger.LogDebug("Extracted note text by concatenating JSON string values");
                    return allText;
                }
            }

            throw new FormatException("Could not extract note text from JSON structure");
        }
        catch (Exception ex) when (ex is not FormatException)
        {
            _logger.LogError(ex, "Error parsing JSON input");
            throw new FormatException("Invalid JSON format", ex);
        }
    }
}

