namespace WebCrawlerConsole.Interfaces;

/// <summary>
/// Interface for web crawler implementations.
/// Can be single-threaded or multi-threaded.
/// </summary>
public interface IWebCrawler
{
    /// <summary>
    /// Crawl all URLs under the same hostname as startUrl.
    /// </summary>
    /// <param name="startUrl">The starting URL</param>
    /// <param name="htmlParser">Parser to fetch URLs from pages</param>
    /// <param name="maxDepth">Maximum depth to crawl (optional)</param>
    /// <param name="maxUrls">Maximum URLs to discover (optional)</param>
    /// <returns>List of all discovered URLs under the same hostname</returns>
    Task<List<string>> CrawlAsync(
        string startUrl, 
        IHtmlParser htmlParser, 
        int? maxDepth = null, 
        int? maxUrls = null);
}
