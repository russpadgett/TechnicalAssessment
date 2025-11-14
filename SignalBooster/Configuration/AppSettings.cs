namespace SignalBooster.Configuration;

/// <summary>
/// Application configuration settings.
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";

    /// <summary>
    /// Default input file path for physician notes.
    /// </summary>
    public string DefaultInputFile { get; set; } = "physician_note.txt";

    /// <summary>
    /// Default API endpoint for submitting DME data.
    /// </summary>
    public string DefaultApiEndpoint { get; set; } = "https://alert-api.com/DrExtract";

    /// <summary>
    /// Extraction method to use: "Pattern" (pattern-based) or "LLM" (AI-based).
    /// </summary>
    public string ExtractionMethod { get; set; } = "Pattern";

    /// <summary>
    /// LLM configuration settings.
    /// </summary>
    public LlmSettings? LlmSettings { get; set; }
}

/// <summary>
/// LLM (Large Language Model) configuration settings.
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// Azure OpenAI endpoint URL (for Azure OpenAI) or OpenAI API base URL.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Deployment name or model name (e.g., "gpt-4", "gpt-3.5-turbo").
    /// </summary>
    public string ModelName { get; set; } = "gpt-4";

    /// <summary>
    /// Whether to use Azure OpenAI (true) or OpenAI (false).
    /// </summary>
    public bool UseAzureOpenAi { get; set; } = false;
}

