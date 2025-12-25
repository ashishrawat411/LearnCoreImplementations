
namespace TimeBasedKVStore.BusinessLogic
{
    public class OnDiskTimedKVStore : ITimeBasedKVStore
    {
        public bool Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdate(string key, KeyValuePair<long, string> value)
        {
            throw new NotImplementedException();
        }

        public string Get(string key)
        {
            throw new NotImplementedException();
        }

        public string GetValueAtTimestamp(string key, long timestamp)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }
    }
}
