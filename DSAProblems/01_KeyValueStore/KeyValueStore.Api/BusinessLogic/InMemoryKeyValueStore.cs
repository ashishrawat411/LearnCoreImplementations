using System.Collections.Concurrent;

namespace KeyValueStore.Api
{
    public class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly ConcurrentDictionary<string, string> cache = new();

        public bool Delete(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            
            if (!cache.ContainsKey(key))
            {
                return false;
            }

            return cache.Remove(key, out string? val);
        }

        public string? Get(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            cache.TryGetValue(key, out string? value);
            return value;
        }

        public bool Set(string key, string value)
        {
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (cache.ContainsKey(key))
            {
                cache[key] = value;
                return true;
            }
            else
            {
                return cache.TryAdd(key, value);
            }
        }

        KeyValuePair<string, string>[] IKeyValueStore.List()
        {
            return cache.ToArray();
        }
    }
}