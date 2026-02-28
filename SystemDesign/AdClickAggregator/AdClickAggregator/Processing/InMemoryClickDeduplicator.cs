using System.Collections.Concurrent;
using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;

namespace AdClickAggregator.Processing;

/// <summary>
/// In-memory click deduplicator using ConcurrentDictionary.
/// 
/// WHY THIS EXISTS:
/// Network is unreliable. A click event might be sent 2-3 times due to:
/// - Client retries (user's browser re-sends on timeout)
/// - Kafka consumer rebalance (re-processes some events)
/// - Producer retries (at-least-once delivery guarantee)
/// 
/// REAL-WORLD AT META:
/// - Bloom filters: ~1% false positive rate, but uses only ~10 bits per element
///   False positive = we SKIP a real click (tiny revenue loss, acceptable)
///   False negative = IMPOSSIBLE (we never double-count)
/// - Redis SET with TTL: Exact dedup, but needs network call per check
/// - Our approach: ConcurrentDictionary with periodic cleanup (exact, in-memory)
/// 
/// INTERVIEW DISCUSSION POINT:
/// "I'd use a Bloom filter for the hot path (nanosecond lookups) with a 
///  time-window of 5 minutes. After 5 minutes, duplicate clicks are ignored
///  at the stream level anyway since we use event-time windowing."
/// </summary>
public class InMemoryClickDeduplicator : IClickDeduplicator
{
    // ClickId -> Timestamp when first seen
    // ConcurrentDictionary.TryAdd is atomic: check + insert in one operation
    private readonly ConcurrentDictionary<string, DateTime> _seenClicks = new();
    private readonly ILogger<InMemoryClickDeduplicator> _logger;

    // How long to remember click IDs (old ones can be cleaned up)
    // In production, this would match your maximum event-time skew
    private readonly TimeSpan _deduplicationWindow = TimeSpan.FromMinutes(10);
    private DateTime _lastCleanup = DateTime.UtcNow;

    public InMemoryClickDeduplicator(ILogger<InMemoryClickDeduplicator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Atomically check if click was seen and mark it if new.
    /// TryAdd returns true ONLY if the key didn't exist → thread-safe dedup.
    /// 
    /// This is the critical insight: TryAdd is a single atomic operation.
    /// Two threads calling TryAdd("click_123") simultaneously:
    ///   Thread A: TryAdd → true  (wins, processes the click)
    ///   Thread B: TryAdd → false (loses, skips the duplicate)
    /// No locks needed!
    /// </summary>
    public bool TryMarkAsSeen(AdClickEvent clickEvent)
    {
        // Periodic cleanup of old click IDs to prevent memory growth
        CleanupIfNeeded();

        bool isNew = _seenClicks.TryAdd(clickEvent.ClickId, clickEvent.Timestamp);

        if (!isNew)
        {
            _logger.LogDebug("Duplicate click detected: {ClickId}", clickEvent.ClickId);
        }

        return isNew;
    }

    public long TrackedCount => _seenClicks.Count;

    /// <summary>
    /// Remove click IDs older than the dedup window.
    /// In production, you'd use a TTL-based cache (Redis SETEX) instead.
    /// </summary>
    private void CleanupIfNeeded()
    {
        if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromMinutes(1))
            return;

        _lastCleanup = DateTime.UtcNow;
        var cutoff = DateTime.UtcNow - _deduplicationWindow;
        int removed = 0;

        foreach (var kvp in _seenClicks)
        {
            if (kvp.Value < cutoff)
            {
                _seenClicks.TryRemove(kvp.Key, out _);
                removed++;
            }
        }

        if (removed > 0)
        {
            _logger.LogInformation("Dedup cleanup: removed {Count} expired click IDs", removed);
        }
    }
}
