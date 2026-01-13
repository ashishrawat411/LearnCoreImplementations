namespace WebCrawler.Models;

/// <summary>
/// Response model containing crawl results and statistics
/// </summary>
public record CrawlResponse
{
    /// <summary>
    /// All URLs discovered during the crawl
    /// </summary>
    public List<string> CrawledUrls { get; init; } = new();

    /// <summary>
    /// Total number of URLs discovered
    /// </summary>
    public int TotalUrls { get; init; }

    /// <summary>
    /// Time elapsed in milliseconds
    /// </summary>
    public long TimeElapsedMs { get; init; }

    /// <summary>
    /// Whether multi-threading was used
    /// </summary>
    public bool UsedMultiThreading { get; init; }

    /// <summary>
    /// Number of threads/tasks used
    /// </summary>
    public int ThreadsUsed { get; init; }

    /// <summary>
    /// Starting URL
    /// </summary>
    public string StartUrl { get; init; } = string.Empty;

    /// <summary>
    /// Hostname that was crawled
    /// </summary>
    public string Hostname { get; init; } = string.Empty;
}
