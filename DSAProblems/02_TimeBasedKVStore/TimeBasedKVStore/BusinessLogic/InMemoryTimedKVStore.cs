using System.Collections.Concurrent;
using TimeBasedKVStore;

namespace TimeBasedKVStore
{
    public class InMemoryTimedKVStore : ITimeBasedKVStore
    {
        private readonly ConcurrentDictionary<string, SortedList<long, string>> cache = new();
        private readonly ISearchStrategy searchStrategy;

        public InMemoryTimedKVStore(ISearchStrategy searchStrategy)
        {
            this.searchStrategy = searchStrategy;
        }
        
        public bool Add(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = cache.TryAdd(key, new SortedList<long, string>() { { DateTime.UtcNow.Ticks, value } });
            return result;
        }

        public bool AddOrUpdate(string key, KeyValuePair<long, string> value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = false;

            if (!cache.ContainsKey(key))
            {
                cache.TryAdd(key, new SortedList<long, string>() { { value.Key, value.Value } });
                result = true;
            }
            else
            {
                cache[key].Add(value.Key, value.Value);
                result = true;
            }

            return result;
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

            SortedList<long, string> values = cache[key];
            return values.Last().Value;
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = cache.TryRemove(key, out _);
            return result;
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

            SortedList<long, string> values = cache[key];

            string result = this.searchStrategy.GetValueAtTimestamp(values, timestamp);

            return result;
        }
    }
}
