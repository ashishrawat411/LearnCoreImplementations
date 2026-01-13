# Concurrency & Threading Concepts

This document explains the fundamental concepts needed to solve the multi-threaded web crawler problem.

---

## Table of Contents

1. [Why Multi-Threading Matters](#why-multi-threading-matters)
2. [Threading Fundamentals](#threading-fundamentals)
3. [Thread-Safe Collections](#thread-safe-collections)
4. [Synchronization Primitives](#synchronization-primitives)
5. [Common Concurrency Problems](#common-concurrency-problems)
6. [Task-Based Asynchronous Pattern (TAP)](#task-based-asynchronous-pattern-tap)
7. [Implementation Strategies](#implementation-strategies)
8. [Performance Analysis](#performance-analysis)

---

## Why Multi-Threading Matters

### The Problem with Single-Threaded I/O

When your code makes a network request (HTTP call), it's **waiting** most of the time:

```
CPU Timeline (Single-Threaded):
[Request 1: ████░░░░░░░░░░░░] 15ms (94% waiting)
[Request 2: ████░░░░░░░░░░░░] 15ms
[Request 3: ████░░░░░░░░░░░░] 15ms
Total: 45ms for 3 URLs
```

**CPU is idle 94% of the time!** The `████` represents actual CPU work, `░░░░` is waiting for network.

### Multi-Threading Solution

Run requests **simultaneously**:

```
CPU Timeline (Multi-Threaded):
Thread 1: [Request 1: ████░░░░░░░░░░░░]
Thread 2: [Request 2: ████░░░░░░░░░░░░]
Thread 3: [Request 3: ████░░░░░░░░░░░░]
Total: ~15ms for 3 URLs (3x speedup!)
```

### Real Numbers

```
Scenario: 1000 URLs, 15ms per request

Single-threaded:  1000 × 15ms = 15,000ms (15 seconds)
10 threads:       1000 ÷ 10 × 15ms = 1,500ms (1.5 seconds)
Speedup:          10x faster! ⚡
```

---

## Threading Fundamentals

### Threads vs Tasks

**Thread:**
- Low-level OS construct
- Expensive to create (~1MB stack)
- `Thread.Start()`, `Thread.Join()`

**Task:**
- High-level abstraction over threads
- Uses thread pool (reuses threads)
- `Task.Run()`, `await`, `Task.WhenAll()`

**Rule of Thumb:** Use `Task` in modern C#, not `Thread` directly.

### Process vs Thread vs Task

```
Process (Your Application)
├─ Thread 1 (Main Thread)
├─ Thread 2 (Worker)
│  ├─ Task A
│  └─ Task B
└─ Thread 3 (Worker)
   └─ Task C
```

- **Process:** Isolated memory space, separate program
- **Thread:** Shares memory, concurrent execution path
- **Task:** Work unit, may run on any thread

---

## Thread-Safe Collections

### Why Regular Collections Fail

```csharp
// ❌ NOT THREAD-SAFE
var visited = new HashSet<string>();

// Thread A checks...
if (!visited.Contains(url))  // Returns false
{
    // Thread B also checks here and gets false!
    visited.Add(url);         // Both add! Race condition!
}
```

### Solution: Concurrent Collections

#### `ConcurrentDictionary<TKey, TValue>`

**Best for:** Tracking visited URLs (key-value pairs)

```csharp
var visited = new ConcurrentDictionary<string, bool>();

// ✅ ATOMIC operation - thread-safe
if (visited.TryAdd(url, true))
{
    // Only ONE thread enters here for each URL
    ProcessUrl(url);
}
```

**Key Methods:**
- `TryAdd(key, value)` - Add if not exists (atomic)
- `TryGetValue(key, out value)` - Get value safely
- `TryRemove(key, out value)` - Remove safely
- `GetOrAdd(key, valueFactory)` - Get or create

**Advantages:**
- Lock-free reads (very fast!)
- Atomic writes (thread-safe)
- High concurrency

#### `ConcurrentBag<T>`

**Best for:** Collecting results (unordered)

```csharp
var results = new ConcurrentBag<string>();

Parallel.ForEach(urls, url =>
{
    results.Add(ProcessUrl(url));  // Thread-safe add
});
```

**Characteristics:**
- No ordering guarantee
- Very fast add/take
- Best for producer-consumer

#### `BlockingCollection<T>`

**Best for:** Work queue with blocking

```csharp
var workQueue = new BlockingCollection<string>();

// Producer
workQueue.Add(url);
workQueue.CompleteAdding();  // Signal done

// Consumer
foreach (var url in workQueue.GetConsumingEnumerable())
{
    ProcessUrl(url);  // Blocks until item available
}
```

**Characteristics:**
- Blocks when empty (efficient waiting)
- Bounded capacity (backpressure)
- Thread-safe enqueue/dequeue

### Comparison Table

| Collection | Ordering | Speed | Use Case |
|-----------|----------|-------|----------|
| `ConcurrentDictionary` | Hash-based | ⚡⚡⚡⚡ | Deduplication, lookup |
| `ConcurrentBag` | None | ⚡⚡⚡⚡ | Unordered results |
| `ConcurrentQueue` | FIFO | ⚡⚡⚡ | Work queue, BFS |
| `BlockingCollection` | FIFO | ⚡⚡⚡ | Producer-consumer |

---

## Synchronization Primitives

### `lock` Statement

**Purpose:** Ensure only ONE thread executes code at a time.

```csharp
private readonly object _lock = new object();
private int _counter = 0;

public void Increment()
{
    lock (_lock)
    {
        _counter++;  // Only one thread here at a time
    }
}
```

**When to use:**
- Protecting shared mutable state
- Short critical sections
- Simple synchronization

**Downside:**
- Blocks threads (wastes resources)
- Can cause deadlocks
- Reduces parallelism

### `SemaphoreSlim`

**Purpose:** Limit number of concurrent operations.

```csharp
private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10);  // Max 10 concurrent

public async Task ProcessUrlAsync(string url)
{
    await _semaphore.WaitAsync();  // Wait for slot
    try
    {
        await FetchUrlAsync(url);   // Do work
    }
    finally
    {
        _semaphore.Release();       // Free slot
    }
}
```

**Use cases:**
- Rate limiting (max N concurrent API calls)
- Resource pooling (max N database connections)
- Throttling

**Why SemaphoreSlim over Semaphore:**
- Lighter weight
- Supports async/await
- Better for async I/O

### `Interlocked` Class

**Purpose:** Atomic operations on simple types.

```csharp
private int _activeTaskCount = 0;

// ❌ NOT THREAD-SAFE
_activeTaskCount++;  // Read-modify-write (3 operations, can be interrupted)

// ✅ THREAD-SAFE
Interlocked.Increment(ref _activeTaskCount);  // Atomic single operation
```

**Common operations:**
- `Interlocked.Increment(ref value)` - Atomic ++
- `Interlocked.Decrement(ref value)` - Atomic --
- `Interlocked.CompareExchange(ref location, value, comparand)` - Atomic conditional update
- `Interlocked.Add(ref location, value)` - Atomic addition

**When to use:**
- Counters (active tasks, processed items)
- Flags (Boolean operations)
- Simple state machines

---

## Common Concurrency Problems

### 1. Race Condition

**Definition:** Outcome depends on timing/order of thread execution.

```csharp
// Example: Two threads incrementing counter
Thread A reads:  counter = 5
Thread B reads:  counter = 5
Thread A writes: counter = 6
Thread B writes: counter = 6  // Lost increment! Should be 7
```

**Detection:** Non-deterministic failures, "works sometimes".

**Solutions:**
- Use `Interlocked` for simple operations
- Use `lock` for complex operations
- Use concurrent collections

### 2. Deadlock

**Definition:** Two threads wait for each other forever.

```csharp
// Thread A
lock (lockA)
{
    lock (lockB)  // Waits for Thread B to release lockB
    {
        // ...
    }
}

// Thread B
lock (lockB)
{
    lock (lockA)  // Waits for Thread A to release lockA
    {
        // ...
    }
}
// ⚠️ Both threads stuck forever!
```

**Prevention:**
- Always acquire locks in same order
- Use timeout with `Monitor.TryEnter()`
- Avoid nested locks
- Use lock-free algorithms

### 3. Starvation

**Definition:** Thread never gets resources.

```csharp
// High-priority threads keep taking lock
// Low-priority thread never gets chance
while (true)
{
    lock (resource)  // High-priority thread grabs it immediately
    {
        Process();
    }
}
```

**Solutions:**
- Fair scheduling (FIFO)
- Priority boosting
- Reader-writer locks (`ReaderWriterLockSlim`)

### 4. Memory Visibility

**Definition:** Thread sees stale cached values.

```csharp
private bool _stop = false;  // Thread A

public void WorkerThread()   // Thread B
{
    while (!_stop)  // May cache _stop = false forever!
    {
        DoWork();
    }
}
```

**Solution:** Use `volatile` or `Interlocked` or synchronization.

```csharp
private volatile bool _stop = false;  // Forces fresh read every time
```

---

## Task-Based Asynchronous Pattern (TAP)

### `async` / `await` Basics

```csharp
public async Task<string> FetchUrlAsync(string url)
{
    // await releases thread while waiting (efficient!)
    var response = await httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}
```

**Key Points:**
- `async` = method can use `await`
- `await` = asynchronous wait (doesn't block thread)
- Thread is **released** during wait (can do other work)
- Returns `Task` or `Task<T>`

### `Task.Run()` vs `await`

```csharp
// Task.Run - CPU-bound work (run on thread pool)
var result = await Task.Run(() => ExpensiveComputation());

// await - I/O-bound work (async wait, no thread used)
var result = await httpClient.GetAsync(url);
```

**Rule:**
- **CPU-bound:** Use `Task.Run()` to offload to thread pool
- **I/O-bound:** Use `await` directly (no `Task.Run` needed)

### `Task.WhenAll()` - Wait for Multiple Tasks

```csharp
var tasks = new List<Task<string>>();

foreach (var url in urls)
{
    tasks.Add(FetchUrlAsync(url));  // Start all tasks
}

// Wait for ALL to complete
var results = await Task.WhenAll(tasks);
```

**Parallel execution:** All tasks run simultaneously!

### `Task.WhenAny()` - Wait for First Completion

```csharp
var tasks = new List<Task<string>> { task1, task2, task3 };

// Returns as soon as ANY task completes
var completedTask = await Task.WhenAny(tasks);
var result = await completedTask;
```

---

## Implementation Strategies

### Strategy 1: Level-by-Level BFS (Recommended)

**Idea:** Process URLs level-by-level, wait for each level to complete.

```csharp
var visited = new ConcurrentDictionary<string, bool>();
var currentLevel = new List<string> { startUrl };

while (currentLevel.Any())
{
    var nextLevel = new ConcurrentBag<string>();
    
    // Process current level in parallel
    var tasks = currentLevel.Select(url => Task.Run(async () =>
    {
        var linkedUrls = await GetLinkedUrlsAsync(url);
        foreach (var linkedUrl in linkedUrls)
        {
            if (visited.TryAdd(linkedUrl, true))
            {
                nextLevel.Add(linkedUrl);
            }
        }
    }));
    
    await Task.WhenAll(tasks);  // Wait for level to complete
    
    currentLevel = nextLevel.ToList();
}
```

**Pros:**
- Clean BFS semantics
- Easy to implement depth limiting
- Natural checkpoints (level boundaries)

**Cons:**
- Not maximum parallelism (waits between levels)

### Strategy 2: Dynamic Task Spawning

**Idea:** Tasks spawn new tasks immediately, track completion.

```csharp
var visited = new ConcurrentDictionary<string, bool>();
var activeTasks = 0;
var completionSource = new TaskCompletionSource<bool>();

async Task ProcessUrl(string url)
{
    Interlocked.Increment(ref activeTasks);
    try
    {
        var linkedUrls = await GetLinkedUrlsAsync(url);
        foreach (var linkedUrl in linkedUrls)
        {
            if (visited.TryAdd(linkedUrl, true))
            {
                _ = ProcessUrl(linkedUrl);  // Fire and forget
            }
        }
    }
    finally
    {
        if (Interlocked.Decrement(ref activeTasks) == 0)
        {
            completionSource.SetResult(true);  // Signal done
        }
    }
}

_ = ProcessUrl(startUrl);
await completionSource.Task;  // Wait for all tasks
```

**Pros:**
- Maximum parallelism
- Continuous work (no waiting)

**Cons:**
- Complex completion detection
- Harder to limit depth

### Strategy 3: Producer-Consumer Pattern

**Idea:** Worker threads consume from shared work queue.

```csharp
var workQueue = new BlockingCollection<string>();
var visited = new ConcurrentDictionary<string, bool>();

// Start workers
var workers = Enumerable.Range(0, workerCount)
    .Select(_ => Task.Run(() => Worker(workQueue, visited)))
    .ToArray();

// Producer
workQueue.Add(startUrl);
// ... as workers discover URLs, they add to queue

// Wait for empty queue, then signal done
workQueue.CompleteAdding();
await Task.WhenAll(workers);

void Worker(BlockingCollection<string> queue, ConcurrentDictionary<string, bool> visited)
{
    foreach (var url in queue.GetConsumingEnumerable())
    {
        var linkedUrls = GetLinkedUrls(url);
        foreach (var linkedUrl in linkedUrls)
        {
            if (visited.TryAdd(linkedUrl, true))
            {
                queue.Add(linkedUrl);
            }
        }
    }
}
```

**Pros:**
- Explicit worker control
- Natural backpressure (bounded queue)
- Easy to add/remove workers

**Cons:**
- Most complex
- Harder to detect completion

---

## Performance Analysis

### Amdahl's Law

**Formula:** `Speedup = 1 / ((1 - P) + P / N)`

Where:
- `P` = Proportion of code that can be parallelized
- `N` = Number of threads

**Example:** If 95% of work is parallelizable:

```
N=1  (single-threaded): Speedup = 1.00x
N=2:  Speedup = 1.90x
N=4:  Speedup = 3.48x
N=8:  Speedup = 5.93x
N=16: Speedup = 9.14x
N=∞:  Speedup = 20.00x (max theoretical)
```

**Takeaway:** Adding more threads has diminishing returns!

### Optimal Thread Count

For **I/O-bound** work (web crawler):

```
Optimal threads = (Wait time / CPU time) × Core count

Example:
Wait time: 14ms (network)
CPU time:  1ms (processing)
Cores:     8

Optimal = (14 / 1) × 8 = 112 threads!
```

For **CPU-bound** work:

```
Optimal threads ≈ Core count

(More threads = more context switching overhead)
```

### Bottlenecks

1. **Network bandwidth** - Can't exceed your internet speed
2. **Target server** - May throttle/block too many requests
3. **Lock contention** - Threads waiting for locks
4. **Context switching** - Too many threads (overhead)

---

## Key Takeaways

1. **Use concurrent collections** (`ConcurrentDictionary`) instead of locks when possible
2. **Limit concurrency** with `SemaphoreSlim` to avoid overwhelming resources
3. **Track active work** with `Interlocked` for coordination
4. **Prefer Task over Thread** for modern C# async patterns
5. **Test with stress tests** - concurrency bugs are non-deterministic
6. **Measure before optimizing** - profile to find real bottlenecks
7. **Start simple** - single-threaded first, then parallelize

---

## Further Reading

- [Task Parallel Library (TPL)](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)
- [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Threading in C#](https://www.albahari.com/threading/)

**Now go implement the crawler!** Start with single-threaded, then add parallelism. Good luck! 🚀
