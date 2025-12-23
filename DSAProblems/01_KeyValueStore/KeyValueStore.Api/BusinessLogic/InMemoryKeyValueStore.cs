namespace KeyValueStore.Api
{
    public class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, string> cache = new();
        public void Delete(string key)
        {
            if (!cache.ContainsKey(key))
            {
                return;
            }
            
            cache.Remove(key);
        }

        public string? Get(string key)
        {
            cache.TryGetValue(key, out string? value);
            return value;
        }

        public void Set(string key, string value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key] = value;
            }
            else
            {
                cache.Add(key, value);
            }
        }

        KeyValuePair<string, string>[] IKeyValueStore.List()
        {
            return cache.ToArray();
        }
    }
}