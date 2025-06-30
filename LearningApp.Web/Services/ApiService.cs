using System.Text.Json;
using LearningApp.Web.Models;

namespace LearningApp.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Topic>> GetTopicsAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to fetch topics from API...");
            var response = await _httpClient.GetAsync("api/topics");
            
            _logger.LogInformation($"API Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                return new List<Topic>();
            }
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"API Response Content Length: {content.Length}");
            
            var topics = await response.Content.ReadFromJsonAsync<List<Topic>>() ?? new();
            return topics;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Request Exception when fetching topics");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout when fetching topics");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error when fetching topics");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching topics");
            throw;
        }
    }

    public async Task<Topic?> GetTopicAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Fetching topic with ID: {id}");
            var response = await _httpClient.GetAsync($"api/topics/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Topic {id} not found: {response.StatusCode}");
                return null;
            }
            
            var topic = await response.Content.ReadFromJsonAsync<Topic>();
            _logger.LogInformation($"Successfully fetched topic: {topic?.Name}");
            return topic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching topic {id}");
            throw;
        }
    }

    public async Task<ExerciseResult> SubmitSolutionAsync(int exerciseId, string code)
    {
        try
        {
            _logger.LogInformation($"Submitting solution for exercise {exerciseId}");
            var request = new SubmitSolutionRequest { Code = code };
            var response = await _httpClient.PostAsJsonAsync($"api/exercises/{exerciseId}/submit", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to submit solution: {response.StatusCode}");
                return new ExerciseResult { IsCorrect = false, Message = "Ошибка при отправке решения" };
            }

            var result = await response.Content.ReadFromJsonAsync<ExerciseResult>() ?? new();
            _logger.LogInformation($"Solution submitted. Result: {result.IsCorrect}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error submitting solution for exercise {exerciseId}");
            throw;
        }
    }

    public async Task<Exercise?> GetExerciseAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Fetching exercise with ID: {id}");
            var response = await _httpClient.GetAsync($"api/exercises/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Exercise {id} not found: {response.StatusCode}");
                return null;
            }

            var exercise = await response.Content.ReadFromJsonAsync<Exercise>();
            _logger.LogInformation($"Successfully fetched exercise: {exercise?.Title}");
            return exercise;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching exercise {id}");
            throw;
        }
    }
}
