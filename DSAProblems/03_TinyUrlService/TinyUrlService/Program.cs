using TinyUrlService.BusinessLogic;
using TinyUrlService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register TinyUrl service as Singleton (maintains state across requests)
builder.Services.AddSingleton<ITinyUrlService, InMemoryTinyUrlService>();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TinyURL Service API",
        Version = "v1",
        Description = "A URL shortening service that creates short codes for long URLs"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Serve index.html at root (must come before UseStaticFiles)
app.UseDefaultFiles();

// Serve static files (HTML, CSS, JS)
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TinyURL Service v1");
        options.RoutePrefix = "swagger"; // Move Swagger to /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
