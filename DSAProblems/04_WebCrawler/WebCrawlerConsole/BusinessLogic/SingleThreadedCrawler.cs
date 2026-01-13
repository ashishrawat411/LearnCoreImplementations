using WebCrawlerConsole.Interfaces;

namespace WebCrawlerConsole.BusinessLogic;

/// <summary>
/// Single-threaded web crawler using BFS (Breadth-First Search).
/// This is the baseline implementation to understand the algorithm.
/// 
/// LEARNING GOALS:
/// - Understand BFS traversal
/// - URL hostname extraction
/// - Deduplication with HashSet
/// - Establish performance baseline
/// </summary>
public class SingleThreadedCrawler : IWebCrawler
{
    public async Task<List<string>> CrawlAsync(
        string startUrl, 
        IHtmlParser htmlParser, 
        int? maxDepth = null, 
        int? maxUrls = null)
    {
        // Extract hostname from start URL
        var startUri = new Uri(startUrl);
        string targetHostname = startUri.Host;

        // Initialize BFS data structures
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        
        // Start with the initial URL
        queue.Enqueue(startUrl);
        visited.Add(startUrl);

        // BFS traversal
        while (queue.Count > 0)
        {
            // Check maxUrls limit
            if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                break;

            string currentUrl = queue.Dequeue();
            
            // Fetch linked URLs from current page (blocking call)
            List<string> linkedUrls = htmlParser.GetUrls(currentUrl);
            
            foreach (string linkedUrl in linkedUrls)
            {
                // Check maxUrls limit
                if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                    break;

                // Extract hostname from linked URL
                if (!Uri.TryCreate(linkedUrl, UriKind.Absolute, out var linkedUri))
                    continue;
                
                // Only process URLs from the same hostname
                if (linkedUri.Host != targetHostname)
                    continue;
                
                // Add unvisited URLs to the queue
                if (!visited.Contains(linkedUrl))
                {
                    visited.Add(linkedUrl);
                    queue.Enqueue(linkedUrl);
                }
            }
        }

        // Return all discovered URLs
        return await Task.FromResult(visited.ToList());
    }

    /// <summary>
    /// Helper method to extract hostname from URL
    /// Example: "http://news.yahoo.com/page" -> "news.yahoo.com"
    /// </summary>
    private static string GetHostname(string url)
    {
        try
        {
            return new Uri(url).Host;
        }
        catch
        {
            return string.Empty;
        }
    }
}
