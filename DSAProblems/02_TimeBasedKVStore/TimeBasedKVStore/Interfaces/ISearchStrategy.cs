namespace TimeBasedKVStore

{
    public interface ISearchStrategy
    {
        string GetValueAtTimestamp(SortedList<long, string> values, long timestamp);
    }
}