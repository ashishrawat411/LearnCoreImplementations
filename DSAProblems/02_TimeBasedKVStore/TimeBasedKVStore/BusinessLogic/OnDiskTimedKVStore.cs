
using System.Collections.Concurrent;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    public class OnDiskTimedKVStore : ITimeBasedKVStore
    {
        private readonly string filePath;
        private readonly ISearchStrategy searchStrategy;
        private readonly ISerializer<ConcurrentDictionary<string, SortedList<long, string>>> serializer;
        private ConcurrentDictionary<string, SortedList<long, string>> cache;
        
        // Per-key locks to protect SortedList modifications
        private readonly ConcurrentDictionary<string, object> keyLocks = new();
        
        // Lock for file serialization to prevent concurrent writes
        private readonly object fileLock = new();

        public OnDiskTimedKVStore(ISearchStrategy searchStrategy,
            ISerializer<ConcurrentDictionary<string, SortedList<long, string>>> serializer,
            string filePath = "D:\\Personal\\Projects\\LearnCoreImplementations\\DSAProblems\\02_TimeBasedKVStore\\diskCache.json")
        {
            this.filePath = filePath;
            this.searchStrategy = searchStrategy;
            this.serializer = serializer;

            this.EnsureFileExists();
            this.DeserializeCache();
        }

        public bool Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdate(string key, KeyValuePair<long, string> value)
        {
            // Get or create lock for this specific key
            var lockObj = keyLocks.GetOrAdd(key, _ => new object());
            
            // Lock per key (not global) - allows concurrent updates to different keys
            lock (lockObj)
            {
                // AddOrUpdate is atomic for dictionary operations
                // But we need lock to protect SortedList modifications
                this.cache.AddOrUpdate(
                    key,
                    (k) => new SortedList<long, string> { { value.Key, value.Value } },
                    (k, values) =>
                    {
                        // Use indexer for add-or-update behavior (not Add which throws on duplicate)
                        values[value.Key] = value.Value;
                        return values;
                    });

                // Lock file writes to prevent concurrent serialization corruption
                lock (fileLock)
                {
                    this.serializer.Serialize(this.cache);
                }
            }

            return true;
        }

        public string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!cache.ContainsKey(key))
            {
                throw new KeyNotFoundException($"The given key '{key}' was not present in the store.");
            }

            SortedList<long, string> values = this.cache[key];
            return values.Last().Value;
        }

        public string GetValueAtTimestamp(string key, long timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!cache.ContainsKey(key))
            {
                throw new KeyNotFoundException($"key {key} does not exist in the store.");
            }

            SortedList<long, string> values = this.cache[key];

            string result = this.searchStrategy.GetValueAtTimestamp(values, timestamp);

            return result;
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(this.filePath))
            {
                File.Create(this.filePath);
            }
        }

        private void DeserializeCache()
        {
            if (!File.Exists(this.filePath))
            {
                throw new FileNotFoundException();
            }

            if (this.cache == null)
            {
                cache = serializer.Deserialize();
            }
        }
    }
}
