using System.Collections.Concurrent;
using System.Text;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.BusinessLogic
{
    /// <summary>
    /// Custom binary serializer using length-prefix encoding.
    /// Format: [count][length][data][length][data]...
    /// Handles any characters including null bytes, newlines, special chars.
    /// </summary>
    public class BinaryTimedKVSerializer : ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>
    {
        private readonly string filePath;
        private const int MAGIC_NUMBER = 0x54424B56; // "TBKV" in hex
        private const int VERSION = 1;

        public BinaryTimedKVSerializer(string filePath)
        {
            this.filePath = filePath;
        }

        public void Serialize(ConcurrentDictionary<string, SortedList<long, string>> cache)
        {
            // Atomic write pattern: write to temp file, then rename
            // Prevents data loss if process crashes during write
            string tempFilePath = filePath + ".tmp";
            
            try
            {
                // Write to temporary file
                using (FileStream stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                    {
                        // Write header for validation and versioning
                        writer.Write(MAGIC_NUMBER);
                        writer.Write(VERSION);
                        
                        // Write number of keys
                        writer.Write(cache.Count);

                        foreach (var kvp in cache)
                        {
                            // Write key using BinaryWriter's built-in string handling
                            // (automatic length-prefix + UTF-8 encoding)
                            writer.Write(kvp.Key);
                            
                            // Write number of timestamps in timeline
                            writer.Write(kvp.Value.Count);
                            
                            foreach (var timeline in kvp.Value)
                            {
                                // Write timestamp (8 bytes)
                                writer.Write(timeline.Key);
                                
                                // Write value string (length-prefixed automatically)
                                writer.Write(timeline.Value);
                            }
                        }
                        
                        // Ensure all data is written to disk before renaming
                        stream.Flush();
                    }
                }
                
                // Atomic rename: if crash happens before this, old file is intact
                // File.Move is atomic on same filesystem (NTFS/ext4/APFS)
                File.Move(tempFilePath, filePath, overwrite: true);
            }
            catch
            {
                // Clean up temp file if write failed
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { /* Ignore cleanup errors */ }
                }
                throw;
            }
        }

        public ConcurrentDictionary<string, SortedList<long, string>> Deserialize()
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                return new ConcurrentDictionary<string, SortedList<long, string>>();
            }

            using (FileStream stream = new FileStream(this.filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    // Validate header
                    int magic = reader.ReadInt32();
                    if (magic != MAGIC_NUMBER)
                    {
                        throw new InvalidDataException($"Invalid file format. Expected magic number {MAGIC_NUMBER:X}, got {magic:X}");
                    }
                    
                    int version = reader.ReadInt32();
                    if (version != VERSION)
                    {
                        throw new InvalidDataException($"Unsupported version. Expected {VERSION}, got {version}");
                    }

                    var cache = new ConcurrentDictionary<string, SortedList<long, string>>();
                    
                    // Read number of keys
                    int keyCount = reader.ReadInt32();
                    
                    for (int i = 0; i < keyCount; i++)
                    {
                        // Read key (BinaryReader.ReadString handles length-prefix)
                        string key = reader.ReadString();
                        
                        // Read number of timestamps
                        int timelineCount = reader.ReadInt32();
                        
                        var timeline = new SortedList<long, string>(timelineCount);
                        
                        for (int j = 0; j < timelineCount; j++)
                        {
                            // Read timestamp
                            long timestamp = reader.ReadInt64();
                            
                            // Read value
                            string value = reader.ReadString();
                            
                            timeline[timestamp] = value;
                        }
                        
                        cache[key] = timeline;
                    }
                    
                    return cache;
                }
            }
        }
    }
}
