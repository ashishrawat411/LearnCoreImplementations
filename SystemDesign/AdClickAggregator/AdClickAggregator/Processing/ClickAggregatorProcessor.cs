using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;

namespace AdClickAggregator.Processing;

/// <summary>
/// Background service that continuously reads from the event stream and aggregates clicks.
/// 
/// THIS IS YOUR IN-MEMORY FLINK / STREAM PROCESSOR.
/// 
/// In the real world (Meta):
/// - Apache Flink or Meta's Stylus processes events in real-time
/// - It runs continuously, reading from Scribe/Kafka partitions
/// - Each processor instance handles a subset of partitions (horizontal scaling)
/// 
/// Our BackgroundService does the same thing:
/// 1. Read event from stream (like Kafka consumer.poll())
/// 2. Deduplicate (check if we've seen this click before)
/// 3. Aggregate into time windows (tumbling window counts)
/// 4. Repeat forever
/// 
/// INTERVIEW KEY POINT:
/// "The stream processor is stateful — it maintains aggregation state.
///  If it crashes, we replay events from the last Kafka offset checkpoint.
///  This gives us exactly-once processing semantics with at-least-once delivery."
/// </summary>
public class ClickAggregatorProcessor : BackgroundService
{
    private readonly IEventStream _eventStream;
    private readonly IClickDeduplicator _deduplicator;
    private readonly IAggregationStore _aggregationStore;
    private readonly ILogger<ClickAggregatorProcessor> _logger;

    // Metrics for monitoring
    private long _totalProcessed;
    private long _totalDeduplicated;

    public long TotalProcessed => _totalProcessed;
    public long TotalDeduplicated => _totalDeduplicated;

    public ClickAggregatorProcessor(
        IEventStream eventStream,
        IClickDeduplicator deduplicator,
        IAggregationStore aggregationStore,
        ILogger<ClickAggregatorProcessor> logger)
    {
        _eventStream = eventStream;
        _deduplicator = deduplicator;
        _aggregationStore = aggregationStore;
        _logger = logger;
    }

    /// <summary>
    /// Main processing loop — runs for the lifetime of the application.
    /// 
    /// This is the HEART of the system. Every click flows through here:
    /// EventStream → Dedup → Aggregate (minute + hour + day windows)
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Click Aggregator Processor started. Waiting for events...");

        try
        {
            // await foreach: blocks (async) until an event is available, then processes it
            // This is the same pattern as Kafka's consumer.poll() loop
            await foreach (var clickEvent in _eventStream.ConsumeAsync(stoppingToken))
            {
                ProcessClick(clickEvent);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Click Aggregator Processor stopping (cancellation requested)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Click Aggregator Processor encountered an error");
            throw; // Let the host know the background service failed
        }
    }

    /// <summary>
    /// Process a single click event through the pipeline:
    /// 1. Dedup check
    /// 2. Aggregate into multiple window sizes simultaneously
    /// 
    /// WHY MULTIPLE WINDOWS?
    /// Advertisers query at different granularities:
    /// - "How many clicks in the last minute?" → minute window
    /// - "How is my campaign doing today?" → hour window  
    /// - "Monthly report" → day window
    /// 
    /// Pre-aggregating at all levels means queries at any granularity are O(1).
    /// The cost is ~3x write amplification (acceptable trade-off).
    /// </summary>
    private void ProcessClick(AdClickEvent clickEvent)
    {
        Interlocked.Increment(ref _totalProcessed);

        // Step 1: Deduplication
        if (!_deduplicator.TryMarkAsSeen(clickEvent))
        {
            Interlocked.Increment(ref _totalDeduplicated);
            _logger.LogDebug("Skipping duplicate click: {ClickId}", clickEvent.ClickId);
            return;
        }

        // Step 2: Aggregate into ALL window sizes simultaneously
        // This is "write amplification" — one click becomes 3 writes
        // But it makes reads fast at any granularity
        _aggregationStore.RecordClick(clickEvent, WindowSize.OneMinute);
        _aggregationStore.RecordClick(clickEvent, WindowSize.OneHour);
        _aggregationStore.RecordClick(clickEvent, WindowSize.OneDay);

        _logger.LogDebug(
            "Processed click {ClickId} for ad {AdId} by user {UserId}",
            clickEvent.ClickId, clickEvent.AdId, clickEvent.UserId);
    }
}
