using AdClickAggregator.Models;

namespace AdClickAggregator.Interfaces;

/// <summary>
/// Abstraction for deduplication of click events.
/// 
/// WHY DEDUP?
/// In distributed systems, "at-least-once" delivery is much easier than "exactly-once".
/// So Kafka/Scribe guarantees your event arrives AT LEAST once, but maybe 2-3 times
/// (network retries, consumer rebalancing, etc.).
/// 
/// Without dedup: User clicks once → counted 3 times → advertiser overbilled.
/// 
/// AT META SCALE:
/// - Bloom filters for probabilistic dedup (tiny memory, small false-positive rate)
/// - Redis/Memcached sets for exact dedup within a time window
/// - We use ConcurrentDictionary (exact, but memory-bounded with TTL cleanup)
/// </summary>
public interface IClickDeduplicator
{
    /// <summary>
    /// Check if this click was already seen. If not, mark it as seen.
    /// Returns true if this is a NEW click (should be processed).
    /// Returns false if this is a DUPLICATE (should be skipped).
    /// 
    /// This must be ATOMIC — check-and-set in one operation to avoid races.
    /// </summary>
    bool TryMarkAsSeen(AdClickEvent clickEvent);

    /// <summary>
    /// How many unique clicks we've tracked (for monitoring)
    /// </summary>
    long TrackedCount { get; }
}
