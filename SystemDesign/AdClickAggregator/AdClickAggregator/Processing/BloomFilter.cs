using System.Collections;

namespace AdClickAggregator.Processing;

/// <summary>
/// A basic Bloom filter implementation for learning purposes.
/// 
/// WHAT IS THIS?
/// A space-efficient probabilistic data structure that answers:
///   "Have I seen this item before?"
/// With two possible answers:
///   - "DEFINITELY NO"  (100% accurate — no false negatives)
///   - "PROBABLY YES"   (might be wrong — false positive rate ~0.1%)
/// 
/// HOW IT WORKS:
/// 1. Maintain a bit array of size m (all bits start as 0)
/// 2. Use k independent hash functions
/// 3. To ADD an item: compute k hashes, set those k bits to 1
/// 4. To CHECK an item: compute k hashes, check if ALL k bits are 1
///    - If any bit is 0 → DEFINITELY not in the set
///    - If all bits are 1 → PROBABLY in the set (could be collision)
/// 
/// WHY META USES THIS FOR AD CLICK DEDUP:
/// - 1 billion click IDs in HashSet = ~40 GB RAM
/// - 1 billion click IDs in Bloom filter = ~1.7 GB RAM
/// - The ~0.1% false positive rate means ~1 in 1000 real clicks gets
///   wrongly skipped. At $0.50 CPC, that's $0.0005 lost per 1000 clicks.
///   The RAM savings are worth millions more.
/// </summary>
public class BloomFilter
{
    // The bit array — this IS the Bloom filter's storage
    // BitArray is .NET's built-in compact bit storage (1 bit per element)
    private readonly BitArray _bits;

    // How many bits in our array
    private readonly int _size;

    // How many different hash functions we use per item
    private readonly int _hashCount;

    // How many items have been added (for monitoring fill rate)
    private long _itemCount;

    /// <summary>
    /// Create a Bloom filter sized for the expected number of items
    /// and desired false positive rate.
    /// 
    /// The math:
    ///   m (bits)    = -(n * ln(p)) / (ln(2))^2
    ///   k (hashes)  = (m / n) * ln(2)
    /// 
    /// Example: 1,000,000 items, 0.1% false positive rate
    ///   m = ~14.4 million bits = ~1.8 MB
    ///   k = 10 hash functions
    /// </summary>
    /// <param name="expectedItems">How many items you expect to add</param>
    /// <param name="falsePositiveRate">Desired false positive rate (0.001 = 0.1%)</param>
    public BloomFilter(int expectedItems, double falsePositiveRate = 0.001)
    {
        // Calculate optimal bit array size
        // m = -(n * ln(p)) / (ln(2))^2
        _size = CalculateOptimalSize(expectedItems, falsePositiveRate);

        // Calculate optimal number of hash functions  
        // k = (m / n) * ln(2)
        _hashCount = CalculateOptimalHashCount(_size, expectedItems);

        _bits = new BitArray(_size);
        _itemCount = 0;
    }

    /// <summary>
    /// Add an item to the Bloom filter.
    /// 
    /// Process:
    /// 1. Compute k different hash values for the item
    /// 2. Set the bit at each hash position to 1
    /// 
    /// Note: You can NEVER remove an item from a basic Bloom filter!
    /// Setting a bit to 0 would affect OTHER items that share that bit.
    /// (Counting Bloom filters solve this with counters instead of bits)
    /// </summary>
    public void Add(string item)
    {
        var hashes = GetHashes(item);

        foreach (int hash in hashes)
        {
            _bits[hash] = true; // Set bit to 1
        }

        Interlocked.Increment(ref _itemCount);
    }

    /// <summary>
    /// Check if an item MIGHT be in the set.
    /// 
    /// Returns:
    ///   false = DEFINITELY NOT in the set (all clear, 100% certain)
    ///   true  = PROBABLY in the set (could be false positive)
    /// 
    /// Process:
    /// 1. Compute k different hash values for the item
    /// 2. Check if ALL k bits are 1
    /// 3. If ANY bit is 0 → definitely not in set (early exit)
    /// 4. If ALL bits are 1 → probably in set
    /// </summary>
    public bool MightContain(string item)
    {
        var hashes = GetHashes(item);

        foreach (int hash in hashes)
        {
            if (!_bits[hash]) // If ANY bit is 0, item is definitely not in the set
            {
                return false; // DEFINITELY NOT HERE — 100% certain
            }
        }

        return true; // All bits are 1 — PROBABLY here (might be false positive)
    }

