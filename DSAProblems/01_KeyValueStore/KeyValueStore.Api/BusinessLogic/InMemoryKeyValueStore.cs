namespace KeyValueStore.Api
{
    public class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, string> _store = new Dictionary<string, string>();
        public void Delete(string key)
        {
            _store.Remove(key);
        }

        public string? Get(string key)
        {
            _store.TryGetValue(key, out string? value);
            return value;
        }

        public void Set(string key, string value)
        {
            _store.Add(key, value);
        }

        string?[] IKeyValueStore.List()
        {
            return _store.Values.ToArray();
        }
    }
}