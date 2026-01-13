# Multi-Threaded Web Crawler

## Problem Statement

**Source:** [LeetCode - Web Crawler Multithreaded](https://leetcode.com/problems/web-crawler-multithreaded/)

Given a URL `startUrl` and an interface `HtmlParser`, implement a **multi-threaded web crawler** to crawl all links that are under the same hostname as `startUrl`.

### Requirements

1. **Start from** `startUrl`
2. **Call** `HtmlParser.GetUrls(url)` to get all URLs from a webpage (blocking call, ~15ms)
3. **Do not crawl the same link twice**
4. **Only explore links under the same hostname** as `startUrl`

### Examples

**Example 1:**
```
Input:
  startUrl = "http://news.yahoo.com/news/topics/"
  urls = [
    "http://news.yahoo.com",
    "http://news.yahoo.com/news",
    "http://news.yahoo.com/news/topics/",
    "http://news.google.com",
    "http://news.yahoo.com/us"
  ]
Output: [
  "http://news.yahoo.com",
  "http://news.yahoo.com/news",
  "http://news.yahoo.com/news/topics/",
  "http://news.yahoo.com/us"
]
```

**Example 2:**
```
Input:
  startUrl = "http://news.google.com"
Output: ["http://news.google.com"]
Explanation: startUrl links to pages with different hostnames
```

### Constraints

- 1 ≤ urls.length ≤ 1000
- Single-threaded solutions will exceed time limit
- `GetUrls()` is a blocking call (~15ms per URL)
- Hostname format follows DNS standards

---

## 🎯 Learning Objectives

This project teaches **essential concurrency concepts** for senior engineering roles:

### Core Concepts You'll Master

1. **Multi-Threading in C#**
   - `Task` and `Task<T>` - modern async primitives
   - `Task.Run()` vs `Task.Factory.StartNew()`
   - Async/await patterns
   - Thread pool management

2. **Thread-Safe Collections**
   - `ConcurrentDictionary<TKey, TValue>` - lock-free reads
   - `ConcurrentBag<T>` - order-agnostic collection
   - `BlockingCollection<T>` - producer-consumer pattern
   - When to use each collection type

3. **Synchronization Primitives**
   - `lock` statement - exclusive access
   - `SemaphoreSlim` - limit concurrent operations
   - `Interlocked` - atomic operations
   - `Monitor` - advanced locking

4. **Graph Traversal with Concurrency**
   - BFS (Breadth-First Search) - level-by-level
   - Parallel BFS challenges
   - Deduplication in concurrent environment
   - Work distribution strategies

5. **Producer-Consumer Pattern**
   - Work queue management
   - Worker thread coordination
   - Graceful shutdown
   - Backpressure handling

6. **Performance Optimization**
   - Why single-threaded fails (15ms × 1000 URLs = 15 seconds)
   - Parallelism speedup (theoretical: 15 seconds → 1.5 seconds with 10 threads)
   - Amdahl's Law - parallel speedup limits
   - Thread count tuning

---

## 🏗️ Architecture & Design

### Phase 1: Single-Threaded Baseline (Learn the Algorithm)

**Goal:** Understand the crawling algorithm without threading complexity.

**Key Components:**
- Simple BFS using `Queue<string>`
- `HashSet<string>` for visited URLs (O(1) lookup)
- Hostname extraction from URLs
- Graph traversal basics

**Why Start Here:**
- Verify logic correctness first
- Establish performance baseline
- Easier debugging without race conditions

### Phase 2: Multi-Threaded Implementation (Add Parallelism)

**Goal:** Parallelize URL fetching while maintaining correctness.

**Challenges to Solve:**
1. **Race Conditions:** Multiple threads accessing shared state
2. **Duplicate Work:** Two threads processing same URL
3. **Work Coordination:** How to know when crawling is done?
4. **Resource Limits:** Don't overwhelm the server with requests

**Design Patterns:**

#### Pattern 1: Task-Based Parallelism ⭐ **RECOMMENDED**
```csharp
// Launch a task for each URL
var tasks = new List<Task>();
foreach (var url in urlsToProcess)
{
    tasks.Add(Task.Run(() => ProcessUrl(url)));
}
await Task.WhenAll(tasks);
```

**Pros:**
- Modern C# idiom (Task Parallel Library)
- Automatic thread pool management
- Easy to reason about
- Built-in cancellation support

**Cons:**
- Less control over thread count
- Can create many tasks for large graphs

#### Pattern 2: Producer-Consumer with BlockingCollection
```csharp
var workQueue = new BlockingCollection<string>();
var workers = Enumerable.Range(0, workerCount)
    .Select(_ => Task.Run(() => Worker(workQueue)))
    .ToArray();
```

**Pros:**
- Explicit control over worker count
- Better for very large graphs
- Natural backpressure handling

**Cons:**
- More complex coordination logic
- Harder to detect completion

#### Pattern 3: Parallel LINQ (PLINQ)
```csharp
var results = urlsToProcess
    .AsParallel()
    .WithDegreeOfParallelism(maxThreads)
    .SelectMany(url => ProcessUrl(url))
    .ToList();
```

**Pros:**
- Declarative, minimal code
- Automatic partitioning

**Cons:**
- Less control over execution
- Harder to handle dynamic work (BFS nature)

### Comparison Table

| Criteria | Task-Based | Producer-Consumer | PLINQ |
|----------|-----------|-------------------|-------|
| Complexity | ⭐⭐ | ⭐⭐⭐⭐ | ⭐ |
| Control | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| Scalability | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |
| Debugging | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| Fit for BFS | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |

**Recommendation:** Task-Based Parallelism for this problem because:
- Natural fit for dynamic graph traversal
- Easy to implement level-by-level BFS
- Good balance of simplicity and performance
- Aligns with modern C# async patterns

---

## 🧵 Threading Deep Dive

### Why Multi-Threading Helps

**The Math:**
- Single-threaded: 1000 URLs × 15ms = **15,000ms (15 seconds)**
- 10 threads: 1000 URLs ÷ 10 × 15ms = **1,500ms (1.5 seconds)** ⚡

**Real-world considerations:**
- Network I/O is waiting time (CPU idle)
- While one thread waits, others work
- Diminishing returns after ~10-20 threads (network bottleneck)

### Thread Safety Challenges

#### Challenge 1: Duplicate URL Processing
```csharp
// ❌ NOT THREAD-SAFE
if (!visited.Contains(url))  // Thread A checks
{
    visited.Add(url);         // Thread B also adds! Race condition!
    ProcessUrl(url);
}
```

**Solution: Atomic Operations**
```csharp
// ✅ THREAD-SAFE
if (visited.TryAdd(url, true))  // ConcurrentDictionary is atomic
{
    ProcessUrl(url);
}
```

#### Challenge 2: Knowing When to Stop
```csharp
// How do we know all threads are done?
// - Can't just count tasks (new tasks spawn dynamically)
// - Can't use task counter (race conditions)
// - Need coordination mechanism
```

**Solution: Task Tracking**
```csharp
var remainingTasks = 0;
Interlocked.Increment(ref remainingTasks);  // Atomic increment
// ... do work ...
if (Interlocked.Decrement(ref remainingTasks) == 0)
{
    // Last thread done!
}
```

---

## 📊 API Design

### REST Endpoints

```http
POST /api/v1/crawler/crawl
Content-Type: application/json

{
  "startUrl": "http://news.yahoo.com",
  "maxDepth": 5,
  "maxUrls": 1000,
  "useMultiThreading": true
}

Response:
{
  "crawledUrls": ["http://...", "http://..."],
  "totalUrls": 156,
  "timeElapsedMs": 1234,
  "threadsUsed": 10
}
```

```http
GET /api/v1/crawler/status/{crawlId}
```

---

## 🎓 What You'll Learn

### Concurrency Fundamentals
- **Race conditions** - when outcome depends on timing
- **Deadlocks** - circular wait dependencies
- **Starvation** - thread never gets resources
- **Memory visibility** - thread-local caching issues

### Advanced Threading
- **Work stealing** - idle threads take work from busy ones
- **Lock-free algorithms** - CAS (Compare-And-Swap)
- **Memory barriers** - ensuring write visibility
- **False sharing** - cache line contention

### System Design
- **Distributed crawling** - horizontal scaling (follow-up)
- **Rate limiting** - per-domain request limits
- **Politeness** - robots.txt, crawl delays
- **Fault tolerance** - retry logic, circuit breakers

---

## 🚀 Implementation Phases

### Phase 1: Single-Threaded (You Implement)
- [ ] Parse hostname from URL
- [ ] Implement BFS with Queue
- [ ] Track visited URLs with HashSet
- [ ] Filter by hostname
- [ ] Return all discovered URLs

**Time estimate:** 30-45 minutes  
**Goal:** Verify algorithm correctness

### Phase 2: Multi-Threaded (Guided Implementation)
- [ ] Replace HashSet with ConcurrentDictionary
- [ ] Launch parallel tasks for each URL
- [ ] Implement task coordination (when to stop?)
- [ ] Add SemaphoreSlim to limit concurrency
- [ ] Measure performance improvement

**Time estimate:** 1-2 hours  
**Goal:** Learn thread synchronization

### Phase 3: Production Features (Extensions)
- [ ] Configurable max depth/URLs
- [ ] Per-domain rate limiting
- [ ] Cancellation token support
- [ ] Progress reporting
- [ ] Error handling and retries

**Time estimate:** 1-2 hours  
**Goal:** Production-ready code

---

## 📚 Key Takeaways

1. **Concurrency ≠ Parallelism**
   - Concurrency: Structure for handling multiple tasks
   - Parallelism: Simultaneous execution

2. **Shared State is the Enemy**
   - Minimize shared mutable state
   - Use thread-safe collections
   - Consider immutable data structures

3. **Testing Concurrent Code is Hard**
   - Race conditions are non-deterministic
   - Use stress tests with many iterations
   - Consider unit testing with mocks (no real threading)

4. **Premature Optimization**
   - Start simple (single-threaded)
   - Measure before optimizing
   - Profile to find real bottlenecks

---

## Next Steps

1. **Read CONCEPTS.md** - Deep dive into threading concepts
2. **Implement Phase 1** - Single-threaded crawler
3. **Review with AI** - Get feedback on your approach
4. **Implement Phase 2** - Add multi-threading
5. **Benchmark** - Compare single vs multi-threaded performance
6. **Extend** - Add production features

**Remember:** The goal is learning, not just working code. Take time to understand WHY each pattern works!
