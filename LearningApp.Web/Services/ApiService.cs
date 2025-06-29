using LearningApp.API.DTOs;
using LearningApp.Core.Models;

namespace LearningApp.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Topic>> GetTopicsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Topic>>("api/topics") ?? new();
    }

    public async Task<Topic?> GetTopicAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Topic>($"api/topics/{id}");
    }

    public async Task<ExerciseResult> SubmitSolutionAsync(int exerciseId, string code)
    {
        var request = new SubmitSolutionRequest { Code = code };
        var response = await _httpClient.PostAsJsonAsync
            ($"api/exercises/{exerciseId}/submit", request);
        
        return await response.Content.ReadFromJsonAsync<ExerciseResult>() ?? new();
    }
}

