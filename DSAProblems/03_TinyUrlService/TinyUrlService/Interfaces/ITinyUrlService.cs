namespace TinyUrlService.Interfaces;

/// <summary>
/// Service for creating and managing shortened URLs
/// </summary>
public interface ITinyUrlService
{
    /// <summary>
    /// Creates a short code for the given long URL
    /// </summary>
    /// <param name="longUrl">The original URL to shorten</param>
    /// <returns>The generated short code (not the full URL)</returns>
    string CreateShortUrl(string longUrl);

    /// <summary>
    /// Retrieves the original long URL for a given short code
    /// </summary>
    /// <param name="shortCode">The short code to look up</param>
    /// <returns>The original URL if found, null otherwise</returns>
    string? GetLongUrl(string shortCode);
}
