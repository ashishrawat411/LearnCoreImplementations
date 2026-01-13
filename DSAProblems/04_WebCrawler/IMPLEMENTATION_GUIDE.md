# Multi-Threaded Web Crawler - Quick Start Guide (Console App)

## 🎯 Your Mission

Implement a multi-threaded web crawler that discovers all URLs under the same hostname. **You'll implement the core logic yourself** - the scaffolding is provided.

---

## 📁 Project Structure

```
04_WebCrawler/
├── README.md                           # Problem description & architecture
├── CONCEPTS.md                         # Deep dive into threading concepts ⭐ READ THIS FIRST
├── IMPLEMENTATION_GUIDE.md             # This file
└── WebCrawlerConsole/
    ├── Program.cs                      # ✅ Complete - Console UI with menu
    ├── Interfaces/
    │   ├── IHtmlParser.cs              # ✅ Complete - URL fetcher interface
    │   └── IWebCrawler.cs              # ✅ Complete - Crawler interface
    └── BusinessLogic/
        ├── MockHtmlParser.cs           # ✅ Complete - Test data provider
        ├── SingleThreadedCrawler.cs    # 🔨 YOU IMPLEMENT - Phase 1
        └── MultiThreadedCrawler.cs     # 🔨 YOU IMPLEMENT - Phase 2
```

---

## 🚀 Phase 1: Single-Threaded Crawler (30-45 minutes)

### Goal
Implement basic BFS (Breadth-First Search) without threading complexity.

### File to Edit
`WebCrawlerConsole/BusinessLogic/SingleThreadedCrawler.cs`

### Algorithm Steps

1. **Extract hostname** from `startUrl`
   ```csharp
   var startUri = new Uri(startUrl);
   string targetHostname = startUri.Host;  // "news.yahoo.com"
   ```

2. **Initialize data structures**
   ```csharp
   var visited = new HashSet<string>();          // Track visited URLs
   var queue = new Queue<string>();              // BFS queue
   queue.Enqueue(startUrl);
   visited.Add(startUrl);
   ```

3. **BFS loop**
   ```csharp
   while (queue.Count > 0)
   {
       string currentUrl = queue.Dequeue();
       
       // Fetch linked URLs (blocking call ~15ms)
       List<string> linkedUrls = htmlParser.GetUrls(currentUrl);
       
       foreach (string linkedUrl in linkedUrls)
       {
           // Extract hostname from linked URL
           if (!Uri.TryCreate(linkedUrl, UriKind.Absolute, out var linkedUri))
               continue;
           
           // Only process if same hostname
           if (linkedUri.Host != targetHostname)
               continue;
           
           // Add if not visited
           if (!visited.Contains(linkedUrl))
           {
               visited.Add(linkedUrl);
               queue.Enqueue(linkedUrl);
           }
       }
   }
   ```

4. **Return results**
   ```csharp
   return visited.ToList();
   ```

### Helper Method

```csharp
private static string GetHostname(string url)
{
    try
    {
        return new Uri(url).Host;
    }
    catch
    {
        return string.Empty;
    }
}
```

### Test Your Implementation

```bash
cd D:\Personal\Projects\LearnCoreImplementations\DSAProblems\04_WebCrawler\WebCrawler
dotnet run
```

Open browser: `http://localhost:5100`

**POST to `/api/v1/crawler/crawl`:**
```json
{
  "startUrl": "http://news.yahoo.com/news/topics/",
  "testScenario": "example1",
  "useMultiThreading": false
}
```

**Expected output:**
```json
{
  "crawledUrls": [
    "http://news.yahoo.com/news/topics/",
    "http://news.yahoo.com",
    "http://news.yahoo.com/news",
    "http://news.yahoo.com/us"
  ],
  "totalUrls": 4,
  "timeElapsedMs": 60,  // ~15ms × 4 URLs
  "usedMultiThreading": false
}
```

---

## 🧵 Phase 2: Multi-Threaded Crawler (1-2 hours)

### Goal
Parallelize URL fetching while maintaining correctness.

### File to Edit
`WebCrawlerConsole/BusinessLogic/MultiThreadedCrawler.cs`

### Key Challenges

1. **Race Condition:** Multiple threads checking/adding same URL
2. **Completion Detection:** When are all threads done?
3. **Resource Limits:** Don't spawn unlimited threads

### Algorithm Steps

1. **Use thread-safe collection**
   ```csharp
   var visited = new ConcurrentDictionary<string, bool>();
   visited.TryAdd(startUrl, true);
   ```

2. **Limit concurrency**
   ```csharp
   var semaphore = new SemaphoreSlim(_maxConcurrency);
   ```

3. **Track active tasks**
   ```csharp
   var activeTasks = 1;  // Start with 1 (initial URL)
   var completionSource = new TaskCompletionSource<bool>();
   ```

4. **Process URL (recursive task)**
   ```csharp
   async Task ProcessUrl(string url)
   {
       // Acquire semaphore slot
       await semaphore.WaitAsync();
       
       try
       {
           // Fetch URLs (blocking, but we're on thread pool)
           var linkedUrls = await Task.Run(() => htmlParser.GetUrls(url));
           
           var hostname = GetHostname(url);
           
           foreach (var linkedUrl in linkedUrls)
           {
               // Filter by hostname
               if (GetHostname(linkedUrl) != hostname)
                   continue;
               
               // Try add (atomic - prevents duplicates)
               if (visited.TryAdd(linkedUrl, true))
               {
                   // Spawn new task
                   Interlocked.Increment(ref activeTasks);
                   _ = ProcessUrl(linkedUrl);  // Fire and forget
               }
           }
       }
       finally
       {
           semaphore.Release();
           
           // Decrement active tasks
           if (Interlocked.Decrement(ref activeTasks) == 0)
           {
               // Last task done - signal completion
               completionSource.TrySetResult(true);
           }
       }
   }
   ```

