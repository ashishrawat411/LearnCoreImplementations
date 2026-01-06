using ProtoBuf;
using System.Collections.Concurrent;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    /// <summary>
    /// Protocol Buffers serializer using protobuf-net library.
    /// Pros: Extremely compact (smallest), fast, schema evolution support
    /// Cons: Requires data contracts, more setup
    /// Use case: Microservices, APIs, long-term storage with versioning
    /// Package: Install-Package protobuf-net
    /// </summary>
    public class ProtobufTimedKVSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;

        public ProtobufTimedKVSerializer(string filePath)
        {
            this.filePath = filePath;
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> cache)
        {
            // Convert to protobuf-compatible data structure
            var protoData = new ProtoCacheData
            {
                Keys = cache.Select(kvp => new ProtoKeyTimeline
                {
                    Key = kvp.Key,
                    Timestamps = kvp.Value.Select(t => new ProtoTimestamp
                    {
                        Time = t.Key,
                        Value = t.Value
                    }).ToList()
                }).ToList()
            };

            using (var stream = File.Create(filePath))
            {
                Serializer.Serialize(stream, protoData);
            }
        }

        public ConcurrentDictionary<string, SortedList<long, string>> Deserialize()
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                return new ConcurrentDictionary<string, SortedList<long, string>>();
            }

            using (var stream = File.OpenRead(filePath))
            {
                var protoData = Serializer.Deserialize<ProtoCacheData>(stream);
                
                var cache = new ConcurrentDictionary<string, SortedList<long, string>>();
                
                foreach (var keyTimeline in protoData.Keys)
                {
                    var sortedList = new SortedList<long, string>();
                    foreach (var timestamp in keyTimeline.Timestamps)
                    {
                        sortedList[timestamp.Time] = timestamp.Value;
                    }
                    cache[keyTimeline.Key] = sortedList;
                }
                
                return cache;
            }
        }
    }

    // Protobuf data contracts
    // Field numbers enable schema evolution (can add fields 4, 5, 6... later)
    
    [ProtoContract]
    public class ProtoCacheData
    {
        [ProtoMember(1)]
        public List<ProtoKeyTimeline> Keys { get; set; } = new();
    }

    [ProtoContract]
    public class ProtoKeyTimeline
    {
        [ProtoMember(1)]
        public string Key { get; set; } = string.Empty;
        
        [ProtoMember(2)]
        public List<ProtoTimestamp> Timestamps { get; set; } = new();
    }

    [ProtoContract]
    public class ProtoTimestamp
    {
        [ProtoMember(1)]
        public long Time { get; set; }
        
        [ProtoMember(2)]
        public string Value { get; set; } = string.Empty;
    }
}
