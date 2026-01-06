using MessagePack;
using System.Collections.Concurrent;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    /// <summary>
    /// MessagePack serializer - schema-less binary format.
    /// Pros: Fast (3-5x faster than JSON), compact, no schema required
    /// Cons: Not as compact as protobuf, less widespread
    /// Use case: Internal services, caching layers, microservices
    /// Package: Install-Package MessagePack
    /// </summary>
    public class MessagePackTimedKVSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;

        public MessagePackTimedKVSerializer(string filePath)
        {
            this.filePath = filePath;
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> cache)
        {
            // MessagePack can't directly serialize ConcurrentDictionary + SortedList
            // Convert to regular Dictionary for serialization
            var serializableData = cache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(t => t.Key, t => t.Value)
            );

            byte[] bytes = MessagePackSerializer.Serialize(serializableData);
            File.WriteAllBytes(filePath, bytes);
        }

        public ConcurrentDictionary<string, SortedList<long, string>> Deserialize()
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                return new ConcurrentDictionary<string, SortedList<long, string>>();
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            
            // Deserialize to regular dictionary, then convert back
            var data = MessagePackSerializer.Deserialize<Dictionary<string, Dictionary<long, string>>>(bytes);
            
            var cache = new ConcurrentDictionary<string, SortedList<long, string>>();
            
            foreach (var kvp in data)
            {
                var sortedList = new SortedList<long, string>(kvp.Value);
                cache[kvp.Key] = sortedList;
            }
            
            return cache;
        }
    }

    /// <summary>
    /// Alternative: MessagePack with compression (LZ4)
    /// Even smaller files, slightly slower
    /// </summary>
    public class MessagePackLZ4TimedKVSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;
        private readonly MessagePackSerializerOptions options;

        public MessagePackLZ4TimedKVSerializer(string filePath)
        {
            this.filePath = filePath;
            // Enable LZ4 compression
            this.options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> cache)
        {
            var serializableData = cache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(t => t.Key, t => t.Value)
            );

            byte[] bytes = MessagePackSerializer.Serialize(serializableData, options);
            File.WriteAllBytes(filePath, bytes);
        }

        public ConcurrentDictionary<string, SortedList<long, string>> Deserialize()
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                return new ConcurrentDictionary<string, SortedList<long, string>>();
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            var data = MessagePackSerializer.Deserialize<Dictionary<string, Dictionary<long, string>>>(bytes, options);
            
            var cache = new ConcurrentDictionary<string, SortedList<long, string>>();
            foreach (var kvp in data)
            {
                cache[kvp.Key] = new SortedList<long, string>(kvp.Value);
            }
            
            return cache;
        }
    }
}
