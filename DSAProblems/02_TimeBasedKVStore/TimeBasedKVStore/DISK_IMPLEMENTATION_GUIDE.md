# Disk-Based KV Store - Implementation Guide

## What I've Created For You (Scaffolding)

✅ **ISerializer** interface - Strategy pattern for serialization
✅ **JsonSerializer** skeleton - Needs your implementation
✅ **OnDiskTimedKVStore** skeleton - Needs your implementation

**Your job:** Fill in the TODO sections!

---

## Implementation Order (Start Here!)

### **Step 1: Implement `GetFilePath` (Easiest)**

This is the simplest method - start here to get familiar:

```csharp
private string GetFilePath(string key)
{
    // Goal: Return "./data/temperature.json" for key "temperature"
    // TODO: Use Path.Combine(_dataDirectory, $"{key}.json")
}
```

**Test it:** Write a simple console test or unit test

---

### **Step 2: Implement `JsonSerializer.Serialize`**

Convert `SortedList<long, string>` to JSON string:

**Hint:** System.Text.Json example:
```csharp
using System.Text.Json;

// Dictionary to JSON
var dict = new Dictionary<long, string> { { 1000, "20°C" } };
string json = JsonSerializer.Serialize(dict);
// Result: {"1000":"20°C"}
```

**Your challenge:** SortedList works similarly, but think about:
- What if timeline is empty?
- What if timeline is null?

---

### **Step 3: Implement `JsonSerializer.Deserialize`**

Convert JSON string back to `SortedList<long, string>`:

**Hint:**
```csharp
string json = "{\"1000\":\"20°C\",\"2000\":\"22°C\"}";
var dict = JsonSerializer.Deserialize<Dictionary<long, string>>(json);

// Now convert to SortedList:
var sortedList = new SortedList<long, string>(dict);
```

**Edge cases to handle:**
- What if `data` is null or empty string?
- What if JSON is invalid?

---

### **Step 4: Implement `WriteTimelineToDisk`**

Write timeline to file:

**Hint:**
```csharp
// 1. Serialize
string json = _serializer.Serialize(timeline);

// 2. Write to file
File.WriteAllText(filePath, json);
```

**Think about:**
- What if directory doesn't exist yet?
- Should you use `File.WriteAllText` (synchronous) or `File.WriteAllTextAsync` (async)?

---

### **Step 5: Implement `ReadTimelineFromDisk`**

Read timeline from file:

**Hint:**
```csharp
// 1. Check if file exists
if (!File.Exists(filePath))
    return null;  // Key doesn't exist

// 2. Read content
string json = File.ReadAllText(filePath);

// 3. Deserialize
return _serializer.Deserialize(json);
```

**Edge cases:**
- File exists but is empty?
- File exists but has invalid JSON?

---

### **Step 6: Implement `AddOrUpdate` (The Core Method!)**

This is where everything comes together:

**Pseudocode:**
```
1. filePath = GetFilePath(key)
2. timeline = ReadTimelineFromDisk(key)
3. If timeline is null:
      timeline = new SortedList<long, string>()
4. timeline[value.Key] = value.Value  // Add or update timestamp
5. WriteTimelineToDisk(key, timeline)
6. return true
```

**Write this yourself!** This is the most important method.

---

### **Step 7: Implement Remaining Methods**

Once `AddOrUpdate` works, these are similar:

**`Get(key)`:**
- Read timeline from disk
- Return timeline.Last().Value (most recent)

**`GetValueAtTimestamp(key, timestamp)`:**
- Read timeline from disk
- Use `_searchStrategy.GetValueAtTimestamp(timeline, timestamp)`
- Return result

**`Remove(key)`:**
- Get file path
- `File.Delete(filePath)` if exists

**`Add(key, value)`:**
- Check if key exists on disk
- If yes, return false
- If no, create timeline and write

---

## Testing Your Implementation

### Manual Test (Controller Remains the Same!)

```powershell
# Just swap in Program.cs:
builder.Services.AddSingleton<ITimeBasedKVStore, OnDiskTimedKVStore>();
builder.Services.AddSingleton<ISerializer, JsonSerializer>();

# Run app
dotnet run

# Test with Swagger or curl
```

### Unit Test Example

```csharp
[Fact]
public void AddOrUpdate_CreatesFileOnDisk()
{
    // Arrange
    var searchStrategy = new BinarySearchStrategy();
    var serializer = new JsonSerializer();
    var store = new OnDiskTimedKVStore(searchStrategy, serializer, "./test-data");
    
    // Act
    store.AddOrUpdate("temp", new KeyValuePair<long, string>(1000, "20°C"));
    
    // Assert
    Assert.True(File.Exists("./test-data/temp.json"));
}
```

---

## Questions to Think About While Implementing

1. **Thread Safety:** What happens if two requests try to write to "temperature.json" at the same time?
   
2. **Error Handling:** What if disk is full and write fails?

3. **Performance:** Every operation reads AND writes disk - is this acceptable?

4. **Data Integrity:** What if app crashes mid-write? File could be corrupted!

5. **File Locking:** Should you use `FileStream` with `FileShare.None` to prevent concurrent access?

---

## DI Registration (Add to Program.cs)

```csharp
// Swap from InMemoryTimedKVStore to OnDiskTimedKVStore
builder.Services.AddSingleton<ISerializer, JsonSerializer>();
builder.Services.AddSingleton<ISearchStrategy, BinarySearchStrategy>();
builder.Services.AddSingleton<ITimeBasedKVStore, OnDiskTimedKVStore>();
```

**Configuration (appsettings.json):**
```json
{
  "DataDirectory": "./data"
}
```

Then inject `IConfiguration` in constructor to read it!

---

## Your Next Steps

1. ✅ Start with `GetFilePath` (5 min)
2. ✅ Implement `JsonSerializer.Serialize` (10 min)
3. ✅ Implement `JsonSerializer.Deserialize` (10 min)
4. ✅ Implement `WriteTimelineToDisk` (5 min)
5. ✅ Implement `ReadTimelineFromDisk` (10 min)
6. ✅ Implement `AddOrUpdate` (15 min)
7. ✅ Implement remaining methods (20 min)
8. ✅ Test with Swagger (10 min)
9. ✅ Check `./data/` folder - see JSON files created!

**Total time:** ~90 minutes

---

## When You're Done

Share your implementation and I'll:
- Review for correctness
- Suggest optimizations
- Discuss thread safety improvements
- Compare performance vs in-memory

Let's build this! Start with **Step 1** and show me your `GetFilePath` implementation! 🚀
