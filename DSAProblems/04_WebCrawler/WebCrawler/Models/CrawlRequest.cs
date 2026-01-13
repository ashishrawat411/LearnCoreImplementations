namespace WebCrawler.Models;

/// <summary>
/// Request model for initiating a web crawl
/// </summary>
public record CrawlRequest
{
    /// <summary>
    /// The starting URL to begin crawling from
    /// </summary>
    public string StartUrl { get; init; } = string.Empty;

    /// <summary>
    /// Maximum depth to crawl (null = unlimited)
    /// </summary>
    public int? MaxDepth { get; init; }

    /// <summary>
    /// Maximum number of URLs to discover (null = unlimited)
    /// </summary>
    public int? MaxUrls { get; init; }

    /// <summary>
    /// Whether to use multi-threading (true) or single-threaded (false)
    /// </summary>
    public bool UseMultiThreading { get; init; } = true;

    /// <summary>
    /// Maximum number of concurrent requests (for multi-threaded crawler)
    /// </summary>
    public int MaxConcurrency { get; init; } = 10;

    /// <summary>
    /// Test scenario name (for using mock data)
    /// Options: "example1", "example2", "custom"
    /// </summary>
    public string? TestScenario { get; init; }
}
