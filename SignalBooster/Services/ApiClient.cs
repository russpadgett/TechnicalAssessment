using Microsoft.Extensions.Logging;
using System.Text;

namespace SignalBooster.Services;

/// <summary>
/// HTTP client implementation for posting DME extraction results to an external API.
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> PostDmeDataAsync(string jsonData, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogError("Cannot post empty JSON data");
            throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogError("API endpoint cannot be null or empty");
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
        }

        try
        {
            _logger.LogInformation("Posting DME data to endpoint: {Endpoint}", endpoint);
            _logger.LogDebug("Request payload: {Payload}", jsonData);

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted DME data. Status code: {StatusCode}", 
                    response.StatusCode);
                return true;
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API request failed. Status code: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseBody);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while posting to endpoint: {Endpoint}", endpoint);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while posting to endpoint: {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while posting to endpoint: {Endpoint}", endpoint);
            throw;
        }
    }
}

