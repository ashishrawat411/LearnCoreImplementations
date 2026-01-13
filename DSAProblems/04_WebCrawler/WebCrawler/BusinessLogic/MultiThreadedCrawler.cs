using System.Collections.Concurrent;
using WebCrawler.Interfaces;

namespace WebCrawler.BusinessLogic;

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
        // TODO: IMPLEMENT THIS METHOD
        // 
        // CHALLENGE: How is this different from single-threaded?
        // 
        // KEY DIFFERENCES:
        // 1. Use ConcurrentDictionary instead of HashSet
        //    - Why? Multiple threads will check/add simultaneously
        //    - TryAdd() is atomic - prevents race conditions
        // 
        // 2. Launch tasks for each URL
        //    - Task.Run(() => ProcessUrl(url))
        //    - Don't await immediately - let them run in parallel
        // 
        // 3. Track active tasks
        //    - How do you know when crawling is done?
        //    - Can't just check if queue is empty (new tasks may spawn)
        //    - Need to track: "Are there any pending tasks?"
        // 
        // 4. Limit concurrency
        //    - Use SemaphoreSlim to limit concurrent requests
        //    - Why? Don't overwhelm the server with 1000 simultaneous requests
        //    - await semaphore.WaitAsync() before making request
        //    - semaphore.Release() after request completes
        // 
        // ALGORITHM OUTLINE:
        // 1. Create ConcurrentDictionary<string, bool> for visited URLs
        // 2. Create SemaphoreSlim(maxConcurrency) to limit parallelism
        // 3. Create a method to process one URL:
        //    - Check if already visited (TryAdd)
        //    - Acquire semaphore
        //    - Call htmlParser.GetUrls()
        //    - Release semaphore
        //    - Launch new tasks for discovered URLs
        // 4. Start initial task for startUrl
        // 5. Wait for all tasks to complete
        //    - This is tricky! Tasks spawn new tasks dynamically
        //    - Solution: Track count of active tasks with Interlocked
        // 6. Return all visited URLs
        //
        // QUESTIONS TO CONSIDER:
        // - What's the difference between ConcurrentDictionary.TryAdd vs ContainsKey + Add?
        // - Why can't we use HashSet with lock?
        // - How do we avoid deadlocks?
        // - What happens if maxUrls limit is reached mid-crawl?

        throw new NotImplementedException("TODO: Implement multi-threaded crawler");
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
