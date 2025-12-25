namespace TimeBasedKVStore
{
    public interface ITimeBasedKVStore
    {
        bool Add(string key, string value);

        bool Remove(string key);

        string Get(string key);

        bool AddOrUpdate(string key, KeyValuePair<long, string> value);

        string GetValueAtTimestamp(string key, long timestamp);
    }
}