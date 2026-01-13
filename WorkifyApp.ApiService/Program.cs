using WorkifyApp.ApiService.Data;
using WorkifyApp.ApiService.Features.Clients;
using WorkifyApp.ApiService.Features.Projects;
using WorkifyApp.ApiService.Features.Invoicing;
using WorkifyApp.ApiService.Features.AI;
using Microsoft.SemanticKernel;
using WorkifyApp.ApiService.Features;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<WorkifyMetrics>();

// Add Postgres Context
builder.AddNpgsqlDbContext<WorkifyDbContext>("workify-db");

// Add Redis Distributed Cache
builder.AddRedisDistributedCache("redis");

// Add HybridCache
builder.Services.AddHybridCache();

// Add Semantic Kernel (Placeholder - requires config)
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
if (!string.IsNullOrEmpty(openAiKey))
{
    builder.Services.AddKernel()
        .AddOpenAIChatCompletion("gpt-4o", openAiKey);
}
else 
{
    // Fallback or empty kernel to avoid crash on startup, but chat will fail
    builder.Services.AddKernel(); 
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Map Feature Endpoints
app.MapClientEndpoints();
app.MapProjectEndpoints();
app.MapInvoiceEndpoints();
app.MapAiEndpoints();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
