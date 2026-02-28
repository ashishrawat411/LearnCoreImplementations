using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;

namespace AdClickAggregator.Processing;

/// <summary>
/// Bloom filter-based deduplicator — the META PRODUCTION approach.
/// 
/// SWAP THIS IN by changing ONE line in Program.cs:
///   Before: builder.Services.AddSingleton&lt;IClickDeduplicator, InMemoryClickDeduplicator&gt;();
///   After:  builder.Services.AddSingleton&lt;IClickDeduplicator, BloomFilterDeduplicator&gt;();
/// 
/// COMPARISON:
/// ┌──────────────────────┬──────────────────────┬──────────────────────┐
/// │                      │ InMemoryDeduplicator  │ BloomFilterDedup     │
/// │                      │ (ConcurrentDictionary)│ (Bloom Filter)       │
/// ├──────────────────────┼──────────────────────┼──────────────────────┤
/// │ Memory for 1M items  │ ~50 MB               │ ~1.8 MB              │
/// │ Memory for 1B items  │ ~50 GB               │ ~1.7 GB              │
/// │ Accuracy             │ 100% exact           │ ~99.9% (0.1% FP)    │
/// │ Can remove items?    │ Yes (TryRemove)      │ No (bits are shared) │
/// │ Thread safety        │ Built-in (Conc.Dict) │ Needs synchronization│
/// │ Used at Meta?        │ No (too much memory) │ YES                  │
/// └──────────────────────┴──────────────────────┴──────────────────────┘
/// </summary>
public class BloomFilterDeduplicator : IClickDeduplicator
{
    private readonly BloomFilter _bloomFilter;
    private readonly ILogger<BloomFilterDeduplicator> _logger;
    private long _trackedCount;

    // Lock for thread safety (BitArray is not thread-safe)
    // In production, you'd use a partitioned Bloom filter (one per thread)
    // to avoid contention. Or use a lock-free CAS-based implementation.
    private readonly object _lock = new();

    public BloomFilterDeduplicator(ILogger<BloomFilterDeduplicator> logger)
    {
        _logger = logger;

        // Size for 1 million expected clicks with 0.1% false positive rate
        // In production, you'd configure this based on expected traffic
        _bloomFilter = new BloomFilter(
            expectedItems: 1_000_000,
            falsePositiveRate: 0.001 // 0.1%
        );

        _logger.LogInformation(
            "Bloom Filter Deduplicator initialized: {Bits} bits ({MB:F1} MB), {Hashes} hash functions",
            _bloomFilter.BitArraySize,
            _bloomFilter.BitArraySize / 8.0 / 1024 / 1024,
            _bloomFilter.HashFunctionCount);
    }

    /// <summary>
    /// Check if this click is new using the Bloom filter.
    /// 
    /// Returns true  = NEW click (Bloom says "definitely not seen") → process it
    /// Returns false = DUPLICATE or false positive (Bloom says "probably seen") → skip it
    /// 
    /// THE FALSE POSITIVE TRADE-OFF:
    /// ~0.1% of genuinely new clicks will be incorrectly identified as duplicates.
    /// At $0.50 CPC (cost per click), processing 1M clicks:
    ///   - 1000 clicks wrongly skipped (0.1% of 1M)
    ///   - Revenue lost: 1000 × $0.50 = $500
    ///   - Memory saved: ~48 MB (vs ConcurrentDictionary)
    /// At Meta's scale (billions of clicks), the memory savings justify this.
    /// </summary>
    public bool TryMarkAsSeen(AdClickEvent clickEvent)
    {
        bool isNew;

        lock (_lock) // BitArray is not thread-safe
        {
            isNew = _bloomFilter.TryAdd(clickEvent.ClickId);
        }

        if (isNew)
        {
            Interlocked.Increment(ref _trackedCount);
        }
        else
        {
            _logger.LogDebug(
                "Bloom filter: duplicate (or false positive) for click {ClickId}",
                clickEvent.ClickId);
        }

        return isNew;
    }

    public long TrackedCount => Interlocked.Read(ref _trackedCount);

    /// <summary>
    /// Bloom filter stats — useful for the dashboard / monitoring.
    /// Watch the fill ratio! As it approaches 1.0, false positive rate skyrockets.
    /// </summary>
    public double FillRatio => _bloomFilter.FillRatio;
    public double EstimatedFalsePositiveRate => _bloomFilter.EstimatedFalsePositiveRate;
    public int BitArraySizeBytes => _bloomFilter.BitArraySize / 8;
}
