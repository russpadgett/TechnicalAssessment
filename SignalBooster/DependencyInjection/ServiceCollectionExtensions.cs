using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalBooster.Configuration;
using SignalBooster.Services;
using SignalBooster.Services.Extractors;
using SignalBooster.Services.InputFormats;

namespace SignalBooster.DependencyInjection;

/// <summary>
/// Extension methods for configuring dependency injection.
/// Centralizes service registration following Dependency Inversion Principle.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services required for DME extraction and processing.
    /// </summary>
    public static IServiceCollection AddDmeServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Input format parsers
        services.AddScoped<IInputFormatParser, JsonWrappedParser>();
        services.AddScoped<IInputFormatParser, PlainTextParser>();
        services.AddScoped<InputFormatParserRegistry>();

        // Core services
        services.AddScoped<IFileReader, FileReader>();
        services.AddScoped<IDmeDataSerializer, DmeDataSerializer>();
        services.AddScoped<IDmeProcessingService, DmeProcessingService>();

        // Get extraction method from configuration
        var appSettings = configuration?.GetSection(AppSettings.SectionName).Get<AppSettings>() ?? new AppSettings();
        var extractionMethod = appSettings.ExtractionMethod ?? "Pattern";

        if (extractionMethod.Equals("LLM", StringComparison.OrdinalIgnoreCase))
        {
            // Register LLM-based extractor
            services.AddScoped<IDmeExtractor>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<LlmDmeExtractor>>();
                var llmSettings = appSettings.LlmSettings;

                if (llmSettings == null || string.IsNullOrWhiteSpace(llmSettings.ApiKey))
                {
                    logger.LogWarning("LLM settings not configured, using unconfigured LLM extractor");
                    return new LlmDmeExtractor(logger);
                }

                OpenAIClient client;
                if (llmSettings.UseAzureOpenAi)
                {
                    // Azure OpenAI - requires endpoint
                    if (string.IsNullOrWhiteSpace(llmSettings.Endpoint))
                    {
                        throw new InvalidOperationException("Azure OpenAI requires an endpoint URL");
                    }
                    client = new OpenAIClient(
                        new Uri(llmSettings.Endpoint),
                        new AzureKeyCredential(llmSettings.ApiKey));
                    logger.LogInformation("Using Azure OpenAI: {Endpoint}, Model: {Model}", 
                        llmSettings.Endpoint, llmSettings.ModelName);
                }
                else
                {
                    // OpenAI - can use custom endpoint or default
                    if (!string.IsNullOrWhiteSpace(llmSettings.Endpoint))
                    {
                        client = new OpenAIClient(
                            new Uri(llmSettings.Endpoint),
                            new AzureKeyCredential(llmSettings.ApiKey));
                        logger.LogInformation("Using OpenAI with custom endpoint: {Endpoint}, Model: {Model}", 
                            llmSettings.Endpoint, llmSettings.ModelName);
                    }
                    else
                    {
                        // Use default OpenAI endpoint - OpenAI uses string API key, not AzureKeyCredential
                        client = new OpenAIClient(llmSettings.ApiKey);
                        logger.LogInformation("Using OpenAI (default endpoint), Model: {Model}", llmSettings.ModelName);
                    }
                }

                return new LlmDmeExtractor(client, llmSettings.ModelName, logger);
            });
        }
        else
        {
            // Register pattern-based extractor (default)
            services.AddScoped<ICommonFieldExtractor, CommonFieldExtractor>();
            services.AddScoped<IDeviceSpecificExtractor, CpapExtractor>();
            services.AddScoped<IDeviceSpecificExtractor, OxygenTankExtractor>();
            services.AddScoped<IDeviceExtractorRegistry, DeviceExtractorRegistry>();
            services.AddScoped<IDmeExtractor, DmeExtractor>();
        }

        // HTTP client for API calls
        services.AddHttpClient<IApiClient, ApiClient>();

        return services;
    }
}

