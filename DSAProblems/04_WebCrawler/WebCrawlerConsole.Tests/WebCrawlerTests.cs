using Moq;
using WebCrawlerConsole.BusinessLogic;
using WebCrawlerConsole.Interfaces;

namespace WebCrawlerConsole.Tests;

/// <summary>
/// TESTING PATTERNS FOR WEB CRAWLER - Using Moq Framework
/// 
/// KEY TESTING CONCEPTS:
/// 1. AAA Pattern: Arrange → Act → Assert
/// 2. Test naming: MethodName_Scenario_ExpectedBehavior
/// 3. Moq for mocking IHtmlParser - flexible, verifiable mocks
/// 4. Setup() - define mock behavior
/// 5. Verify() - assert mock was called correctly
/// 6. It.IsAny<T>() - match any argument of type T
/// 7. Callback() - execute custom code when mock is called
/// 
/// WHY MOQ OVER HAND-WRITTEN MOCKS?
/// - Less boilerplate code
/// - Verify interactions (was GetUrls called? how many times?)
/// - Flexible argument matching
/// - Setup different behaviors per input
/// </summary>
public class WebCrawlerTests
{
    #region === HELPER: Moq Setup Utilities ===
    
    /// <summary>
    /// Creates a Mock<IHtmlParser> configured with a URL graph.
    /// Each URL maps to its list of linked URLs.
    /// 
    /// MOQ PATTERN: Setup() defines what the mock returns for given inputs.
    /// </summary>
    private static Mock<IHtmlParser> CreateMockParser(Dictionary<string, List<string>> graph, int delayMs = 0)
    {
        var mock = new Mock<IHtmlParser>();
        
        // Setup: For each URL in graph, return its linked URLs
        foreach (var kvp in graph)
        {
            mock.Setup(p => p.GetUrls(kvp.Key))
                .Returns(() =>
                {
                    if (delayMs > 0) Thread.Sleep(delayMs); // Simulate network latency
                    return kvp.Value;
                });
        }
        
        // Default: Unknown URLs return empty list (prevents null reference)
        mock.Setup(p => p.GetUrls(It.IsAny<string>()))
            .Returns(new List<string>());
        
        // Re-apply specific setups (they take precedence over generic)
        foreach (var kvp in graph)
        {
            mock.Setup(p => p.GetUrls(kvp.Key))
                .Returns(() =>
                {
                    if (delayMs > 0) Thread.Sleep(delayMs);
                    return kvp.Value;
                });
        }
        
        return mock;
    }
    
    #endregion

    #region === SINGLE-THREADED CRAWLER TESTS ===

