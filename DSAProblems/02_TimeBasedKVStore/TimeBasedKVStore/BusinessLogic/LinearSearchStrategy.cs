
namespace TimeBasedKVStore.BusinessLogic
{
    public class LinearSearchStrategy : ISearchStrategy
    {
        public string GetValueAtTimestamp(SortedList<long, string> values, long timestamp)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            string result = string.Empty;

            foreach (var ts in values)
            {
                if(ts.Key <= timestamp)
                {
                    result = ts.Value;
                }
            }

            return result;
        }
    }
}
