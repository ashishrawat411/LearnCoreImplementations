using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebCrawler.BusinessLogic;
using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CrawlerController : ControllerBase
{
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(ILogger<CrawlerController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Crawl a website starting from the given URL
    /// </summary>
    /// <param name="request">Crawl configuration</param>
    /// <returns>Crawl results with statistics</returns>
    [HttpPost("crawl")]
    [ProducesResponseType(typeof(CrawlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crawl([FromBody] CrawlRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.StartUrl))
            {
                return BadRequest(new { error = "StartUrl is required" });
            }

            if (!Uri.TryCreate(request.StartUrl, UriKind.Absolute, out var uri))
            {
                return BadRequest(new { error = "Invalid URL format" });
            }

            _logger.LogInformation(
                "Starting crawl: {StartUrl}, MultiThreading: {UseMultiThreading}, MaxConcurrency: {MaxConcurrency}",
                request.StartUrl, request.UseMultiThreading, request.MaxConcurrency);

            // Create HTML parser (mock or real)
            IHtmlParser htmlParser = string.IsNullOrEmpty(request.TestScenario)
                ? throw new NotImplementedException("Real HTML parser not implemented yet. Use TestScenario.")
                : MockHtmlParser.CreateFromScenario(request.TestScenario);

            // Select crawler implementation
            IWebCrawler crawler = request.UseMultiThreading
                ? new MultiThreadedCrawler(request.MaxConcurrency)
                : new SingleThreadedCrawler();

            // Start timer
            var stopwatch = Stopwatch.StartNew();

            // Perform crawl
            var crawledUrls = await crawler.CrawlAsync(
                request.StartUrl,
                htmlParser,
                request.MaxDepth,
                request.MaxUrls);

            stopwatch.Stop();

            _logger.LogInformation(
                "Crawl completed: {UrlCount} URLs in {ElapsedMs}ms",
                crawledUrls.Count, stopwatch.ElapsedMilliseconds);

            // Build response
            var response = new CrawlResponse
            {
                CrawledUrls = crawledUrls,
                TotalUrls = crawledUrls.Count,
                TimeElapsedMs = stopwatch.ElapsedMilliseconds,
                UsedMultiThreading = request.UseMultiThreading,
                ThreadsUsed = request.UseMultiThreading ? request.MaxConcurrency : 1,
                StartUrl = request.StartUrl,
                Hostname = uri.Host
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during crawl");
            return StatusCode(500, new { error = "An error occurred during crawling" });
        }
    }

    /// <summary>
    /// Compare single-threaded vs multi-threaded performance
    /// </summary>
    [HttpPost("benchmark")]
    [ProducesResponseType(typeof(BenchmarkResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Benchmark([FromBody] CrawlRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StartUrl))
            {
                return BadRequest(new { error = "StartUrl is required" });
            }

            IHtmlParser htmlParser = string.IsNullOrEmpty(request.TestScenario)
                ? throw new NotImplementedException("Real HTML parser not implemented yet. Use TestScenario.")
                : MockHtmlParser.CreateFromScenario(request.TestScenario);

            _logger.LogInformation("Running benchmark for {StartUrl}", request.StartUrl);

            // Run single-threaded
            var singleThreaded = new SingleThreadedCrawler();
            var swSingle = Stopwatch.StartNew();
            var resultsSingle = await singleThreaded.CrawlAsync(request.StartUrl, htmlParser, request.MaxDepth, request.MaxUrls);
            swSingle.Stop();

            // Run multi-threaded
            var multiThreaded = new MultiThreadedCrawler(request.MaxConcurrency);
            var swMulti = Stopwatch.StartNew();
            var resultsMulti = await multiThreaded.CrawlAsync(request.StartUrl, htmlParser, request.MaxDepth, request.MaxUrls);
            swMulti.Stop();

            var speedup = (double)swSingle.ElapsedMilliseconds / swMulti.ElapsedMilliseconds;

            return Ok(new BenchmarkResponse
            {
                StartUrl = request.StartUrl,
                SingleThreadedTimeMs = swSingle.ElapsedMilliseconds,
                MultiThreadedTimeMs = swMulti.ElapsedMilliseconds,
                SpeedupFactor = speedup,
                UrlsCrawled = resultsSingle.Count,
                ThreadsUsed = request.MaxConcurrency
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during benchmark");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public record BenchmarkResponse
{
    public string StartUrl { get; init; } = string.Empty;
    public long SingleThreadedTimeMs { get; init; }
    public long MultiThreadedTimeMs { get; init; }
    public double SpeedupFactor { get; init; }
    public int UrlsCrawled { get; init; }
    public int ThreadsUsed { get; init; }
}
