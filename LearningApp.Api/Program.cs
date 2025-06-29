using System.Collections;
using System.Text.Json.Serialization;
using LearningApp.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

Console.WriteLine("=== ДИАГНОСТИКА DATABASE_URL ===");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL length: {databaseUrl?.Length ?? -1}");
Console.WriteLine($"DATABASE_URL is null: {databaseUrl == null}");
Console.WriteLine($"DATABASE_URL is empty: {string.IsNullOrEmpty(databaseUrl)}");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine($"DATABASE_URL first 50 chars: {databaseUrl.Substring(0, Math.Min(50, databaseUrl.Length))}...");
}

var configConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Config DefaultConnection: {configConnectionString}");

Console.WriteLine("=== ALL DATABASE VARIABLES ===");
foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    var key = env.Key.ToString();
    if (key?.Contains("DATABASE", StringComparison.OrdinalIgnoreCase) == true)
    {
        Console.WriteLine($"{key} = {env.Value}");
    }
}
Console.WriteLine("=== END ДИАГНОСТИКА ===");

if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("Using PostgreSQL from DATABASE_URL");
    builder.Services.AddDbContext<LearningDbContext>(options =>
        options.UseNpgsql(databaseUrl));
}
else
{
    Console.WriteLine("Using SQLite from config (DATABASE_URL is empty!)");
    builder.Services.AddDbContext<LearningDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
    try
    {
        Console.WriteLine("Attempting to create database...");
        context.Database.EnsureCreated();
        Console.WriteLine("Database creation successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database creation error: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
    }
}

app.MapGet("/", () => new { 
    Status = "API Working", 
    Time = DateTime.Now,
    Environment = app.Environment.EnvironmentName,
    Swagger = "/swagger"
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
var url = $"http://0.0.0.0:{port}";

Console.WriteLine($"API starting on: {url}");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

app.Run(url);