using System.Collections.Concurrent;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore
{
    public class InMemoryTimedKVStore : ITimeBasedKVStore
    {
        private readonly ConcurrentDictionary<string, SortedList<long, string>> cache = new();

        bool ITimeBasedKVStore.Add(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = cache.TryAdd(key, new SortedList<long, string>() { { DateTime.UtcNow.Ticks, value } });
            return result;
        }

        bool ITimeBasedKVStore.AddOrUpdate(string key, KeyValuePair<long, string> value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = false;

            if (!cache.ContainsKey(key))
            {
                cache.TryAdd(key, new SortedList<long, string>() { {value.Key, value.Value } });
                result = true;
            }
            else
            {
                cache[key].Add(value.Key, value.Value);
                result = true;
            }

            return result;
        }

        string ITimeBasedKVStore.Get(string key)
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

        bool ITimeBasedKVStore.Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool result = cache.TryRemove(key, out _);
            return result;
        }
    }
}
