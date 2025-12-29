using System.Collections.Concurrent;
using TimeBasedKVStore;
using TimeBasedKVStore.BusinessLogic;
using TimeBasedKVStore.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get file path from configuration (cleaner than hardcoding)
var filePath = builder.Configuration["CacheSettings:FilePath"] 
    ?? "diskCache.json"; // fallback

// Register dependencies with factory lambdas
builder.Services.AddSingleton<ISearchStrategy, LinearSearchStrategy>();
builder.Services.AddSingleton<ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>>(
    sp => new MyJsonSerializer(filePath));
builder.Services.AddSingleton<ITimeBasedKVStore>(
    sp => new OnDiskTimedKVStore(
        sp.GetRequiredService<ISearchStrategy>(),
        sp.GetRequiredService<ISerializer<ConcurrentDictionary<string, SortedList<long, string>>>>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();