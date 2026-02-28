using AdClickAggregator.EventStream;
using AdClickAggregator.Interfaces;
using AdClickAggregator.Processing;
using AdClickAggregator.Storage;

var builder = WebApplication.CreateBuilder(args);

// ==================== DEPENDENCY INJECTION ====================
// This is where we wire up our "infrastructure" with in-memory implementations.
// In production, you'd swap these for Kafka, Redis, Gorilla, etc.
// The controller and processor don't know or care — they use interfaces.

// Event Stream: Channel<T> (in-memory Kafka)
// Singleton because there's ONE stream that producers write to and consumers read from
builder.Services.AddSingleton<IEventStream, InMemoryEventStream>();

// Deduplicator: ConcurrentDictionary (in-memory Bloom filter / Redis SET)
// Singleton because dedup state must be shared across all processing
builder.Services.AddSingleton<IClickDeduplicator, InMemoryClickDeduplicator>();

// Aggregation Store: ConcurrentDictionary (in-memory Gorilla / time-series DB)
// Singleton because all aggregated data lives in one place
builder.Services.AddSingleton<IAggregationStore, InMemoryAggregationStore>();

// Stream Processor: BackgroundService (in-memory Flink)
// This starts automatically and runs for the app's lifetime
builder.Services.AddSingleton<ClickAggregatorProcessor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ClickAggregatorProcessor>());

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Serve the web UI from wwwroot/
app.UseDefaultFiles(); // Serves index.html by default
app.UseStaticFiles();  // Serves CSS, JS, etc.

app.MapControllers();

// Fallback: redirect root to the UI
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "AdClickAggregator",
    timestamp = DateTime.UtcNow
}));

app.Run();
