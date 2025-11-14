namespace SignalBooster.Services;

/// <summary>
/// Client for sending DME extraction results to an external API.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Posts the extracted DME data to the external API.
    /// </summary>
    /// <param name="jsonData">The JSON string containing the extracted DME data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <returns>True if the request was successful, false otherwise.</returns>
    Task<bool> PostDmeDataAsync(string jsonData, string endpoint);
}

