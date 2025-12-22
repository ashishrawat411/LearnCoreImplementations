# DSA Problems Learning Path

A curated progression through 15 data structure and algorithm problems from the [OpenAI Interview Prep Guide](OpenAI%20interview%20prep.md), ordered from foundational to advanced concepts.

---

## 🎯 Learning Philosophy

**Incremental Complexity**: Start with core data structures, progressively add layers of complexity through concurrency, async operations, and distributed systems.

**Production Focus**: Each problem is built as a .NET Core REST API with proper design patterns, testing, and documentation.

**Time Investment**: ~110-140 hours total (3-4 weeks full-time or 7-8 weeks part-time)

---

## 📚 Problems Ordered by Difficulty

### 🟢 EASY - Foundations (Problems 1-4)

#### 1. Key-Value Store Implementation
**Reference:** Problem #15 from interview guide  
**Difficulty:** ⭐  
**Core Concepts:** Dictionary/HashMap basics, CRUD operations, REST API fundamentals
**Skills:** API design, dependency injection, Repository pattern
**Prerequisites:** None
**Estimated Time:** 3-4 hours

**What You'll Learn:**
- Basic dictionary operations and hash map internals
- RESTful CRUD endpoint design
- Repository pattern for data access abstraction
- Unit testing with xUnit

---

#### 2. Dependency Version Check
**Reference:** Problem #7 from interview guide  
**Difficulty:** ⭐  
**Core Concepts:** String parsing, tuple comparison, version semantics  
**Skills:** Parsing algorithms, comparison logic, validation  
**Prerequisites:** None  
**Estimated Time:** 2-3 hours

**What You'll Learn:**
- String manipulation and parsing techniques
- Custom comparison logic implementation
- Input validation patterns
- Edge case handling (leading zeros, missing segments)

---

#### 3. IPv4 Address Iterator
**Reference:** Problem #14 from interview guide  
**Difficulty:** ⭐  
**Core Concepts:** CIDR notation, bit manipulation, iteration patterns  
**Skills:** Bitwise operations, IP address handling, iterator design  
**Prerequisites:** Basic understanding of binary representation  
**Estimated Time:** 3-4 hours

**What You'll Learn:**
- Bit manipulation techniques
- CIDR block calculations
- Iterator pattern implementation
- IP address validation

---

#### 4. Resumable Iterator
**Reference:** Problem #4 from interview guide  
**Difficulty:** ⭐  
**Core Concepts:** State management, cursor-based iteration, serialization  
**Skills:** Memento pattern, state persistence, iterator protocol  
**Prerequisites:** Understanding of iteration concepts  
**Estimated Time:** 4-5 hours

**What You'll Learn:**
- State pattern and Memento pattern
- Cursor-based navigation
- State serialization/deserialization
- Async iterator extensions

---

### 🟡 MEDIUM - Building Complexity (Problems 5-9)

#### 5. Time-Based Key-Value Store
**Reference:** Problem #1 from interview guide  
**Difficulty:** ⭐⭐  
**Core Concepts:** Binary search, sorted data structures, temporal queries  
**Skills:** Search algorithms, time-series data, thread safety  
**Prerequisites:** Problem #1 (basic KV store)  
**Estimated Time:** 5-6 hours

**What You'll Build On:**
- Extends basic KV store with time dimension
- Adds O(log n) binary search optimization
- Introduces thread safety concerns

**What You'll Learn:**
- Binary search on sorted collections
- SortedList vs List<T> trade-offs
- Lock-based synchronization
- File persistence for time-series data

---

#### 6. KV Store Serialize/Deserialize
**Reference:** Problem #2 from interview guide  
**Difficulty:** ⭐⭐  
**Core Concepts:** Binary serialization, length-prefixing, protocol design  
**Skills:** BinaryWriter/Reader, encoding schemes, protocol compatibility  
**Prerequisites:** Problems #1 and #5 (storage foundations)  
**Estimated Time:** 4-5 hours

**What You'll Build On:**
- Adds persistence layer to KV stores
- Handles edge cases (null bytes, special characters)

**What You'll Learn:**
- Length-prefix encoding technique
- Binary I/O in .NET
- Protocol design considerations
- Redis protocol compatibility basics

---

