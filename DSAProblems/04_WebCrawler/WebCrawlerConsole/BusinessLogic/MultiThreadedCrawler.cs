using System.Collections.Concurrent;
using WebCrawlerConsole.Interfaces;

namespace WebCrawlerConsole.BusinessLogic;

/// <summary>
/// Multi-threaded web crawler using iterative Producer-Consumer pattern.
/// NO RECURSION - uses a work queue (BlockingCollection) instead.
/// 
/// LEARNING GOALS:
/// - Producer-Consumer pattern with BlockingCollection
/// - Thread-safe collections (ConcurrentDictionary, BlockingCollection)
/// - Worker pool pattern (fixed number of consumers)
/// - Graceful shutdown with CancellationToken
/// - No recursion = predictable stack usage
/// 
/// PATTERN COMPARISON:
/// - Recursive: Each URL spawns a new Task (unbounded task creation)
/// - Iterative: Fixed workers pull from a shared queue (bounded, predictable)
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
        // Extract target hostname for filtering
        var startUri = new Uri(startUrl);
        string targetHostname = startUri.Host;

        // Thread-safe set of visited URLs (also serves as our result)
        var visited = new ConcurrentDictionary<string, bool>();
        visited.TryAdd(startUrl, true);

        // Work queue: URLs waiting to be processed
        // BlockingCollection wraps ConcurrentQueue and provides blocking Take()
        using var workQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        workQueue.Add(startUrl);

        // Track how many URLs are currently being processed
        // When this hits 0 AND queue is empty, we're done
        int activeWorkers = 0;

        // Cancellation for graceful shutdown
        using var cts = new CancellationTokenSource();

        // Create worker tasks (fixed pool of consumers)
        var workers = Enumerable.Range(0, _maxConcurrency)
            .Select(_ => Task.Run(() => WorkerLoop(
                workQueue,
                visited,
                htmlParser,
                targetHostname,
                maxUrls,
                ref activeWorkers,
                cts.Token)))
            .ToArray();

        // Wait for all workers to complete
        await Task.WhenAll(workers);

        return visited.Keys.ToList();
    }

    /// <summary>
    /// Worker loop - each worker pulls URLs from the queue and processes them.
    /// This is the "Consumer" in Producer-Consumer pattern.
    /// </summary>
    private void WorkerLoop(
        BlockingCollection<string> workQueue,
        ConcurrentDictionary<string, bool> visited,
        IHtmlParser htmlParser,
        string targetHostname,
        int? maxUrls,
        ref int activeWorkers,
        CancellationToken cancellationToken)
    {
        // Spin-wait timeout for checking if work is done
        const int timeoutMs = 50;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Try to get work from the queue (with timeout)
            if (!workQueue.TryTake(out string? url, timeoutMs))
            {
                // No work available - check if we should exit
                // Exit if: no active workers AND queue is empty
                if (Volatile.Read(ref activeWorkers) == 0 && workQueue.Count == 0)
                {
                    // Mark queue as complete to signal other workers
                    workQueue.CompleteAdding();
                    break;
                }
                continue; // Keep waiting for work
            }

            // Got work - increment active count
            Interlocked.Increment(ref activeWorkers);

            try
            {
                // Check maxUrls limit before fetching
                if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                {
                    workQueue.CompleteAdding();
                    break;
                }

                // Fetch linked URLs (this is the slow I/O operation)
                List<string> linkedUrls = htmlParser.GetUrls(url);

                // Process discovered URLs (Producer side - add work to queue)
                foreach (string linkedUrl in linkedUrls)
                {
                    // Check limit again
                    if (maxUrls.HasValue && visited.Count >= maxUrls.Value)
                        break;

                    // Filter: same hostname only
                    if (GetHostname(linkedUrl) != targetHostname)
                        continue;

                    // TryAdd returns true only if this is a NEW url
                    // This is atomic - prevents duplicate processing
                    if (visited.TryAdd(linkedUrl, true))
                    {
                        // Add to work queue for processing
                        if (!workQueue.IsAddingCompleted)
                        {
                            workQueue.Add(linkedUrl);
                        }
                    }
                }
            }
            finally
            {
                // Done with this URL - decrement active count
                Interlocked.Decrement(ref activeWorkers);
            }
        }
    }

    /// <summary>
    /// Extract hostname from URL safely
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
