using System.Runtime.CompilerServices;
using System.Threading.Channels;
using AdClickAggregator.Interfaces;
using AdClickAggregator.Models;

namespace AdClickAggregator.EventStream;

/// <summary>
/// In-memory implementation of an event stream using System.Threading.Channels.
/// 
/// THIS IS YOUR IN-MEMORY KAFKA.
/// 
/// System.Threading.Channels is perfect for this because:
/// 1. Thread-safe producer/consumer without manual locking
/// 2. Bounded capacity (backpressure — just like Kafka partitions have limits)
/// 3. Async-native (no blocking threads while waiting for events)
/// 4. Built into .NET (no external dependencies)
/// 
/// REAL-WORLD EQUIVALENT:
/// ┌──────────┐     ┌─────────────────────┐     ┌──────────┐
/// │ Producer │────▶│ Channel (= Kafka    │────▶│ Consumer │
/// │ (API)    │     │   partition)        │     │(Processor)│
/// └──────────┘     └─────────────────────┘     └──────────┘
/// 
/// In Kafka, you'd have multiple partitions keyed by adId.
/// Here we use a single channel for simplicity, but the PATTERN is the same.
/// </summary>
public class InMemoryEventStream : IEventStream
{
    // Bounded channel = bounded queue with backpressure
    // If the channel is full (10,000 events), PublishAsync will WAIT
    // This prevents memory from growing unbounded if consumers are slow
    private readonly Channel<AdClickEvent> _channel;
    private readonly ILogger<InMemoryEventStream> _logger;

    public InMemoryEventStream(ILogger<InMemoryEventStream> logger)
    {
        _logger = logger;

        // BoundedChannelOptions configures our "partition"
        _channel = Channel.CreateBounded<AdClickEvent>(new BoundedChannelOptions(10_000)
        {
            // What happens when channel is full?
            // Wait = apply backpressure (producer slows down)
            // DropOldest = lose oldest events (acceptable for metrics, NOT for billing)
            FullMode = BoundedChannelFullMode.Wait,

            // Allow multiple concurrent writers (multiple API requests)
            SingleWriter = false,

            // Single reader (our processor) — allows internal optimizations
            SingleReader = true
        });
    }

    /// <summary>
    /// Publish a click event. This is called by the API controller.
    /// In Kafka terms: producer.send(topic="ad_clicks", key=event.AdId, value=event)
    /// </summary>
    public async Task PublishAsync(AdClickEvent clickEvent, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(clickEvent, ct);
        _logger.LogDebug("Published click {ClickId} for ad {AdId}", clickEvent.ClickId, clickEvent.AdId);
    }

    /// <summary>
    /// Consume events as they arrive. Called by the background processor.
    /// In Kafka terms: while(true) { records = consumer.poll(); process(records); }
    /// 
    /// IAsyncEnumerable is elegant here — the processor does:
    ///   await foreach (var click in stream.ConsumeAsync(ct))
    ///   {
    ///       Process(click);
    ///   }
    /// </summary>
    public async IAsyncEnumerable<AdClickEvent> ConsumeAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // ReadAllAsync yields events as they arrive, blocking (async) when empty
        await foreach (var clickEvent in _channel.Reader.ReadAllAsync(ct))
        {
            yield return clickEvent;
        }
    }

    /// <summary>
    /// Backpressure indicator. In production, you'd alert if this stays high.
    /// High pending count = consumers can't keep up = need to scale out.
    /// </summary>
    public int PendingCount => _channel.Reader.Count;
}
