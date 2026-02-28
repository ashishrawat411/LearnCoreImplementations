using System.Collections.Concurrent;
using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;

namespace AdClickAggregator.Storage;

/// <summary>
/// In-memory aggregation store using ConcurrentDictionary.
/// 
/// This is the "pre-computed materialized view" pattern.
/// 
/// KEY INSIGHT FOR INTERVIEWS:
/// Instead of storing every raw click and counting at query time:
///   SELECT COUNT(*) FROM clicks WHERE ad_id='X' AND ts BETWEEN A AND B  ← O(millions)
/// 
/// We pre-aggregate into time buckets as events arrive:
///   { "ad_123|2024-01-15T14:30": { count: 47, unique_users: 31 } }  ← O(1) lookup
/// 
/// TUMBLING WINDOWS:
/// Time is divided into fixed, non-overlapping windows:
///   [14:30:00 - 14:31:00) [14:31:00 - 14:32:00) [14:32:00 - 14:33:00)
/// 
/// Every click falls into exactly ONE window based on its event timestamp.
/// No double-counting, no gaps. Simple and correct.
/// 
/// REAL-WORLD STORAGE:
/// - Hot data (last hour): In-memory / Redis → fast queries for live dashboard
/// - Warm data (last month): Gorilla time-series DB → compressed, queryable
/// - Cold data (historical): Hive/HDFS → batch analytics, cheap storage
/// </summary>
public class InMemoryAggregationStore : IAggregationStore
{
    // Key: "adId|windowStart" → Value: aggregated counts
    // Why string key? ConcurrentDictionary needs a good hash key.
    // Composite key = adId + window boundary timestamp
    private readonly ConcurrentDictionary<string, AggregatedResult> _minuteAggregations = new();
    private readonly ConcurrentDictionary<string, AggregatedResult> _hourAggregations = new();
    private readonly ConcurrentDictionary<string, AggregatedResult> _dayAggregations = new();

    private readonly ILogger<InMemoryAggregationStore> _logger;

    public InMemoryAggregationStore(ILogger<InMemoryAggregationStore> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Record a click into the correct time window bucket.
    /// This is called on the HOT PATH for every click — must be fast.
    /// 
    /// GetOrAdd + Interlocked gives us thread-safe increment without locks.
    /// </summary>
    public void RecordClick(AdClickEvent clickEvent, WindowSize windowSize)
    {
        var store = GetStore(windowSize);
        var (windowStart, windowEnd) = GetWindowBoundaries(clickEvent.Timestamp, windowSize);
        string key = BuildKey(clickEvent.AdId, windowStart);

        // GetOrAdd is atomic: either gets existing bucket or creates new one
        var aggregation = store.GetOrAdd(key, _ => new AggregatedResult
        {
            AdId = clickEvent.AdId,
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            ClickCount = 0
        });

        // Lock per-bucket to safely increment count and update the user set.
        // In production, you'd use atomic counters (Interlocked on a plain field)
        // and HyperLogLog for approximate unique counts.
        lock (aggregation.UniqueUsers)
        {
            aggregation.ClickCount++;
            aggregation.UniqueUsers.Add(clickEvent.UserId);
            aggregation.LastUpdated = DateTime.UtcNow;
        }

        _logger.LogDebug(
            "Recorded click for ad {AdId} in window {WindowStart} (count: {Count})",
            clickEvent.AdId, windowStart, aggregation.ClickCount);
    }

    /// <summary>
    /// Query aggregated results for an ad within a time range.
    /// Returns pre-computed buckets — no scanning of raw events needed.
    /// </summary>
    public IReadOnlyList<AggregatedResult> Query(
        string adId, DateTime from, DateTime to, WindowSize windowSize)
    {
        var store = GetStore(windowSize);

        return store.Values
            .Where(a => a.AdId == adId && a.WindowStart >= from && a.WindowEnd <= to)
            .OrderBy(a => a.WindowStart)
            .ToList();
    }

    /// <summary>
    /// Get all aggregation data (for the dashboard).
    /// </summary>
    public IReadOnlyList<AggregatedResult> GetAll(WindowSize windowSize)
    {
        var store = GetStore(windowSize);
        return store.Values.OrderByDescending(a => a.WindowStart).ToList();
    }

    /// <summary>
    /// Calculate the window boundaries for a given timestamp.
    /// 
    /// TUMBLING WINDOW LOGIC:
    /// For minute windows: 14:30:45 → window [14:30:00, 14:31:00)
    /// For hour windows:   14:30:45 → window [14:00:00, 15:00:00)
    /// For day windows:    14:30:45 → window [00:00:00, 00:00:00+1day)
    /// 
    /// The key insight: truncate the timestamp to the window boundary.
    /// </summary>
    private static (DateTime start, DateTime end) GetWindowBoundaries(
        DateTime timestamp, WindowSize windowSize)
    {
        return windowSize switch
        {
            WindowSize.OneMinute => (
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc),
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc).AddMinutes(1)
            ),
            WindowSize.OneHour => (
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, 0, 0, DateTimeKind.Utc),
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, 0, 0, DateTimeKind.Utc).AddHours(1)
            ),
            WindowSize.OneDay => (
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    0, 0, 0, DateTimeKind.Utc),
                new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    0, 0, 0, DateTimeKind.Utc).AddDays(1)
            ),
            _ => throw new ArgumentException($"Unknown window size: {windowSize}")
        };
    }

    private static string BuildKey(string adId, DateTime windowStart)
        => $"{adId}|{windowStart:O}";

    private ConcurrentDictionary<string, AggregatedResult> GetStore(WindowSize windowSize)
        => windowSize switch
        {
            WindowSize.OneMinute => _minuteAggregations,
            WindowSize.OneHour => _hourAggregations,
            WindowSize.OneDay => _dayAggregations,
            _ => throw new ArgumentException($"Unknown window size: {windowSize}")
        };
}
