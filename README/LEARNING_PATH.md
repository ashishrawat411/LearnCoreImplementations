# DSA Problems Learning Path

A curated progression through 15 data structure and algorithm problems from the [OpenAI Interview Prep Guide](OpenAI%20interview%20prep.md), ordered from foundational to advanced concepts.

---

## 🎯 Learning Philosophy

**Incremental Complexity**: Start with core data structures, progressively add layers of complexity through concurrency, async operations, and distributed systems.

**Production Focus**: Each problem is built as a .NET Core REST API with proper design patterns, testing, and documentation.

**Time Investment**: ~110-140 hours total (3-4 weeks full-time or 7-8 weeks part-time)

---

## 📚 Problems Ordered by Concept (Grouped Learning Path)

### 🔵 TRACK 1: Storage Systems (Master Data Storage Progressively)

Build a complete understanding of data storage from simple to complex, culminating in a full database.

#### 1. Key-Value Store Implementation
**Reference:** Problem #15 from interview guide  
**Difficulty:** ⭐ Easy  
**Core Concepts:** Dictionary/HashMap basics, CRUD operations, REST API fundamentals  
**Skills:** API design, dependency injection, Repository pattern  
**Prerequisites:** None  
**Estimated Time:** 3-4 hours

**What You'll Learn:**
- Basic dictionary operations and hash map internals
- RESTful CRUD endpoint design
- Repository pattern for data access abstraction
- Unit testing with xUnit
- Thread safety with ConcurrentDictionary

---

#### 2. Time-Based Key-Value Store
**Reference:** Problem #1 from interview guide  
**Difficulty:** ⭐⭐ Medium  
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
- Multiple values per key management

---

#### 3. KV Store Serialize/Deserialize
**Reference:** Problem #2 from interview guide  
**Difficulty:** ⭐⭐ Medium  
**Core Concepts:** Binary serialization, length-prefixing, protocol design  
**Skills:** BinaryWriter/Reader, encoding schemes, protocol compatibility  
**Prerequisites:** Problems #1 and #2 (storage foundations)  
**Estimated Time:** 4-5 hours

**What You'll Build On:**
- Adds persistence layer to KV stores
- Handles edge cases (null bytes, special characters)

**What You'll Learn:**
- Length-prefix encoding technique
- Binary I/O in .NET
- Protocol design considerations
- File persistence for time-series data

---

#### 4. In-Memory Database with SQL
**Reference:** Problem #3 from interview guide  
**Difficulty:** ⭐⭐⭐ Advanced  
**Core Concepts:** SQL parsing, query execution, indexes, multi-table operations  
**Skills:** Parser design, query optimization, JOIN algorithms  
**Prerequisites:** Problems #1, #2, #3 (storage mastery)  
**Estimated Time:** 10-12 hours

**What You'll Build On:**
- Combines all storage concepts learned so far
- Most complex storage integration problem

**What You'll Learn:**
- Recursive descent parser for SQL
- Query execution engine design
- WHERE clause evaluation with predicates
- ORDER BY with IComparer
- JOIN algorithms (nested loop, hash join)
- Index design for performance

---

### 🟢 TRACK 2: Parsing & String Manipulation

#### 5. Dependency Version Check
**Reference:** Problem #7 from interview guide  
**Difficulty:** ⭐ Easy  
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

### 🟡 TRACK 3: Iteration Patterns

#### 6. IPv4 Address Iterator
**Reference:** Problem #14 from interview guide  
**Difficulty:** ⭐ Easy  
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

