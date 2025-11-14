using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SignalBooster.Models;
using System.Text;

namespace SignalBooster.Services.Extractors;

/// <summary>
/// LLM-based DME extractor using OpenAI or Azure OpenAI.
/// Uses AI to extract DME information from physician notes with high accuracy.
/// </summary>
public class LlmDmeExtractor : IDmeExtractor
{
    private readonly OpenAIClient? _openAiClient;
    private readonly string _modelName;
    private readonly ILogger<LlmDmeExtractor> _logger;
    private readonly bool _isConfigured;

    public LlmDmeExtractor(
        ILogger<LlmDmeExtractor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isConfigured = false;
        _modelName = string.Empty;
    }

    public LlmDmeExtractor(
        OpenAIClient openAiClient,
        string modelName,
        ILogger<LlmDmeExtractor> logger)
    {
        _openAiClient = openAiClient ?? throw new ArgumentNullException(nameof(openAiClient));
        _modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isConfigured = true;
    }

    public DmeExtractionResult ExtractDmeInfo(string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _logger.LogWarning("Empty or null note text provided for LLM extraction");
            return new DmeExtractionResult();
        }

        if (!_isConfigured || _openAiClient == null)
        {
            _logger.LogWarning("LLM extractor not configured, returning empty result");
            return new DmeExtractionResult();
        }

        _logger.LogInformation("Extracting DME information using LLM: {ModelName}", _modelName);

        try
        {
            var prompt = BuildExtractionPrompt(noteText);
            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are a medical data extraction assistant. Extract DME (Durable Medical Equipment) information from physician notes and return ONLY valid JSON."),
                new ChatRequestUserMessage(prompt)
            };
            
            var options = new ChatCompletionsOptions(_modelName, messages)
            {
                Temperature = 0.1f, // Low temperature for more consistent, structured output
                MaxTokens = 1000
            };

            var response = _openAiClient.GetChatCompletionsAsync(options).GetAwaiter().GetResult();

            var content = response.Value.Choices[0].Message.Content;
            _logger.LogDebug("LLM response received: {Response}", content);

            var result = ParseLlmResponse(content);
            _logger.LogInformation("LLM extraction complete. Device: {Device}, Provider: {Provider}", 
                result.Device, result.OrderingProvider);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LLM extraction");
            // Return empty result on error rather than crashing
            return new DmeExtractionResult();
        }
    }

    private string BuildExtractionPrompt(string noteText)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("Extract DME information from the following physician note and return it as JSON.");
        prompt.AppendLine();
        prompt.AppendLine("Required JSON structure:");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"device\": \"CPAP\" | \"Oxygen Tank\" | \"Wheelchair\" | \"Unknown\",");
        prompt.AppendLine("  \"ordering_provider\": \"Dr. Name\" | \"Unknown\",");
        prompt.AppendLine("  \"patient_name\": \"Patient Name\" | null,");
        prompt.AppendLine("  \"dob\": \"MM/DD/YYYY\" | null,");
        prompt.AppendLine("  \"diagnosis\": \"Diagnosis\" | null,");
        prompt.AppendLine("  \"mask_type\": \"full face\" | \"nasal\" | \"nasal pillow\" | null,");
        prompt.AppendLine("  \"add_ons\": [\"humidifier\", \"heated humidifier\"] | null,");
        prompt.AppendLine("  \"qualifier\": \"AHI > 20\" | null,");
        prompt.AppendLine("  \"liters\": \"2 L\" | null,");
        prompt.AppendLine("  \"usage\": \"sleep\" | \"exertion\" | \"sleep and exertion\" | null");
        prompt.AppendLine("}");
        prompt.AppendLine();
        prompt.AppendLine("Physician Note:");
        prompt.AppendLine(noteText);
        prompt.AppendLine();
        prompt.AppendLine("Return ONLY the JSON object, no additional text or explanation.");

        return prompt.ToString();
    }

    private DmeExtractionResult ParseLlmResponse(string response)
    {
        try
        {
            // Try to extract JSON from the response (LLM might add markdown formatting)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var jsonObj = JObject.Parse(jsonText);

                return new DmeExtractionResult
                {
                    Device = jsonObj["device"]?.ToString() ?? "Unknown",
                    OrderingProvider = jsonObj["ordering_provider"]?.ToString() ?? "Unknown",
                    PatientName = jsonObj["patient_name"]?.ToString(),
                    DateOfBirth = jsonObj["dob"]?.ToString(),
                    Diagnosis = jsonObj["diagnosis"]?.ToString(),
                    MaskType = jsonObj["mask_type"]?.ToString(),
                    AddOns = jsonObj["add_ons"]?.ToObject<List<string>>(),
                    Qualifier = jsonObj["qualifier"]?.ToString(),
                    Liters = jsonObj["liters"]?.ToString(),
                    Usage = jsonObj["usage"]?.ToString()
                };
            }

            _logger.LogWarning("Could not extract JSON from LLM response");
            return new DmeExtractionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing LLM response as JSON");
            return new DmeExtractionResult();
        }
    }
}

