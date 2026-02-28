using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;
using AdClickAggregator.Processing;
using Microsoft.AspNetCore.Mvc;

namespace AdClickAggregator.Controllers;

/// <summary>
/// REST API for the Ad Click Aggregator.
/// 
/// Two main responsibilities:
/// 1. INGEST: Accept click events (simulates ad server sending clicks)
/// 2. QUERY: Return aggregated results (simulates advertiser dashboard)
/// 
/// In Meta's architecture:
/// - Ingest would go directly to Scribe/Kafka (not through an API)
/// - Query would be a separate service (read/write separation)
/// - We combine them for simplicity, but discuss the separation in interviews
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdClickController : ControllerBase
{
    private readonly IEventStream _eventStream;
    private readonly IAggregationStore _aggregationStore;
    private readonly IClickDeduplicator _deduplicator;
    private readonly ClickAggregatorProcessor _processor;
    private readonly ILogger<AdClickController> _logger;

    public AdClickController(
        IEventStream eventStream,
        IAggregationStore aggregationStore,
        IClickDeduplicator deduplicator,
        ClickAggregatorProcessor processor,
        ILogger<AdClickController> logger)
    {
        _eventStream = eventStream;
        _aggregationStore = aggregationStore;
        _deduplicator = deduplicator;
        _processor = processor;
        _logger = logger;
    }

    // ==================== INGEST ENDPOINTS ====================

    /// <summary>
    /// Record a single ad click event.
    /// In production: this would be the ad server calling Kafka producer.
    /// Here: we publish to our in-memory Channel (our in-memory Kafka).
    /// </summary>
    [HttpPost("click")]
    public async Task<IActionResult> RecordClick([FromBody] ClickRequest request)
    {
        var clickEvent = new AdClickEvent
        {
            ClickId = request.ClickId ?? Guid.NewGuid().ToString("N"),
            AdId = request.AdId,
            UserId = request.UserId ?? $"user_{Random.Shared.Next(1, 100)}",
            CampaignId = request.CampaignId ?? "default_campaign",
            Timestamp = DateTime.UtcNow
        };

        await _eventStream.PublishAsync(clickEvent);

        return Ok(new
        {
            message = "Click recorded",
            clickId = clickEvent.ClickId,
            adId = clickEvent.AdId,
            pendingInStream = _eventStream.PendingCount
        });
    }

    /// <summary>
    /// Generate a batch of random clicks for testing.
    /// Simulates a burst of ad clicks across multiple ads.
    /// </summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> SimulateClicks([FromBody] SimulateRequest request)
    {
        var adIds = request.AdIds ?? new[] { "ad_meta_feed", "ad_instagram_story", "ad_reels_mid" };
        int count = Math.Min(request.Count, 10000); // Cap at 10k

        for (int i = 0; i < count; i++)
        {
            var adId = adIds[Random.Shared.Next(adIds.Length)];
            var clickEvent = new AdClickEvent
            {
                ClickId = Guid.NewGuid().ToString("N"),
                AdId = adId,
                UserId = $"user_{Random.Shared.Next(1, request.UniqueUsers)}",
                CampaignId = $"campaign_{adId}",
                Timestamp = DateTime.UtcNow
            };

            await _eventStream.PublishAsync(clickEvent);
        }

        return Ok(new
        {
            message = $"Simulated {count} clicks across {adIds.Length} ads",
            adIds,
            pendingInStream = _eventStream.PendingCount
        });
    }

    /// <summary>
    /// Send a duplicate click to demonstrate deduplication.
    /// </summary>
    [HttpPost("simulate-duplicate")]
    public async Task<IActionResult> SimulateDuplicate([FromBody] DuplicateRequest request)
    {
        var clickEvent = new AdClickEvent
        {
            ClickId = request.ClickId, // Same ID = duplicate
            AdId = request.AdId,
            UserId = request.UserId ?? "user_dup_test",
            Timestamp = DateTime.UtcNow
        };

        // Send same event multiple times (simulating network retries)
        for (int i = 0; i < request.Times; i++)
        {
            await _eventStream.PublishAsync(clickEvent);
        }

        return Ok(new
        {
            message = $"Sent the SAME click {request.Times} times (ClickId: {request.ClickId})",
            note = "Only 1 should be counted after deduplication"
        });
    }

    // ==================== QUERY ENDPOINTS ====================

    /// <summary>
    /// Query aggregated click counts for a specific ad.
    /// This is what an advertiser sees on their dashboard.
    /// 
    /// Because we pre-aggregated, this is a simple dictionary lookup — O(1)!
    /// No need to scan through millions of raw click events.
    /// </summary>
    [HttpGet("query/{adId}")]
    public IActionResult QueryAd(
        string adId,
        [FromQuery] WindowSize window = WindowSize.OneMinute,
        [FromQuery] int lastMinutes = 60)
    {
        var from = DateTime.UtcNow.AddMinutes(-lastMinutes);
        var to = DateTime.UtcNow;

        var results = _aggregationStore.Query(adId, from, to, window);

        return Ok(new
        {
            adId,
            window = window.ToString(),
            timeRange = new { from, to },
            totalClicks = results.Sum(r => r.ClickCount),
            totalUniqueUsers = results.SelectMany(r => r.UniqueUsers).Distinct().Count(),
            windows = results.Select(r => new
            {
                windowStart = r.WindowStart,
                windowEnd = r.WindowEnd,
                clicks = r.ClickCount,
                uniqueUsers = r.UniqueUsers.Count,
                lastUpdated = r.LastUpdated
            })
        });
    }

    /// <summary>
    /// Get all aggregation data (overview dashboard).
    /// </summary>
    [HttpGet("dashboard")]
    public IActionResult Dashboard([FromQuery] WindowSize window = WindowSize.OneMinute)
    {
        var results = _aggregationStore.GetAll(window);

        return Ok(new
        {
            window = window.ToString(),
            totalAds = results.Select(r => r.AdId).Distinct().Count(),
            totalClicksAllAds = results.Sum(r => r.ClickCount),
            ads = results
                .GroupBy(r => r.AdId)
                .Select(g => new
                {
                    adId = g.Key,
                    totalClicks = g.Sum(r => r.ClickCount),
                    totalUniqueUsers = g.SelectMany(r => r.UniqueUsers).Distinct().Count(),
                    windows = g.OrderByDescending(r => r.WindowStart).Select(r => new
                    {
                        windowStart = r.WindowStart,
                        clicks = r.ClickCount,
                        uniqueUsers = r.UniqueUsers.Count
                    })
                })
        });
    }

    /// <summary>
    /// System health/stats endpoint. Shows processing pipeline metrics.
    /// In production, these metrics would go to a monitoring system (Prometheus/ODS at Meta).
    /// </summary>
    [HttpGet("stats")]
    public IActionResult Stats()
    {
        return Ok(new
        {
            pipeline = new
            {
                pendingInStream = _eventStream.PendingCount,
                totalProcessed = _processor.TotalProcessed,
                totalDeduplicated = _processor.TotalDeduplicated,
                deduplicatorTracking = _deduplicator.TrackedCount
            },
            aggregations = new
            {
                minuteWindows = _aggregationStore.GetAll(WindowSize.OneMinute).Count,
                hourWindows = _aggregationStore.GetAll(WindowSize.OneHour).Count,
                dayWindows = _aggregationStore.GetAll(WindowSize.OneDay).Count
            }
        });
    }
}

// ==================== REQUEST DTOs ====================

public record ClickRequest
{
    public required string AdId { get; init; }
    public string? ClickId { get; init; }
    public string? UserId { get; init; }
    public string? CampaignId { get; init; }
}

public record SimulateRequest
{
    public int Count { get; init; } = 100;
    public int UniqueUsers { get; init; } = 50;
    public string[]? AdIds { get; init; }
}

public record DuplicateRequest
{
    public required string ClickId { get; init; }
    public required string AdId { get; init; }
    public string? UserId { get; init; }
    public int Times { get; init; } = 3;
}