#### 7. Resumable Iterator
**Reference:** Problem #4 from interview guide  
**Difficulty:** ⭐ Easy  
**Core Concepts:** State management, cursor-based iteration, serialization  
**Skills:** Memento pattern, state persistence, iterator protocol  
**Prerequisites:** Understanding of iteration concepts (Problem #6)  
**Estimated Time:** 4-5 hours

**What You'll Learn:**
- State pattern and Memento pattern
- Cursor-based navigation
- State serialization/deserialization
- Resume/pause functionality

---

### 🟠 TRACK 4: State Management & Navigation

#### 8. Unix CD Command
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
Medium  
**Core Concepts:** Stack-based navigation, cycle detection, path resolution  
**Skills:** Graph traversal, symlink handling, loop detection  
**Prerequisites:** Problem #7 (state management)  
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

### 🟣 TRACK 5: Resource Management & Scheduling

#### 9mated Time:**Medium  
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

### 🔴 TRACK 6: Dependencies & Graph Algorithms

#### 10 Concepts:**Medium  
**Core Concepts:** Dependency graphs, DFS traversal, cycle detection, caching  
**Skills:** Graph algorithms, topological sort, Observer pattern  
**Prerequisites:** Graph basics (Problem #8)  
**Estimated Time:** 6-8 hours

**What You'll Learn:**
- Dependency graph construction
- DFS-based cycle detection
- Observer pattern for cell dependencies
- Memoization and caching strategies
- Topological sort for evaluation order

---

#### 11. Node Counting in Tree
**Reference:** Problem #11 from interview guide  
**Difficulty:** ⭐⭐⭐ Advanced  
**Core Concepts:** Async/await, recursive aggregation, message passing  
**Skills:** Async reAdvanced  
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

#### 13

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
| Fri | #7 | Path navExpert  
**Core Concepts:** Job scheduling, resource quotas, priority management, OOM handling  
**Skills:** Heap-based sche (New Grouped Order)

| # | Problem | Track | Difficulty | Key Patterns | Est. Hours |
|---|---------|-------|-----------|--------------|-----------|
| 1 | Key-Value Store | Storage | ⭐ | Repository | 3-4 |
| 2 | Time-Based KV Store | Storage | ⭐⭐ | Repository | 5-6 |
| 3 | KV Serialization | Storage | ⭐⭐ | Strategy | 4-5 |
| 4 | In-Memory SQL DB | Storage | ⭐⭐⭐ | Repository, Strategy | 10-12 |
| 5 | Version Check | Parsing | ⭐ | Strategy | 2-3 |
| 6 | IPv4 Iterator | Iteration | ⭐ | Iterator | 3-4 |
| 7 | Resumable Iterator | Iteration | ⭐ | Memento, State | 4-5 |
| 8 | Unix CD | State/Nav | ⭐⭐ | State | 5-6 |
| 9 | GPU Credits | Resources | ⭐⭐ | Strategy | 5-7 |
| 10 | Spreadsheet | Dependencies | ⭐⭐ | Observer, Visitor | 6-8 |
| 11 | Node Counting | Dependencies | ⭐⭐⭐ | Composite | 6-8 |
| 12 | Web Crawler | Concurrency | ⭐⭐⭐ | Producer-Consumer | 8-10 |
| 13 | Job Manager | Concurrency | ⭐⭐⭐⭐ | Command, Strategy | 12-15 |
| 14 | Training Pipeline | Distributed | ⭐⭐⭐⭐ | Saga, CQRS | 15-20 |
| 15 | Type System | Specialized | ⭐⭐⭐ | Visitor | 10-15 |

**Total:** ~110-140 hours

**Learning Path Philosophy:** Problems are now grouped by concept/track, allowing you to master one domain before moving to the next. This supports deeper understanding and better retention.rt  
**Core Concepts:** Streaming data, checkpointing, log aggregation, fault tolerance  
**Skills:** Distributed systems, consistency, partial failure recovery  
**Prerequisites:** Problems #11 (async), #12 (concurrency), #13 (job management)  
**Estimated Time:** 15-20 hours - Grouped by Track

**Week 1 - Storage, Parsing, & Iteration Mastery**

| Day | Problems | Focus Track | Hours |
|-----|----------|-------------|-------|
| Mon | #1 | Storage: Basic KV Store | 3-4 |
| Mon-Tue | #2 | Storage: Time-Based KV | 5-6 |
| Wed | #3 | Storage: Serialization | 4-5 |
| Thu | #5 | Parsing: Version Check | 2-3 |
| Thu-Fri | #6, #7 | Iteration: IPv4 & Resumable | 7-9 |
| Sat | #8 | State: Unix CD | 5-6 |
| Sun | #9 | Resources: GPU Credits | 5-7 |

**Week 2 - Dependencies, Concurrency & Integration**

| Day | Problems | Focus Track | Hours |
|-----|----------|-------------|-------|
| Mon | #10 | Dependencies: Spreadsheet | 6-8 |
| Tue | #11 | Dependencies: Node Counting | 6-8 |
| Wed-Thu | #4 | Storage: SQL Database | 10-12 |
| Fri | #12 | Concurrency: Web Crawler | 8-10 |
| Sat | #13 | Concurrency: Job Manager (core) | 8-10 |
| Sun | #14 | Distributed: Training Pipeline (core) | 8-10 compilers  
**Estimated Time:** 10-15 hours

**What You'll Learn:**
- Abstract Syntax Tree (AST) design
- Hindley-Milner type inference basics
- Visitor pattern for AST traversal
- Type unification algorithm - Track Focused

**Week 1: Storage Systems Track**
- Mon-Tue: Problem #1 - Basic KV Store (3-4 hrs)
- Wed-Thu: Problem #2 - Time-Based KV (5-6 hrs)
- Fri-Sat: Problem #3 - Serialization (4-5 hrs)
- Sun: Review & Documentation (2 hrs)

**Week 2: Parsing, Iteration, State Tracks**
- Mon: Problem #5 - Version Check (2-3 hrs)
- Tue-Wed: Problem #6 - IPv4 Iterator (3-4 hrs)
- Thu-Fri: Problem #7 - Resumable Iterator (4-5 hrs)
- Sat-Sun: Problem #8 - Unix CD (5-6 hrs)

**Week 3: Resources, Dependencies Tracks**
- Mon-Tue: Problem #9 - GPU Credits (5-7 hrs)
- Wed-Fri: Problem #10 - Spreadsheet (6-8 hrs)
- Sat-Sun: Problem #11 - Node Counting (6-8 hrs)

**Week 4: Storage Integration & Concurrency**
- Mon-Wed: Problem #4 - SQL Database core (10-12 hrs)
- Thu-Sun: Problem #12 - Web Crawler (8-10 hrs)

**Optional Week 5-6: Expert Level**
- Week 5: Problem #13 - Job Manager
- Week 6: Problem #14 - Training Pipeline

---

## 🎓 Learning Milestones - Track Mastery

Perfect for working professionals who want thorough understanding.

| Week | Problems | Focus Track |
|------|----------|-------------|
| 1 | #1, #2 | Storage Track: Basic + Time-Based KV |
| 2 | #3, #5 | Storage + Parsing: Serialization + Version Check |
| 3 | #6, #7 | Iteration Track: IPv4 + Resumable Iterator |
| 4 | #8, #9 | State + Resources: Unix CD + GPU Credits |
| 5 | #10, #11 | Dependencies Track: Spreadsheet + Node Counting |
| 6 | #4 | Storage Integration: SQL Database |
| 7 | #12, #13 | Concurrency Track: Web Crawler + Job Manager |
| 8 | #14, #15 (optional) | Distributed + Specializedon)
- ✅ Priority queues and heaps
- ✅ Observer pattern
- ✅ Serialization techniques

**After Advanced Tier (Problems 10-13):**
- ✅ Multithreading and sy (Track-Based)

After completing each track, you should be comfortable with:

**After Storage Track (Problems 1-4):**
- ✅ Building REST APIs in .NET Core
- ✅ Repository pattern and dependency injection
- ✅ Hash maps, sorted collections, and binary search
- ✅ Serialization and persistence
- ✅ SQL parsing and query execution
- ✅ Thread safety with ConcurrentDictionary

**After Parsing & Iteration Tracks (Problems 5-7):**
- ✅ String parsing and validation
- ✅ Iterator pattern and state management
- ✅ Bit manipulation
- ✅ State serialization
- ✅ Resume/pause functionality

**After State & Resource Tracks (Problems 8-9):**
- ✅ Stack-based navigation
- ✅ Priority queues and heaps
- ✅ Time-based resource management
- ✅ Cycle detection

**After Dependencies Track (Problems 10-11):**
- ✅ Dependency graphs and DFS
- ✅ Observer pattern
- ✅ Async recursion and Task composition
- ✅ Topological sorting

**After Concurrency & Distributed Tracks (Problems 12-14):**
- ✅ Multithreading and synchronization
- ✅ Producer-Consumer pattern
- ✅ Job scheduling and worker pools
- ✅ Streaming data processing
- ✅ FMaster Each Track**: Complete all problems in a track before moving to next
2. **Build on Previous Work**: Each track builds conceptual depth progressively
3. **Test Everything**: Write tests before implementation (TDD)
4. **Document Decisions**: Maintain DESIGN.md for pattern choices in each project
5. **Ask Why**: Understand trade-offs, not just implementations
6. **Build Incrementally**: Start minimal, add complexity in phases
7. **Use Git**: Commit after each phase (minimal → optimized → concurrent → extended)
8. **Cross-Reference**: Notice how later problems apply concepts from earlier tracks
 the **Storage Track**:
1. Review [PROJECT_GUIDE_FOR_AI.md](PROJECT_GUIDE_FOR_AI.md) for detailed instructions
2. Set up your development environment (.NET 8 SDK, Visual Studio/VS Code)
3. Choose a schedule above that fits your availability
4. **Start with Problem #1: Key-Value Store Implementation**
5. **Progress through Problems #1 → #2 → #3 → #4** to master storage systems
6. Move to next track once storage concepts are solid

**Why Start with Storage Track?**
- Builds foundational API design skills
- All 15 problems benefit from understanding data storage
- Progressive complexity within one domain
- Natural progression: simple → time-based → persistent → queryable