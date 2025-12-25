namespace TimeBasedKVStore
{
    internal class BinarySearchStrategy: ISearchStrategy
    {
        public string GetValueAtTimestamp(SortedList<long, string> values, long timestamp)
        {
            if(values == null || values.Count == 0)
            {
                throw new ArgumentNullException(nameof(values));
            }

            int left = 0;
            int right = values.Count - 1;
            string result = null;

            while(left <= right)
            {
                int mid = left + (right - left) / 2;

                if(values.Keys[mid] == timestamp)
                {
                    return values.Values[mid];
                }
                else if(values.Keys[mid] < timestamp)
                {
                    result = values.Values[mid];
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            
            return result;
        }
    }
    
}
