namespace AdClickAggregator.Models;

/// <summary>
/// Represents a single ad click event — this is what arrives from the ad server.
/// In the real world, this comes from the user's browser/device when they click an ad.
/// 
/// INTERVIEW TIP: At Meta scale, billions of these flow through Scribe (Kafka-like)
/// partitioned by AdId so all clicks for the same ad land on the same processor.
/// </summary>
public record AdClickEvent
{
    /// <summary>
    /// Globally unique click identifier — used for DEDUPLICATION.
    /// Why? Network retries can send the same click twice. Without dedup,
    /// advertisers get double-billed and metrics are inflated.
    /// </summary>
    public required string ClickId { get; init; }

    /// <summary>
    /// Which ad was clicked. This is the PARTITION KEY in real systems.
    /// All clicks for ad "ad_123" go to the same stream partition → same processor.
    /// This means per-ad aggregation needs ZERO coordination between processors.
    /// </summary>
    public required string AdId { get; init; }

    /// <summary>
    /// Which user clicked. Used for fraud detection (same user clicking 1000 times).
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// When the click happened (UTC). This is the EVENT TIME, not processing time.
    /// The distinction matters: a click at 11:59:59 might arrive at 12:00:01.
    /// We use event time for correct windowing.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Which campaign this ad belongs to. Useful for higher-level aggregation.
    /// Advertisers create campaigns containing multiple ads.
    /// </summary>
    public string CampaignId { get; init; } = string.Empty;
}

/// <summary>
/// The result of aggregating clicks in a time window.
/// Stored pre-computed so queries are O(1) lookups, not full scans.
/// 
/// INTERVIEW TIP: This is the "materialized view" pattern.
/// Instead of: SELECT COUNT(*) FROM clicks WHERE ad_id = X AND time BETWEEN A AND B
/// We pre-compute: { ad_123, minute_2024_01_15_14_30, count: 47 }
/// </summary>
public record AggregatedResult
{
    /// <summary>
    /// Which ad these counts are for
    /// </summary>
    public required string AdId { get; init; }

    /// <summary>
    /// The start of the time window (e.g., 14:30:00 for minute-level)
    /// </summary>
    public required DateTime WindowStart { get; init; }

    /// <summary>
    /// The end of the time window (e.g., 14:31:00 for minute-level)
    /// </summary>
    public required DateTime WindowEnd { get; init; }

    /// <summary>
    /// Total clicks in this window
    /// </summary>
    public long ClickCount { get; set; }

    /// <summary>
    /// Unique users who clicked (for fraud detection / unique reach)
    /// </summary>
    public HashSet<string> UniqueUsers { get; init; } = new();

    /// <summary>
    /// Last time this aggregation was updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the size of aggregation windows.
/// Real systems pre-aggregate at multiple granularities simultaneously.
/// Minute data rolls up to hour data rolls up to day data.
/// </summary>
public enum WindowSize
{
    OneMinute,
    OneHour,
    OneDay
}