5. **Start and wait**
   ```csharp
   _ = ProcessUrl(startUrl);
   await completionSource.Task;
   return visited.Keys.ToList();
   ```

### Why This Works

**Atomic Operations:**
- `visited.TryAdd()` - Only ONE thread succeeds per URL
- `Interlocked.Increment/Decrement` - Atomic counter updates
- `semaphore.WaitAsync()` - Limits concurrent requests

**Task Coordination:**
- Each task spawns child tasks
- `activeTasks` tracks pending work
- When counter reaches 0, all done!

### Test Your Implementation

**Run the console app and select option 1, then choose 'y' for multi-threaded:**

```bash
dotnet run

Select option: 1
Use multi-threading? (y/n): y
```

**Expected output:**
```
Start URL: http://news.yahoo.com/news/topics/
Use multi-threading? (y/n): y

Crawling with Multi-Threaded crawler...

Results:
────────────────────────────────────────────────────────────
  ✓ http://news.yahoo.com
  ✓ http://news.yahoo.com/news
  ✓ http://news.yahoo.com/news/topics/
  ✓ http://news.yahoo.com/us
────────────────────────────────────────────────────────────
URLs Found:      4
Expected:        4
TiSelect option 3 from the main menu:**

```
Select option: 3
Select test scenario (1=Example1, 2=Example2): 1
```

**Expected output:**
```
Benchmarking: http://news.yahoo.com/news/topics/
Running both single-threaded and multi-threaded...

────────────────────────────────────────────────────────────
Benchmark Results:
────────────────────────────────────────────────────────────
URLs Crawled:           4
Expected URLs:          4

Single-Threaded Time:   60ms
Multi-Threaded Time:    15ms

Speedup Factor:         4.00x faster ⚡

✅ Both implementations are correct!
**Expected output:**
```json
{
  "startUrl": "http://news.yahoo.com/news/topics/",
  "singleThreadedTimeMs": 60,
  "multiThreadedTimeMs": 15,
  "speedupFactor": 4.0,    // 4x faster!
  "urlsCrawled": 4,
  "threadsUsed": 10
}
```

---

## 🎓 Learning Checkpoints

### After Phase 1 (Single-Threaded)
- ✅ Can you explain how BFS works?
- ✅ Why use `Queue` instead of recursion?
- ✅ What's the time complexity? O(V + E) where V=URLs, E=links

### After Phase 2 (Multi-Threaded)
- ✅ Why is `ConcurrentDictionary.TryAdd()` atomic?
- ✅ What happens without `Interlocked` for the counter?
- ✅ Why use `SemaphoreSlim` instead of spawning unlimited tasks?
- ✅ How do you prevent deadlocks?

---

## 🐛 Common Mistakes

### Mistake 1: Using `HashSet` with `lock`
```csharp
// ❌ DON'T DO THIS
private readonly HashSet<string> _visited = new();
private readonly object _lock = new();

if (!_visited.Contains(url))  // Check outside lock!
{
    lock (_lock)
    {
        _visited.Add(url);  // Race condition!
    }
}
```

**Fix:** Use `ConcurrentDictionary.TryAdd()`

### Mistake 2: Not Handling Completion
```csharp
// ❌ This never completes!
_ = ProcessUrl(startUrl);
// Method returns immediately, tasks still running
```

**Fix:** Use `TaskCompletionSource` or track active tasks

### Mistake 3: Forgetting Semaphore Release
```csharp
await semaphore.WaitAsync();
var urls = htmlParser.GetUrls(url);  // Throws exception!
semaphore.Release();  // ❌ Never called!
```

**Fix:** Use `try/finally`

---

## 📊 Expected Performance

| Scenario | URLs | Single-Threaded | Multi-Threaded (10x) | Speedup |
|----------|------|----------------|---------------------|---------|
| Example 1 | 4 | ~60ms | ~15ms | 4x |
| Example 2 | 1 | ~15ms | ~15ms | 1x |
| Large Graph | 100 | ~1500ms | ~150ms | 10x |

---

## 🚦 Next Steps

1. **Read CONCEPTS.md** - Understand threading fundamentals
2. **Implement Phase 1** - Single-threaded crawler
3. **Test with Swagger** - Verify correctness
4. **Implement Phase 2** - Multi-threaded crawler
5. **Run Benchmark** - Measure speedup
6. **Add Extensions:**
   - Max depth limiting
   - Max URLs limiting
   - Per-domain rate limiting
   - Progress reporting
   - Cancellation token support

---

## 💡 Hints

**Stuck on single-threaded?**
- Draw the graph on paper
- Trace through BFS manually
- Use debugger to step through

**Stuck on multi-threaded?**
- Start with Phase 1 working
- Add threading incrementally
- Use `Console.WriteLine` to trace execution
- Test with smaller examples first

**Still stuck?**
Review CONCEPTS.md sections:
- Thread-Safe Collections
- Synchronization Primitives
- Implementation Strategies

---

## 🎯 Success Criteria

**Single-Threaded:**
- ✅ Returns correct URLs for test scenarios
- ✅ No duplicates
- ✅ Only same-hostname URLs

**Multi-Threaded:**
- ✅ Same correctness as single-threaded
- ✅ Significant speedup (3-10x)
- ✅ No race conditions
- ✅ Proper completion detection

**Good luck! 🚀 Start coding!**