#### 7. Unix CD Command
**Reference:** Problem #5 from interview guide  
**Difficulty:** ⭐⭐  
**Core Concepts:** Stack-based navigation, cycle detection, path resolution  
**Skills:** Graph traversal, symlink handling, loop detection  
**Prerequisites:** Problem #4 (state management)  
**Estimated Time:** 5-6 hours

**What You'll Build On:**
- More complex state management than resumable iterator
- Introduces graph concepts (symlinks as edges)

**What You'll Learn:**
- Stack-based path resolution
- Cycle detection with visited sets
- Canonical path normalization
- Symlink resolution algorithms

---

#### 8. GPU Credit System
**Reference:** Problem #9 from interview guide  
**Difficulty:** ⭐⭐  
**Core Concepts:** Priority queues, FIFO with expiration, resource management  
**Skills:** Heap data structures, time-based operations, event ordering  
**Prerequisites:** Understanding of heaps/priority queues  
**Estimated Time:** 5-7 hours

**What You'll Learn:**
- PriorityQueue<T> in .NET 6+
- FIFO queue with expiration policies
- Time-based resource allocation
- Event interleaving and ordering

---

#### 9. Spreadsheet Formula Evaluation
**Reference:** Problem #6 from interview guide  
**Difficulty:** ⭐⭐  
**Core Concepts:** Dependency graphs, DFS traversal, cycle detection, caching  
**Skills:** Graph algorithms, topological sort, Observer pattern  
**Prerequisites:** Graph basics (BFS/DFS)  
**Estimated Time:** 6-8 hours

**What You'll Learn:**
- Dependency graph construction
- DFS-based cycle detection
- Observer pattern for cell dependencies
- Memoization and caching strategies
- Topological sort for evaluation order

---

### 🟠 ADVANCED - Concurrency & Integration (Problems 10-13)

#### 10. Multithreaded Web Crawler
**Reference:** Problem #8 from interview guide  
**Difficulty:** ⭐⭐⭐  
**Core Concepts:** Multithreading, deduplication, rate limiting, concurrent collections  
**Skills:** Producer-Consumer pattern, thread pools, synchronization  
**Prerequisites:** Understanding of threading and locks  
**Estimated Time:** 8-10 hours

**What You'll Learn:**
- ConcurrentQueue and ConcurrentHashSet
- Producer-Consumer pattern with BlockingCollection
- Rate limiting with SemaphoreSlim
- Thread coordination and signaling
- Graceful shutdown with CancellationToken

---

#### 11. Node Counting in Tree
**Reference:** Problem #11 from interview guide  
**Difficulty:** ⭐⭐⭐  
**Core Concepts:** Async/await, recursive aggregation, message passing  
**Skills:** Async recursion, Task composition, timeout handling  
**Prerequisites:** Understanding of async/await and Tasks  
**Estimated Time:** 6-8 hours

**What You'll Learn:**
- Async recursive algorithms
- Task.WhenAll for parallel aggregation
- CancellationToken for timeouts
- Async message passing patterns
- Error handling in async trees

---

#### 12. In-Memory Database with SQL
**Reference:** Problem #3 from interview guide  
**Difficulty:** ⭐⭐⭐  
**Core Concepts:** SQL parsing, query execution, indexes, multi-table operations  
**Skills:** Parser design, query optimization, JOIN algorithms  
**Prerequisites:** Problems #1, #5 (storage), parsing experience  
**Estimated Time:** 10-12 hours

**What You'll Build On:**
- Combines storage, parsing, filtering, and sorting
- Most complex integration problem so far

**What You'll Learn:**
- Recursive descent parser for SQL
- Query execution engine design
- WHERE clause evaluation with predicates
- ORDER BY with IComparer
- JOIN algorithms (nested loop, hash join)
- Index design for performance

---

#### 13. Toy Language Type System
**Reference:** Problem #13 from interview guide  
**Difficulty:** ⭐⭐⭐  
**Core Concepts:** Type inference, AST traversal, constraint solving  
**Skills:** Compiler design, type checking, Visitor pattern  
**Prerequisites:** Understanding of parsers and compilers  
**Estimated Time:** 10-15 hours

**What You'll Learn:**
- Abstract Syntax Tree (AST) design
- Hindley-Milner type inference basics
- Visitor pattern for AST traversal
- Type unification algorithm
- Generic type handling
- Compiler error reporting

