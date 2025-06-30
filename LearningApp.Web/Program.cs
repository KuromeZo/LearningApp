using LearningApp.Web.Components;
using LearningApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiUrl = Environment.GetEnvironmentVariable("API_URL") 
             ?? builder.Configuration.GetValue<string>("ApiUrl")
             ?? "http://localhost:5000/";

if (!apiUrl.EndsWith("/"))
{
    apiUrl += "/";
}

Console.WriteLine($"Using API URL: {apiUrl}");

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "LearningApp.Web/1.0");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", () => new { 
    Status = "Web App Working", 
    Time = DateTime.Now,
    Environment = app.Environment.EnvironmentName,
    ApiUrl = apiUrl
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = $"http://0.0.0.0:{port}";

Console.WriteLine($"Starting application on {url}");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"API URL: {apiUrl}");

app.Run(url);