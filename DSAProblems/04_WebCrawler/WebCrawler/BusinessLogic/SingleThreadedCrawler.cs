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
    public async Task<List<string>> CrawlAsync(
        string startUrl, 
        IHtmlParser htmlParser, 
        int? maxDepth = null, 
        int? maxUrls = null)
    {
        // TODO: IMPLEMENT THIS METHOD
        // 
        // HINTS:
        // 1. Extract hostname from startUrl using Uri class
        // 2. Use Queue<string> for BFS (FIFO - First In First Out)
        // 3. Use HashSet<string> to track visited URLs (O(1) lookup)
        // 4. While queue is not empty:
        //    - Dequeue a URL
        //    - Call htmlParser.GetUrls(url) to get linked URLs
        //    - Filter URLs by hostname (only keep same hostname)
        //    - Add unvisited URLs to queue
        //    - Mark URLs as visited
        // 5. Return all visited URLs
        //
        // QUESTIONS TO CONSIDER:
        // - How do you extract hostname from "http://news.yahoo.com/page"?
        // - Why use HashSet instead of List for visited tracking?
        // - What happens if there's a cycle in the graph?
        // - How would you add maxDepth support?

        throw new NotImplementedException("TODO: Implement single-threaded BFS crawler");
    }

    /// <summary>
    /// Helper method to extract hostname from URL
    /// Example: "http://news.yahoo.com/page" -> "news.yahoo.com"
    /// </summary>
    private static string GetHostname(string url)
    {
        // TODO: IMPLEMENT THIS
        // HINT: Use Uri class - new Uri(url).Host
        throw new NotImplementedException();
    }
}
