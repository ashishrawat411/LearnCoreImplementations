using AdClickAggregator.Models;

namespace AdClickAggregator.Interfaces;

/// <summary>
/// Abstraction for the aggregation data store.
/// 
/// In Meta's real architecture, this would be:
/// - Gorilla (time-series DB) for recent hot data
/// - Hive/Spark for historical cold data
/// - TAO for social graph + cached aggregates
/// 
/// We use ConcurrentDictionary, but the INTERFACE is what matters for the interview.
/// You should be able to explain what backing store you'd use and why.
/// </summary>
public interface IAggregationStore
{
    /// <summary>
    /// Record a click into the appropriate time window.
    /// This is the HOT PATH — called for every single click event.
    /// Must be thread-safe and fast (no locks if possible).
    /// </summary>
    void RecordClick(AdClickEvent clickEvent, WindowSize windowSize);

    /// <summary>
    /// Query aggregated results for an ad within a time range.
    /// This is what the advertiser sees on their dashboard.
    /// Because we pre-aggregate, this is O(number of windows) not O(number of clicks).
    /// </summary>
    IReadOnlyList<AggregatedResult> Query(string adId, DateTime from, DateTime to, WindowSize windowSize);

    /// <summary>
    /// Get all aggregation results (for debugging / dashboard).
    /// </summary>
    IReadOnlyList<AggregatedResult> GetAll(WindowSize windowSize);
}
