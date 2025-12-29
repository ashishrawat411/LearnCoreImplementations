
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
            bool result = false;
            try
            {
                // AddOrUpdate is atomic and thread-safe
                // Parameters:
                //   key: the dictionary key
                //   addValueFactory: called when key doesn't exist - creates new SortedList with our timestamp-value
                //   updateValueFactory: called when key exists - adds timestamp-value to existing SortedList
                this.cache.AddOrUpdate(
                    key,
                    (k) => new SortedList<long, string> { { value.Key, value.Value } },
                    (k, values) =>
                    {
                        values.Add(value.Key, value.Value);
                        return values;
                    });

                // Persist to disk after in-memory update
                this.serializer.Serialize(this.cache);
                result = true;
            }
            catch 
            {
                throw;
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
