using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Collections.Concurrent;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    /// <summary>
    /// BSON (Binary JSON) serializer - MongoDB's native format.
    /// Pros: JSON compatibility, rich types (Date, Binary, ObjectId), MongoDB integration
    /// Cons: Not as compact as protobuf, mainly MongoDB ecosystem
    /// Use case: MongoDB storage, JSON-like flexibility with binary efficiency
    /// Package: Install-Package MongoDB.Bson
    /// </summary>
    public class BsonTimedKVSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;

        public BsonTimedKVSerializer(string filePath)
        {
            this.filePath = filePath;
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> cache)
        {
            // Convert to BSON-friendly structure
            var bsonDocument = new BsonDocument();
            
            foreach (var kvp in cache)
            {
                var timelineArray = new BsonArray();
                
                foreach (var timestamp in kvp.Value)
                {
                    timelineArray.Add(new BsonDocument
                    {
                        { "t", timestamp.Key },      // timestamp
                        { "v", timestamp.Value }     // value
                    });
                }
                
                bsonDocument.Add(kvp.Key, timelineArray);
            }

            using (var stream = File.Create(filePath))
            {
                using (var writer = new MongoDB.Bson.IO.BsonBinaryWriter(stream))
                {
                    BsonSerializer.Serialize(writer, bsonDocument);
                }
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
                using (var reader = new MongoDB.Bson.IO.BsonBinaryReader(stream))
                {
                    var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(reader);
                    var cache = new ConcurrentDictionary<string, SortedList<long, string>>();

                    foreach (var element in bsonDocument.Elements)
                    {
                        var key = element.Name;
                        var timelineArray = element.Value.AsBsonArray;
                        
                        var sortedList = new SortedList<long, string>();
                        
                        foreach (var item in timelineArray)
                        {
                            var doc = item.AsBsonDocument;
                            long timestamp = doc["t"].AsInt64;
                            string value = doc["v"].AsString;
                            sortedList[timestamp] = value;
                        }
                        
                        cache[key] = sortedList;
                    }

                    return cache;
                }
            }
        }
    }
}