---

### 🔴 EXPERT - Distributed Systems (Problems 14-15)

#### 14. Async Training Job Manager
**Reference:** Problem #10 from interview guide  
**Difficulty:** ⭐⭐⭐⭐  
**Core Concepts:** Job scheduling, resource quotas, priority management, OOM handling  
**Skills:** Heap-based scheduling, worker pools, fault tolerance  
**Prerequisites:** Problems #8 (concurrency), #9 (priority queues), #11 (async)  
**Estimated Time:** 12-15 hours

**What You'll Build On:**
- Combines priority queues (#8), concurrency (#10), and async (#11)
- Most complex orchestration problem

**What You'll Learn:**
- Multi-priority job scheduling
- Resource quota enforcement
- Out-of-memory (OOM) detection and rollback
- Worker pool management
- Starvation prevention algorithms
- Background job status tracking API

---

#### 15. Model Training Pipeline
**Reference:** Problem #12 from interview guide  
**Difficulty:** ⭐⭐⭐⭐  
**Core Concepts:** Streaming data, checkpointing, log aggregation, fault tolerance  
**Skills:** Distributed systems, consistency, partial failure recovery  
**Prerequisites:** Problems #10 (concurrency), #11 (async), #14 (job management)  
**Estimated Time:** 15-20 hours

**What You'll Build On:**
- Capstone problem integrating all previous concepts
- Full distributed system with streaming and recovery

**What You'll Learn:**
- Streaming data processing with IAsyncEnumerable
- Checkpoint/restart mechanisms
- Write-ahead logging (WAL)
- Distributed log aggregation
- Partial failure handling
- Eventually consistent systems
- gRPC streaming (advanced alternative to REST)

---

## 📊 Quick Reference Table

| # | Problem | Difficulty | Category | Key Patterns | Est. Hours |
|---|---------|-----------|----------|--------------|-----------|
| 1 | Key-Value Store | ⭐ | Storage | Repository | 3-4 |
| 2 | Version Check | ⭐ | Parsing | Strategy | 2-3 |
| 3 | IPv4 Iterator | ⭐ | Iteration | Iterator | 3-4 |
| 4 | Resumable Iterator | ⭐ | State | Memento, State | 4-5 |
| 5 | Time-Based KV Store | ⭐⭐ | Storage | Repository | 5-6 |
| 6 | Serialization | ⭐⭐ | I/O | Strategy | 4-5 |
| 7 | Unix CD | ⭐⭐ | Navigation | State | 5-6 |
| 8 | GPU Credits | ⭐⭐ | Resources | Strategy | 5-7 |
| 9 | Spreadsheet | ⭐⭐ | Dependencies | Observer, Visitor | 6-8 |
| 10 | Web Crawler | ⭐⭐⭐ | Concurrency | Producer-Consumer | 8-10 |
| 11 | Node Counting | ⭐⭐⭐ | Async | Composite | 6-8 |
| 12 | In-Memory DB | ⭐⭐⭐ | Integration | Repository, Strategy | 10-12 |
| 13 | Type System | ⭐⭐⭐ | Specialized | Visitor | 10-15 |
| 14 | Job Manager | ⭐⭐⭐⭐ | Orchestration | Command, Strategy | 12-15 |
| 15 | Training Pipeline | ⭐⭐⭐⭐ | Distributed | Saga, CQRS | 15-20 |

**Total:** ~110-140 hours

---

## 📅 Recommended Learning Schedules

### Option A: 2-Week Intensive (Full-Time)

**Week 1 - Foundations & Medium Complexity**

| Day | Problems | Focus | Hours |
|-----|----------|-------|-------|
| Mon | #1, #2 | Storage basics + Parsing | 6-7 |
| Tue | #3, #4 | Iteration patterns | 7-9 |
| Wed | #5 | Time-based queries + Binary search | 5-6 |
| Thu | #6 | Serialization + I/O | 4-5 |
| Fri | #7 | Path navigation + Graphs intro | 5-6 |
| Sat | #8 | Priority queues + Resources | 5-7 |
| Sun | #9 | Dependencies + DFS | 6-8 |

**Week 2 - Advanced & Expert**

| Day | Problems | Focus | Hours |
|-----|----------|-------|-------|
| Mon | #10 | Multithreading + Concurrency | 8-10 |
| Tue | #11 | Async patterns | 6-8 |
| Wed | #12 | SQL parsing (core only) | 6-8 |
| Thu | #13 OR #14 | Choose based on interest | 8-10 |
| Fri | #14 OR #15 | Job scheduling (core) | 8-10 |
| Sat | #15 | Streaming pipeline (core) | 8-10 |
| Sun | Review | Add follow-ups, refactor | 4-6 |

---

### Option B: 4-Week Balanced (Part-Time, 20 hrs/week)

**Week 1: Foundations**
- Mon-Tue: Problem #1 (3-4 hrs)
- Wed: Problem #2 (2-3 hrs)
- Thu-Fri: Problem #3 (3-4 hrs)
- Sat-Sun: Problem #4 (4-5 hrs)

**Week 2: Medium Complexity**
- Mon-Tue: Problem #5 (5-6 hrs)
- Wed-Thu: Problem #6 (4-5 hrs)
- Fri-Sat: Problem #7 (5-6 hrs)
- Sun: Buffer/Review

**Week 3: Medium-Advanced**
- Mon-Tue: Problem #8 (5-7 hrs)
- Wed-Sun: Problem #9 (6-8 hrs)
- Weekend: Problem #10 start (4-5 hrs)

**Week 4: Advanced Systems**
- Mon-Wed: Problem #10 finish (4-5 hrs)
- Thu-Fri: Problem #11 (6-8 hrs)
- Sat-Sun: Problem #12 core OR #14 core (6-8 hrs)

**Optional Week 5-6: Expert Level**
- Continue with Problems #12-15 as time permits

---

### Option C: 8-Week Deep Dive (10 hrs/week)

Perfect for working professionals who want thorough understanding.

| Week | Problems | Focus Area |
|------|----------|------------|
| 1 | #1, #2 | Storage & Parsing fundamentals |
| 2 | #3, #4 | Iteration patterns & State |
| 3 | #5, #6 | Time-series & Serialization |
| 4 | #7, #8 | Navigation & Resources |
| 5 | #9, #10 | Dependencies & Concurrency |
| 6 | #11, #12 | Async & SQL integration |
| 7 | #13 OR #14 | Specialization (choose one) |
| 8 | #15 | Distributed systems capstone |

---

## 🎓 Learning Milestones

After completing each tier, you should be comfortable with:

**After Foundations (Problems 1-4):**
- ✅ Building REST APIs in .NET Core
- ✅ Repository pattern and dependency injection
- ✅ Basic data structures (Dictionary, List)
- ✅ State management patterns
- ✅ Unit testing with xUnit

**After Medium Tier (Problems 5-9):**
- ✅ Search algorithms (binary search)
- ✅ Graph algorithms (DFS, cycle detection)
- ✅ Priority queues and heaps
- ✅ Observer pattern
- ✅ Serialization techniques

**After Advanced Tier (Problems 10-13):**
- ✅ Multithreading and synchronization
- ✅ Async/await patterns
- ✅ Producer-Consumer pattern
- ✅ Complex parsing and query engines
- ✅ Concurrent collections

**After Expert Tier (Problems 14-15):**
- ✅ Distributed system design
- ✅ Fault tolerance and recovery
- ✅ Resource scheduling
- ✅ Streaming data processing
- ✅ Production-ready system architecture

---

## 💡 Tips for Success

1. **Don't Skip the Easy Ones**: Foundations are critical for API design patterns
2. **Test Everything**: Write tests before implementation (TDD)
3. **Document Decisions**: Maintain DESIGN.md for pattern choices
4. **Refactor Often**: Return to earlier problems with new knowledge
5. **Ask Why**: Understand trade-offs, not just implementations
6. **Build Incrementally**: Start minimal, add complexity in phases
7. **Use Git**: Commit after each phase (minimal → optimized → concurrent → extended)

---

## 📖 Next Steps

Ready to start? Begin with:
1. Review [PROJECT_GUIDE_FOR_AI.md](PROJECT_GUIDE_FOR_AI.md) for detailed instructions
2. Set up your development environment (.NET 8 SDK, Visual Studio/VS Code)
3. Choose a schedule above that fits your availability
4. Start with Problem #1: Key-Value Store Implementation

**Let's build something great! 🚀**
