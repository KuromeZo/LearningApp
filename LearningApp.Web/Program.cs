using LearningApp.Web.Components;
using LearningApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiUrl = Environment.GetEnvironmentVariable("API_URL") 
             ?? builder.Configuration.GetValue<string>("ApiUrl")
             ?? "https://localhost:7031/";

Console.WriteLine($"Using API URL: {apiUrl}");

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Starting application on port {port}");
app.Run($"http://0.0.0.0:{port}");