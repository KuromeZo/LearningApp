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

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Using PostgreSQL from DATABASE_URL");
    builder.Services.AddDbContext<LearningDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("Using SQLite for local development");
    builder.Services.AddDbContext<LearningDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
    try
    {
        Console.WriteLine("Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
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