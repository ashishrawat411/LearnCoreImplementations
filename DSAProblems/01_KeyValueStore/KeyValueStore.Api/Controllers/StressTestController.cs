using Microsoft.AspNetCore.Mvc;

namespace KeyValueStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StressTestController : ControllerBase
    {
        private readonly IKeyValueStore _keyValueStore;

        public StressTestController(IKeyValueStore keyValueStore)
        {
            _keyValueStore = keyValueStore;
        }

        /// <summary>
        /// Triggers concurrent writes to expose thread safety issues
        /// </summary>
        [HttpGet("concurrent-writes")]
        public IActionResult TriggerConcurrentWrites()
        {
            var tasks = new List<Task>();
            var errors = new List<string>();
            var lockObj = new object();

            // Spawn 100 concurrent tasks that all try to write
            for (int i = 0; i < 100; i++)
            {
                int taskId = i;
                var task = Task.Run(() =>
                {
                    try
                    {
                        // Each task tries to add the same key multiple times
                        _keyValueStore.Set($"key-{taskId}", $"value-{taskId}");
                        
                        // Immediately try to read it back
                        var value = _keyValueStore.Get($"key-{taskId}");
                        
                        // Try to update it
                        _keyValueStore.Set($"key-{taskId}", $"updated-{taskId}");
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                        {
                            errors.Add($"Task {taskId}: {ex.GetType().Name} - {ex.Message}");
                        }
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            if (errors.Any())
            {
                return Ok(new
                {
                    Success = false,
                    Message = "Thread safety issues detected!",
                    ErrorCount = errors.Count,
                    Errors = errors.Take(10).ToList() // Show first 10 errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "All 100 concurrent operations completed successfully"
            });
        }

        /// <summary>
        /// Triggers race condition between ContainsKey and Add
        /// </summary>
        [HttpGet("race-condition")]
        public IActionResult TriggerRaceCondition()
        {
            var errors = new List<string>();
            var lockObj = new object();

            // Clear the store first
            try
            {
                _keyValueStore.Delete("race-key");
            }
            catch { }

            var tasks = new List<Task>();

            // 50 tasks all trying to add the SAME key simultaneously
            for (int i = 0; i < 50; i++)
            {
                int taskId = i;
                var task = Task.Run(() =>
                {
                    try
                    {
                        // This will cause race condition in your Set method
                        // Multiple threads pass ContainsKey check, then all try to Add
                        _keyValueStore.Set("race-key", $"value-from-task-{taskId}");
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                        {
                            errors.Add($"Task {taskId}: {ex.GetType().Name}");
                        }
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            return Ok(new
            {
                ErrorCount = errors.Count,
                Message = errors.Count > 0 
                    ? $"Race condition detected! {errors.Count} tasks crashed" 
                    : "No errors (lucky timing, or you fixed it!)",
                Errors = errors.GroupBy(e => e).Select(g => new { Error = g.Key, Count = g.Count() })
            });
        }

        /// <summary>
        /// Simulates realistic concurrent traffic
        /// </summary>
        [HttpGet("realistic-load")]
        public async Task<IActionResult> RealisticLoad()
        {
            var tasks = new List<Task>();
            var operations = new List<string>();
            var errors = new List<string>();
            var lockObj = new object();
            var random = new Random();

            for (int i = 0; i < 200; i++)
            {
                int taskId = i;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        // Random delay to simulate real traffic patterns
                        await Task.Delay(random.Next(0, 10));

                        var operation = random.Next(0, 3);
                        switch (operation)
                        {
                            case 0: // Write
                                _keyValueStore.Set($"key-{taskId % 20}", $"value-{taskId}");
                                lock (lockObj) operations.Add("Write");
                                break;
                            case 1: // Read
                                _keyValueStore.Get($"key-{taskId % 20}");
                                lock (lockObj) operations.Add("Read");
                                break;
                            case 2: // Delete
                                _keyValueStore.Delete($"key-{taskId % 20}");
                                lock (lockObj) operations.Add("Delete");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                        {
                            errors.Add($"{ex.GetType().Name}: {ex.Message}");
                        }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return Ok(new
            {
                TotalOperations = operations.Count,
                Writes = operations.Count(o => o == "Write"),
                Reads = operations.Count(o => o == "Read"),
                Deletes = operations.Count(o => o == "Delete"),
                Errors = errors.Count,
                ErrorSamples = errors.Take(5),
                Status = errors.Count > 0 ? "⚠️ Thread safety issues detected!" : "✅ All operations succeeded"
            });
        }
    }
}
