using System.Collections.Concurrent;
using System.Text.Json;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    public class MyJsonSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;
        public MyJsonSerializer(string _filepath)
        {
            if (!File.Exists(_filepath))
            {
                throw new FileNotFoundException(_filepath);
            }

            this.filePath = _filepath;
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> value)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            // Serialize to string (synchronous)
            string jsonString = JsonSerializer.Serialize(value, options);

            // Write to file (synchronous)
            File.WriteAllText(this.filePath, jsonString);
        }

        ConcurrentDictionary<string, SortedList<long, string>> ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>.Deserialize()
        {
            // Read JSON from file
            string jsonContent = File.ReadAllText(this.filePath);
            
            // Handle empty file
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return new ConcurrentDictionary<string, SortedList<long, string>>();
            }
            
            // Deserialize
            var result = JsonSerializer.Deserialize<ConcurrentDictionary<string, SortedList<long, string>>>(jsonContent);
            
            // Handle null result (invalid JSON or literal "null")
            return result ?? new ConcurrentDictionary<string, SortedList<long, string>>();
        }
    }
}
