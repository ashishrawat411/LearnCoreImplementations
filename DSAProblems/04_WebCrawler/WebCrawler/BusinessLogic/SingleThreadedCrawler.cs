using WebCrawler.Interfaces;

namespace WebCrawler.BusinessLogic;

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
    public Task<List<string>> CrawlAsync(
        string startUrl, 
        IHtmlParser htmlParser, 
        int? maxDepth = null, 
        int? maxUrls = null)
    {
        // Extract hostname from start URL
        var targetHostname = GetHostname(startUrl);
        if (string.IsNullOrEmpty(targetHostname))
        {
            return Task.FromResult(new List<string>());
        }

        // Initialize data structures for BFS
        var visited = new HashSet<string>();
        var queue = new Queue<(string url, int depth)>();
        
        // Start BFS with the initial URL at depth 0
        queue.Enqueue((startUrl, 0));
        visited.Add(startUrl);

        // BFS traversal
        while (queue.Count > 0)
        {
            // Check if we've reached the max URLs limit
            if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
            {
                break;
            }

            var (currentUrl, currentDepth) = queue.Dequeue();

            // Check if we've reached the max depth limit
            if (maxDepth.HasValue && currentDepth >= maxDepth.Value)
            {
                continue;
            }

            // Get all URLs from the current page
            // Note: This is a blocking call, but we're single-threaded so it's fine
            var linkedUrls = htmlParser.GetUrls(currentUrl);

            // Process each linked URL
            foreach (var linkedUrl in linkedUrls)
            {
                // Check max URLs limit before processing
                if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                {
                    break;
                }

                // Skip if already visited
                if (visited.Contains(linkedUrl))
                {
                    continue;
                }

                // Filter by hostname - only crawl URLs with the same hostname
                var linkedHostname = GetHostname(linkedUrl);
                if (linkedHostname != targetHostname)
                {
                    continue;
                }

                // Add to visited and queue
                visited.Add(linkedUrl);
                queue.Enqueue((linkedUrl, currentDepth + 1));
            }
        }

        // Return all visited URLs as a list
        return Task.FromResult(visited.ToList());
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
        catch (UriFormatException)
        {
            return string.Empty;
        }
        catch (ArgumentNullException)
        {
            return string.Empty;
        }
    }
}
