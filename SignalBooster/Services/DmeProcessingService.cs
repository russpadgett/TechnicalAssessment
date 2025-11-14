using Microsoft.Extensions.Logging;
using SignalBooster.Models;

namespace SignalBooster.Services;

/// <summary>
/// Orchestrates the complete DME extraction and submission workflow.
/// Coordinates file reading, extraction, serialization, and API submission.
/// </summary>
public class DmeProcessingService : IDmeProcessingService
{
    private readonly IFileReader _fileReader;
    private readonly IDmeExtractor _dmeExtractor;
    private readonly IDmeDataSerializer _serializer;
    private readonly IApiClient _apiClient;
    private readonly ILogger<DmeProcessingService> _logger;

    public DmeProcessingService(
        IFileReader fileReader,
        IDmeExtractor dmeExtractor,
        IDmeDataSerializer serializer,
        IApiClient apiClient,
        ILogger<DmeProcessingService> logger)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _dmeExtractor = dmeExtractor ?? throw new ArgumentNullException(nameof(dmeExtractor));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ProcessPhysicianNoteAsync(string inputFilePath, string apiEndpoint)
    {
        try
        {
            _logger.LogInformation("Starting DME processing workflow. File: {FilePath}, Endpoint: {Endpoint}", 
                inputFilePath, apiEndpoint);

            // Step 1: Read the physician note
            string noteText;
            try
            {
                noteText = await _fileReader.ReadFileAsync(inputFilePath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Input file not found: {FilePath}", inputFilePath);
                throw;
            }

            // Step 2: Extract DME information
            var extractionResult = _dmeExtractor.ExtractDmeInfo(noteText);

            // Step 3: Serialize to JSON
            var jsonData = _serializer.SerializeToJson(extractionResult);
            
            // Log the extracted JSON for debugging/verification
            _logger.LogInformation("Extracted DME data (JSON):\n{JsonData}", jsonData);
            Console.WriteLine("\n=== Extracted DME Data ===");
            Console.WriteLine(jsonData);
            Console.WriteLine("==========================\n");

            // Step 4: Post to API
            var success = await _apiClient.PostDmeDataAsync(jsonData, apiEndpoint);

            if (success)
            {
                _logger.LogInformation("DME processing completed successfully");
            }
            else
            {
                _logger.LogError("DME processing completed but API submission failed");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DME processing workflow");
            throw;
        }
    }
}

