using System.Text.Json.Serialization;
using LearningApp.Api.Data;
using LearningApp.Api.Services;
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

builder.Services.AddHttpClient<IAIService, GoogleGeminiService>();
builder.Services.AddScoped<IAIService, GoogleGeminiService>();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("Using PostgreSQL from DATABASE_URL");
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        if (userInfo.Length != 2)
        {
            throw new InvalidOperationException("Invalid DATABASE_URL format - missing user info");
        }
        
        var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        
        Console.WriteLine($"Parsed connection string: Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password=***");
        builder.Services.AddDbContext<LearningDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        Console.WriteLine($"DATABASE_URL format should be: postgres://user:password@host:port/database");
        throw;
    }
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
        
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")))
        {
            Console.WriteLine("Production: Ensuring database created with correct PostgreSQL settings...");
            context.Database.EnsureCreated();
        }
        else
        {
            context.Database.Migrate();
        }
        
        Console.WriteLine("Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

app.MapGet("/", () => new { 
    Status = "API Working with Google Gemini AI", 
    Time = DateTime.Now,
    Environment = app.Environment.EnvironmentName,
    Swagger = "/swagger",
    Features = new[] { "Exercise Generation", "AI Code Review", "Smart Hints" },
    AIProvider = "Google Gemini (FREE)"
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