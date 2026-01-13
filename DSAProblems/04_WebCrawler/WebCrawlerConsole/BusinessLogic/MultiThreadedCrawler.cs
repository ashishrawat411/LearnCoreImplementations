using System.Collections.Concurrent;
using WebCrawlerConsole.Interfaces;

namespace WebCrawlerConsole.BusinessLogic;

/// <summary>
/// Multi-threaded web crawler using Task-based parallelism.
/// Uses ConcurrentDictionary for thread-safe deduplication.
/// 
/// LEARNING GOALS:
/// - Task-based async programming
/// - Thread-safe collections (ConcurrentDictionary)
/// - Race condition prevention
/// - Work coordination (when to stop?)
/// - Semaphore for concurrency limiting
/// </summary>
public class MultiThreadedCrawler : IWebCrawler
{
    private readonly int _maxConcurrency;

    public MultiThreadedCrawler(int maxConcurrency = 10)
    {
        _maxConcurrency = maxConcurrency;
    }

    public async Task<List<string>> CrawlAsync(
        string startUrl, 
        IHtmlParser htmlParser, 
        int? maxDepth = null, 
        int? maxUrls = null)
    {
        // Extract target hostname
        var startUri = new Uri(startUrl);
        string targetHostname = startUri.Host;

        // Thread-safe collection for visited URLs
        var visited = new ConcurrentDictionary<string, bool>();
        visited.TryAdd(startUrl, true);

        // Semaphore to limit concurrent requests
        var semaphore = new SemaphoreSlim(_maxConcurrency);

        // Track active tasks to know when crawling is complete
        int activeTasks = 1;
        var completionSource = new TaskCompletionSource<bool>();

        // Process a single URL recursively
        async Task ProcessUrl(string url)
        {
            // Acquire semaphore slot
            await semaphore.WaitAsync();
            
            try
            {
                // Fetch URLs (run on thread pool to not block)
                var linkedUrls = await Task.Run(() => htmlParser.GetUrls(url));
                
                var hostname = GetHostname(url);
                
                foreach (var linkedUrl in linkedUrls)
                {
                    // Check maxUrls limit
                    if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                        break;

                    // Filter by hostname
                    if (GetHostname(linkedUrl) != hostname)
                        continue;
                    
                    // Try add (atomic - prevents race conditions)
                    if (visited.TryAdd(linkedUrl, true))
                    {
                        // Spawn new task for discovered URL
                        Interlocked.Increment(ref activeTasks);
                        _ = ProcessUrl(linkedUrl);  // Fire and forget
                    }
                }
            }
            finally
            {
                // Release semaphore slot
                semaphore.Release();
                
                // Decrement active task count
                if (Interlocked.Decrement(ref activeTasks) == 0)
                {
                    // Last task done - signal completion
                    completionSource.TrySetResult(true);
                }
            }
        }

        // Start crawling from the initial URL
        _ = ProcessUrl(startUrl);
        
        // Wait for all tasks to complete
        await completionSource.Task;
        
        // Return all discovered URLs
        return visited.Keys.ToList();
    }

    /// <summary>
    /// Helper method to extract hostname from URL
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
