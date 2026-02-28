using AdClickAggregator.Models;

namespace AdClickAggregator.Interfaces;

/// <summary>
/// Abstraction for an event stream (like Kafka / Scribe at Meta).
/// 
/// WHY AN INTERFACE?
/// In production, this would be Kafka. In our learning version, it's an in-memory
/// Channel<T>. The stream processor doesn't care — it just reads events.
/// This is the same pattern used at Meta: abstractions over infrastructure.
/// 
/// KEY CONCEPT: Producer-Consumer pattern
/// - Producers (ad servers) PUSH click events into the stream
/// - Consumers (processors) PULL events out and aggregate them
/// - The stream DECOUPLES producers from consumers (they run at different speeds)
/// </summary>
public interface IEventStream
{
    /// <summary>
    /// Publish a click event to the stream.
    /// In Kafka: this would be `producer.send(topic, key=adId, value=event)`
    /// The adId as key ensures partitioning — all clicks for same ad go to same partition.
    /// </summary>
    Task PublishAsync(AdClickEvent clickEvent, CancellationToken ct = default);

    /// <summary>
    /// Consume events as an async stream.
    /// In Kafka: this would be `consumer.poll()` in a loop.
    /// IAsyncEnumerable gives us a clean way to process events one at a time.
    /// </summary>
    IAsyncEnumerable<AdClickEvent> ConsumeAsync(CancellationToken ct = default);

    /// <summary>
    /// How many events are waiting to be processed (backpressure indicator).
    /// If this grows, processors can't keep up → need more partitions/processors.
    /// </summary>
    int PendingCount { get; }
}