    /// <summary>
    /// Convenience: Check + Add atomically.
    /// Returns true if item is NEW (not seen before).
    /// Returns false if item was ALREADY SEEN (or false positive).
    /// 
    /// This is exactly what the deduplicator needs:
    ///   if (bloom.TryAdd("click_abc"))  → process it (first time)
    ///   if (!bloom.TryAdd("click_abc")) → skip it (duplicate)
    /// </summary>
    public bool TryAdd(string item)
    {
        // IMPORTANT: This is NOT truly atomic. In production, you'd need
        // synchronization or use a thread-safe Bloom filter library.
        // For learning, this demonstrates the concept.
        if (MightContain(item))
        {
            return false; // Already seen (or false positive)
        }

        Add(item);
        return true; // New item, added successfully
    }

    // ==================== STATS (for monitoring/dashboard) ====================

    public long ItemCount => Interlocked.Read(ref _itemCount);
    public int BitArraySize => _size;
    public int HashFunctionCount => _hashCount;

    /// <summary>
    /// What percentage of bits are set to 1.
    /// As this approaches 100%, false positive rate increases dramatically.
    /// In production, you'd create a new Bloom filter or start rejecting writes.
    /// </summary>
    public double FillRatio
    {
        get
        {
            int setBits = 0;
            for (int i = 0; i < _bits.Length; i++)
            {
                if (_bits[i]) setBits++;
            }
            return (double)setBits / _size;
        }
    }

    /// <summary>
    /// Estimated current false positive rate based on fill level.
    /// Formula: (setBits / totalBits) ^ hashCount
    /// </summary>
    public double EstimatedFalsePositiveRate
        => Math.Pow(FillRatio, _hashCount);

    // ==================== HASH FUNCTIONS ====================

    /// <summary>
    /// Generate k different hash values for an item.
    /// 
    /// TECHNIQUE: Double hashing
    /// Instead of k independent hash functions (expensive), we use:
    ///   hash_i(item) = (hash1 + i * hash2) % m
    /// 
    /// This is proven to be as good as k independent hashes for Bloom filters.
    /// See: Kirsch & Mitzenmacher, "Less Hashing, Same Performance"
    /// 
    /// hash1 = first 32 bits of a hash
    /// hash2 = second 32 bits (or a different hash)
    /// </summary>
    private int[] GetHashes(string item)
    {
        // Get two independent hash values
        int hash1 = GetHash1(item);
        int hash2 = GetHash2(item);

        var hashes = new int[_hashCount];

        for (int i = 0; i < _hashCount; i++)
        {
            // Combine: hash_i = (hash1 + i * hash2) mod m
            // Math.Abs handles negative values from int overflow
            hashes[i] = Math.Abs((hash1 + i * hash2) % _size);
        }

        return hashes;
    }

    /// <summary>
    /// First hash function — uses .NET's string hash with a salt.
    /// In production, you'd use MurmurHash3 or xxHash for better distribution.
    /// </summary>
    private static int GetHash1(string item)
    {
        // Simple DJB2 hash — fast, reasonable distribution
        unchecked
        {
            int hash = 5381;
            foreach (char c in item)
            {
                hash = ((hash << 5) + hash) + c; // hash * 33 + c
            }
            return hash & int.MaxValue; // Ensure positive
        }
    }

    /// <summary>
    /// Second hash function — must be INDEPENDENT from the first.
    /// Uses FNV-1a hash.
    /// </summary>
    private static int GetHash2(string item)
    {
        // FNV-1a hash — good distribution, different from DJB2
        unchecked
        {
            int hash = unchecked((int)2166136261);
            foreach (char c in item)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return hash & int.MaxValue; // Ensure positive
        }
    }

    // ==================== OPTIMAL SIZE CALCULATIONS ====================

    /// <summary>
    /// Calculate optimal bit array size.
    /// m = -(n * ln(p)) / (ln(2))^2
    /// </summary>
    private static int CalculateOptimalSize(int expectedItems, double falsePositiveRate)
    {
        double m = -(expectedItems * Math.Log(falsePositiveRate)) / Math.Pow(Math.Log(2), 2);
        return (int)Math.Ceiling(m);
    }

    /// <summary>
    /// Calculate optimal number of hash functions.
    /// k = (m / n) * ln(2)
    /// </summary>
    private static int CalculateOptimalHashCount(int bitArraySize, int expectedItems)
    {
        double k = ((double)bitArraySize / expectedItems) * Math.Log(2);
        return Math.Max(1, (int)Math.Round(k));
    }
}
