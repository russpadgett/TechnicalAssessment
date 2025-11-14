using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalBooster.Configuration;
using SignalBooster.DependencyInjection;
using SignalBooster.Services;

namespace SignalBooster;

/// <summary>
/// Main application entry point for the Signal Booster DME extraction tool.
/// Reads physician notes, extracts DME information, and posts it to an external API.
/// Follows Dependency Inversion Principle by using dependency injection.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure services using dependency injection
        var serviceProvider = ConfigureServices();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        logger.LogInformation("Signal Booster DME Extraction Tool starting");

        try
        {
            // Get configuration
            var appSettings = config.GetSection(AppSettings.SectionName).Get<AppSettings>() ?? new AppSettings();
            
            // Determine input file path (command line argument or default)
            var inputFilePath = args.Length > 0 ? args[0] : appSettings.DefaultInputFile;
            var apiEndpoint = args.Length > 1 ? args[1] : appSettings.DefaultApiEndpoint;

            logger.LogInformation("Input file: {InputFile}, API endpoint: {Endpoint}", 
                inputFilePath, apiEndpoint);

            // Get the processing service from DI container
            var processingService = serviceProvider.GetRequiredService<IDmeProcessingService>();

            // Process the physician note
            var success = await processingService.ProcessPhysicianNoteAsync(inputFilePath, apiEndpoint);

            if (success)
            {
                logger.LogInformation("DME extraction and submission completed successfully");
                return 0;
            }
            else
            {
                logger.LogError("Failed to submit DME data to API");
                return 1;
            }
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError(ex, "Input file not found: {FilePath}", ex.FileName);
            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error occurred during DME extraction process");
            return 1;
        }
        finally
        {
            // Dispose of the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Configures the dependency injection container with all required services.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Add logging
        services.AddLogging(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });

        // Add DME services (pass configuration for LLM settings)
        services.AddDmeServices(configuration);

        return services.BuildServiceProvider();
    }
}