    [Fact]
    public async Task SingleThreaded_BasicCrawl_DiscoversAllUrls()
    {
        // ARRANGE: Set up mock with URL graph
        var graph = new Dictionary<string, List<string>>
        {
            ["http://example.com"] = new() { "http://example.com/page1", "http://example.com/page2" },
            ["http://example.com/page1"] = new() { "http://example.com/page3" },
            ["http://example.com/page2"] = new List<string>(),
            ["http://example.com/page3"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new SingleThreadedCrawler();

        // ACT: Execute the method under test
        var result = await crawler.CrawlAsync("http://example.com", mockParser.Object);

        // ASSERT: Verify expected behavior
        Assert.Equal(4, result.Count);
        Assert.Contains("http://example.com", result);
        Assert.Contains("http://example.com/page1", result);
        Assert.Contains("http://example.com/page2", result);
        Assert.Contains("http://example.com/page3", result);
        
        // VERIFY: Mock was called for each URL exactly once
        mockParser.Verify(p => p.GetUrls("http://example.com"), Times.Once);
        mockParser.Verify(p => p.GetUrls("http://example.com/page1"), Times.Once);
    }

    [Fact]
    public async Task SingleThreaded_FiltersExternalUrls_OnlySameHostname()
    {
        // ARRANGE: Graph with external URLs
        var graph = new Dictionary<string, List<string>>
        {
            ["http://mysite.com"] = new() 
            { 
                "http://mysite.com/about",
                "http://google.com",           // External - should be filtered
                "http://other.com/page"        // External - should be filtered
            },
            ["http://mysite.com/about"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new SingleThreadedCrawler();

        // ACT
        var result = await crawler.CrawlAsync("http://mysite.com", mockParser.Object);

        // ASSERT: Only same-hostname URLs
        Assert.Equal(2, result.Count);
        Assert.Contains("http://mysite.com", result);
        Assert.Contains("http://mysite.com/about", result);
        Assert.DoesNotContain("http://google.com", result);
        Assert.DoesNotContain("http://other.com/page", result);
        
        // VERIFY: External URLs were never fetched (filtered before GetUrls)
        mockParser.Verify(p => p.GetUrls("http://google.com"), Times.Never);
        mockParser.Verify(p => p.GetUrls("http://other.com/page"), Times.Never);
    }

    [Fact]
    public async Task SingleThreaded_HandlesCycles_NoDuplicates()
    {
        // ARRANGE: Graph with cycles (A -> B -> C -> A)
        var graph = new Dictionary<string, List<string>>
        {
            ["http://cycle.com/a"] = new() { "http://cycle.com/b" },
            ["http://cycle.com/b"] = new() { "http://cycle.com/c" },
            ["http://cycle.com/c"] = new() { "http://cycle.com/a" }  // Cycle back!
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new SingleThreadedCrawler();

        // ACT
        var result = await crawler.CrawlAsync("http://cycle.com/a", mockParser.Object);

        // ASSERT: Each URL visited exactly once
        Assert.Equal(3, result.Count);
        Assert.Equal(result.Distinct().Count(), result.Count); // No duplicates
        
        // VERIFY: Each URL fetched exactly once (cycle detection works)
        mockParser.Verify(p => p.GetUrls("http://cycle.com/a"), Times.Once);
        mockParser.Verify(p => p.GetUrls("http://cycle.com/b"), Times.Once);
        mockParser.Verify(p => p.GetUrls("http://cycle.com/c"), Times.Once);
    }

    [Fact]
    public async Task SingleThreaded_MaxUrlsLimit_StopsAtLimit()
    {
        // ARRANGE: Large graph
        var graph = new Dictionary<string, List<string>>
        {
            ["http://big.com"] = new() { "http://big.com/1", "http://big.com/2", "http://big.com/3" },
            ["http://big.com/1"] = new() { "http://big.com/4", "http://big.com/5" },
            ["http://big.com/2"] = new List<string>(),
            ["http://big.com/3"] = new List<string>(),
            ["http://big.com/4"] = new List<string>(),
            ["http://big.com/5"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new SingleThreadedCrawler();

        // ACT: Limit to 3 URLs
        var result = await crawler.CrawlAsync("http://big.com", mockParser.Object, maxUrls: 3);

        // ASSERT: Respects limit
        Assert.True(result.Count <= 3);
    }

    [Fact]
    public async Task SingleThreaded_StartUrlOnly_ReturnsStartUrl()
    {
        // ARRANGE: Using Moq inline setup (alternative pattern)
        var mockParser = new Mock<IHtmlParser>();
        mockParser.Setup(p => p.GetUrls("http://lonely.com"))
                  .Returns(new List<string>()); // No outgoing links
        
        var crawler = new SingleThreadedCrawler();

        // ACT
        var result = await crawler.CrawlAsync("http://lonely.com", mockParser.Object);

        // ASSERT
        Assert.Single(result);
        Assert.Contains("http://lonely.com", result);
        
        // VERIFY: GetUrls was called exactly once
        mockParser.Verify(p => p.GetUrls(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region === MULTI-THREADED CRAWLER TESTS ===

    [Fact]
    public async Task MultiThreaded_BasicCrawl_DiscoversAllUrls()
    {
        // ARRANGE
        var graph = new Dictionary<string, List<string>>
        {
            ["http://example.com"] = new() { "http://example.com/page1", "http://example.com/page2" },
            ["http://example.com/page1"] = new() { "http://example.com/page3" },
            ["http://example.com/page2"] = new List<string>(),
            ["http://example.com/page3"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: 4);

        // ACT
        var result = await crawler.CrawlAsync("http://example.com", mockParser.Object);

        // ASSERT: Same results as single-threaded (correctness)
        Assert.Equal(4, result.Count);
        Assert.Contains("http://example.com", result);
        Assert.Contains("http://example.com/page1", result);
        Assert.Contains("http://example.com/page2", result);
        Assert.Contains("http://example.com/page3", result);
    }

    [Fact]
    public async Task MultiThreaded_HandlesCycles_NoDuplicates()
    {
        // ARRANGE: Cyclic graph - critical test for multi-threading!
        var graph = new Dictionary<string, List<string>>
        {
            ["http://cycle.com/a"] = new() { "http://cycle.com/b", "http://cycle.com/c" },
            ["http://cycle.com/b"] = new() { "http://cycle.com/a", "http://cycle.com/c" },
            ["http://cycle.com/c"] = new() { "http://cycle.com/a", "http://cycle.com/b" }
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: 3);

        // ACT
        var result = await crawler.CrawlAsync("http://cycle.com/a", mockParser.Object);

        // ASSERT: No duplicates even with concurrent access
        Assert.Equal(3, result.Count);
        Assert.Equal(result.Distinct().Count(), result.Count);
    }

    [Fact]
    public async Task MultiThreaded_FiltersExternalUrls_OnlySameHostname()
    {
        // ARRANGE
        var graph = new Dictionary<string, List<string>>
        {
            ["http://mysite.com"] = new() 
            { 
                "http://mysite.com/about",
                "http://external.com/spam"
            },
            ["http://mysite.com/about"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: 2);

        // ACT
        var result = await crawler.CrawlAsync("http://mysite.com", mockParser.Object);

        // ASSERT
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain("http://external.com/spam", result);
    }

    [Fact]
    public async Task MultiThreaded_MaxUrlsLimit_RespectsLimit()
    {
        // ARRANGE
        var graph = new Dictionary<string, List<string>>
        {
            ["http://big.com"] = new() { "http://big.com/1", "http://big.com/2", "http://big.com/3" },
            ["http://big.com/1"] = new() { "http://big.com/4" },
            ["http://big.com/2"] = new() { "http://big.com/5" },
            ["http://big.com/3"] = new List<string>(),
            ["http://big.com/4"] = new List<string>(),
            ["http://big.com/5"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: 4);

        // ACT
        var result = await crawler.CrawlAsync("http://big.com", mockParser.Object, maxUrls: 3);

        // ASSERT
        Assert.True(result.Count <= 3);
    }

    #endregion

    #region === CONCURRENCY TESTS (Multi-threaded specific) ===

    [Fact]
    public async Task MultiThreaded_HighConcurrency_NoRaceConditions()
    {
        // ARRANGE: Many URLs to stress test thread safety
        var graph = new Dictionary<string, List<string>>
        {
            ["http://stress.com"] = Enumerable.Range(1, 20)
                .Select(i => $"http://stress.com/page{i}")
                .ToList()
        };
        
        // Add empty pages for each link
        for (int i = 1; i <= 20; i++)
        {
            graph[$"http://stress.com/page{i}"] = new List<string>();
        }
        
        var mockParser = CreateMockParser(graph, delayMs: 5); // Small delay to encourage race conditions
        var crawler = new MultiThreadedCrawler(maxConcurrency: 10);

        // ACT
        var result = await crawler.CrawlAsync("http://stress.com", mockParser.Object);

        // ASSERT: All URLs found, no duplicates
        Assert.Equal(21, result.Count); // 1 start + 20 pages
        Assert.Equal(result.Distinct().Count(), result.Count);
    }

    [Fact]
    public async Task MultiThreaded_IsFasterThanSingleThreaded()
    {
        // ARRANGE: Graph with parallel branches
        var graph = new Dictionary<string, List<string>>
        {
            ["http://parallel.com"] = new() 
            { 
                "http://parallel.com/a",
                "http://parallel.com/b",
                "http://parallel.com/c"
            },
            ["http://parallel.com/a"] = new List<string>(),
            ["http://parallel.com/b"] = new List<string>(),
            ["http://parallel.com/c"] = new List<string>()
        };
        var delayMs = 50;
        
        var singleMock = CreateMockParser(graph, delayMs);
        var multiMock = CreateMockParser(graph, delayMs);
        
        var singleCrawler = new SingleThreadedCrawler();
        var multiCrawler = new MultiThreadedCrawler(maxConcurrency: 4);

        // ACT: Time both implementations
        var swSingle = System.Diagnostics.Stopwatch.StartNew();
        await singleCrawler.CrawlAsync("http://parallel.com", singleMock.Object);
        swSingle.Stop();

        var swMulti = System.Diagnostics.Stopwatch.StartNew();
        await multiCrawler.CrawlAsync("http://parallel.com", multiMock.Object);
        swMulti.Stop();

        // ASSERT: Multi-threaded should be faster
        // 4 URLs × 50ms = 200ms single-threaded
        // With parallelism, should be ~100ms or less
        Assert.True(swMulti.ElapsedMilliseconds < swSingle.ElapsedMilliseconds,
            $"Multi: {swMulti.ElapsedMilliseconds}ms, Single: {swSingle.ElapsedMilliseconds}ms");
    }

    #endregion

    #region === PARAMETERIZED TESTS (Theory + InlineData) ===

    /// <summary>
    /// Theory allows running the same test with different inputs.
    /// Great for testing multiple scenarios without code duplication.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task MultiThreaded_VariousConcurrencyLevels_AllSucceed(int concurrency)
    {
        // ARRANGE
        var graph = new Dictionary<string, List<string>>
        {
            ["http://test.com"] = new() { "http://test.com/a", "http://test.com/b" },
            ["http://test.com/a"] = new List<string>(),
            ["http://test.com/b"] = new List<string>()
        };
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: concurrency);

        // ACT
        var result = await crawler.CrawlAsync("http://test.com", mockParser.Object);

        // ASSERT
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region === MOQ ADVANCED PATTERNS ===

    [Fact]
    public async Task SingleThreaded_VerifyCallOrder_UsingCallback()
    {
        // ARRANGE: Track call order using Callback()
        var callOrder = new List<string>();
        
        var mockParser = new Mock<IHtmlParser>();
        mockParser.Setup(p => p.GetUrls("http://order.com"))
                  .Callback(() => callOrder.Add("start"))
                  .Returns(new List<string> { "http://order.com/a" });
        mockParser.Setup(p => p.GetUrls("http://order.com/a"))
                  .Callback(() => callOrder.Add("a"))
                  .Returns(new List<string>());
        
        var crawler = new SingleThreadedCrawler();

        // ACT
        await crawler.CrawlAsync("http://order.com", mockParser.Object);

        // ASSERT: BFS processes start URL first, then /a
        Assert.Equal(new[] { "start", "a" }, callOrder);
    }

    [Fact]
    public async Task MultiThreaded_CountTotalCalls_UsingVerify()
    {
        // ARRANGE
        var mockParser = new Mock<IHtmlParser>();
        mockParser.Setup(p => p.GetUrls(It.IsAny<string>()))
                  .Returns(new List<string>());
        mockParser.Setup(p => p.GetUrls("http://count.com"))
                  .Returns(new List<string> { "http://count.com/1", "http://count.com/2" });
        mockParser.Setup(p => p.GetUrls("http://count.com/1"))
                  .Returns(new List<string>());
        mockParser.Setup(p => p.GetUrls("http://count.com/2"))
                  .Returns(new List<string>());
        
        var crawler = new MultiThreadedCrawler(maxConcurrency: 2);

        // ACT
        await crawler.CrawlAsync("http://count.com", mockParser.Object);

        // VERIFY: GetUrls called exactly 3 times total
        mockParser.Verify(p => p.GetUrls(It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SingleThreaded_ThrowsOnError_MockException()
    {
        // ARRANGE: Setup mock to throw exception
        var mockParser = new Mock<IHtmlParser>();
        mockParser.Setup(p => p.GetUrls("http://error.com"))
                  .Throws(new HttpRequestException("Network error"));
        
        var crawler = new SingleThreadedCrawler();

        // ACT & ASSERT: Verify exception propagates
        await Assert.ThrowsAsync<HttpRequestException>(
            () => crawler.CrawlAsync("http://error.com", mockParser.Object));
    }

    #endregion

    #region === LEETCODE EXAMPLE TESTS ===

    [Fact]
    public async Task LeetCode_Example1_Yahoo()
    {
        // ARRANGE: From the LeetCode problem - using Moq
        var graph = new Dictionary<string, List<string>>
        {
            ["http://news.yahoo.com/news/topics/"] = new() { "http://news.yahoo.com", "http://news.yahoo.com/news" },
            ["http://news.yahoo.com"] = new() { "http://news.yahoo.com/us" },
            ["http://news.yahoo.com/news"] = new() { "http://news.google.com", "http://news.yahoo.com/news/topics/" },
            ["http://news.google.com"] = new() { "http://news.yahoo.com" },
            ["http://news.yahoo.com/us"] = new List<string>()
        };
        
        var mockParser = CreateMockParser(graph);
        var crawler = new MultiThreadedCrawler(maxConcurrency: 4);

        // ACT
        var result = await crawler.CrawlAsync("http://news.yahoo.com/news/topics/", mockParser.Object);

        // ASSERT: Should find exactly 4 yahoo URLs
        Assert.Equal(4, result.Count);
        Assert.Contains("http://news.yahoo.com/news/topics/", result);
        Assert.Contains("http://news.yahoo.com", result);
        Assert.Contains("http://news.yahoo.com/news", result);
        Assert.Contains("http://news.yahoo.com/us", result);
        
        // No google URLs (different hostname)
        Assert.DoesNotContain("http://news.google.com", result);
    }

    [Fact]
    public async Task LeetCode_Example2_Google_OnlyStartUrl()
    {
        // ARRANGE: Starting from google.com, all links are to yahoo
        var mockParser = new Mock<IHtmlParser>();
        mockParser.Setup(p => p.GetUrls("http://news.google.com"))
                  .Returns(new List<string> { "http://news.yahoo.com", "http://news.yahoo.com/news" });
        
        var crawler = new MultiThreadedCrawler(maxConcurrency: 4);

        // ACT
        var result = await crawler.CrawlAsync("http://news.google.com", mockParser.Object);

        // ASSERT: Only the start URL (yahoo links filtered out)
        Assert.Single(result);
        Assert.Contains("http://news.google.com", result);
        
        // VERIFY: Only start URL was fetched
        mockParser.Verify(p => p.GetUrls("http://news.google.com"), Times.Once);
        mockParser.Verify(p => p.GetUrls("http://news.yahoo.com"), Times.Never);
    }

    #endregion
}
