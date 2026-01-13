namespace WebCrawlerConsole.Interfaces;

/// <summary>
/// Interface for fetching URLs from a webpage.
/// Simulates an HTTP request to get all links on a page.
/// </summary>
public interface IHtmlParser
{
    /// <summary>
    /// Get all URLs from a webpage.
    /// This is a BLOCKING call that simulates HTTP request (~15ms).
    /// </summary>
    /// <param name="url">The URL to fetch links from</param>
    /// <returns>List of URLs found on the page</returns>
    List<string> GetUrls(string url);
}
