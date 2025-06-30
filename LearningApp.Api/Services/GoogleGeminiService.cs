using System.Text.Json;
using LearningApp.Api.Core.Models;

namespace LearningApp.Api.Services;

public class GoogleGeminiService : IAIService
{
    private readonly  HttpClient _httpClient;
    private readonly ILogger<GoogleGeminiService> _logger;
    private readonly string _apiKey;

    public GoogleGeminiService(HttpClient httpClient, ILogger<GoogleGeminiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;   
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Google Gemini API key not configured");
        
        _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    }

    public async Task<AICodeCheckResult> CheckCodeAsync(string userCode, string expectedSolution, string exerciseDescription)
    {
        try
        {
            var prompt = $@"
Ты эксперт по C++. Проанализируй код студента и дай обратную связь на русском языке.

ЗАДАНИЕ: {exerciseDescription}

ОЖИДАЕМОЕ РЕШЕНИЕ:
```cpp
{expectedSolution}
```

КОД СТУДЕНТА:
```cpp
{userCode}
```

Проанализируй код и ответь ТОЛЬКО в JSON формате (без дополнительного текста):
{{
    ""isCorrect"": true/false,
    ""feedback"": ""подробная обратная связь на русском"",
    ""suggestions"": [""совет 1"", ""совет 2""],
    ""score"": число_от_0_до_100
}}

Критерии оценки:
- Правильность логики (40%)
- Соответствие заданию (30%)  
- Качество кода (20%)
- Стиль кодирования (10%)";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 1000,
                    topP = 0.8,
                    topK = 10
                }
            };

            var url = $"v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini API Error: {response.StatusCode} - {errorContent}");
                return GetFallbackResult(userCode, expectedSolution);
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var content = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";

            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                var aiResult = JsonSerializer.Deserialize<AICodeCheckResult>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return aiResult ?? GetFallbackResult(userCode, expectedSolution);
            }

            return GetFallbackResult(userCode, expectedSolution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking code with Gemini AI");
            return GetFallbackResult(userCode, expectedSolution);
        }
    }

    public async Task<Exercise> GenerateExerciseAsync(string topicName, int difficulty, string? focusArea = null)
    {
        try
        {
            var focusText = !string.IsNullOrEmpty(focusArea) ? $"\nОсобое внимание: {focusArea}" : "";
            
            var prompt = $@"
Создай упражнение по C++ для темы ""{topicName}"" с уровнем сложности {difficulty} (1-5).
{focusText}

ВАЖНО! Четко разделяй starterCode и solution:
- starterCode: ТОЛЬКО базовая структура C++ с комментариями-подсказками
- solution: ПОЛНОЕ работающее решение

Ответь ТОЛЬКО в JSON формате:
{{
    ""title"": ""название упражнения"",
    ""description"": ""подробное описание задания"",
    ""starterCode"": ""#include <iostream>\nusing namespace std;\n\nint main() {{\n    // Ваш код здесь\n    // Подсказка: что нужно сделать\n    \n    return 0;\n}}"",
    ""solution"": ""полное правильное решение с #include и всем кодом"",
    ""expectedOutput"": ""ожидаемый вывод программы"",
    ""difficulty"": {difficulty}
}}

Пример правильного starterCode:
#include <iostream>
using namespace std;

int main() {{
    // Объявите переменную
    // Присвойте ей значение  
    // Выведите результат
    
    return 0;
}}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 1500,
                    topP = 0.9,
                    topK = 20
                }
            };

            var url = $"v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini API Error: {response.StatusCode} - {errorContent}");
                return GetFallbackExercise(topicName, difficulty);
            }
            
            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var content = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";
            
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                var exerciseData = JsonSerializer.Deserialize<ExerciseGenerationResult>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new Exercise
                {
                    Title = exerciseData?.Title ?? $"Упражнение по теме: {topicName}",
                    Description = exerciseData?.Description ?? "Создайте простую программу на C++",
                    StarterCode = CleanStarterCode(exerciseData?.StarterCode) ?? GetDefaultStarterCode(), // ИЗМЕНИТЬ
                    Solution = exerciseData?.Solution ?? GetDefaultSolution(),
                    ExpectedOutput = exerciseData?.ExpectedOutput ?? "Hello World!",
                    Difficulty = exerciseData?.Difficulty ?? difficulty
                };
            }

            return GetFallbackExercise(topicName, difficulty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating exercise with Gemini AI");
            return GetFallbackExercise(topicName, difficulty);
        }
    }

    public async Task<string> GenerateHintAsync(string userCode, string exerciseDescription, string correctSolution)
    {
        try
        {
            var prompt = $@"
Студент решает задачу по C++, но у него не получается. Дай короткую подсказку на русском языке (не более 2-3 предложений).

ЗАДАНИЕ: {exerciseDescription}

КОД СТУДЕНТА:
```cpp
{userCode}
```

ПРАВИЛЬНОЕ РЕШЕНИЕ (НЕ ПОКАЗЫВАЙ ЕГО СТУДЕНТУ):
```cpp
{correctSolution}
```

Дай только подсказку, которая поможет студенту найти правильное направление, но не раскрывает полное решение. Отвечай только текстом подсказки, без дополнительного форматирования.";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.5,
                    maxOutputTokens = 200,
                    topP = 0.8,
                    topK = 10
                }
            };

            var url = $"v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                return "Проверьте синтаксис и логику вашего кода.";
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var hint = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";
            
            return string.IsNullOrWhiteSpace(hint) ? "Попробуйте проверить логику вашего кода." : hint.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hint with Gemini AI");
            return "Проверьте синтаксис и логику вашего кода.";
        }
    }

    private AICodeCheckResult GetFallbackResult(string userCode, string expectedSolution)
    {
        var isCorrect = userCode.Trim().Equals(expectedSolution.Trim(), StringComparison.OrdinalIgnoreCase);
        return new AICodeCheckResult
        {
            IsCorrect = isCorrect,
            Feedback = isCorrect ? "Код выглядит правильно!" : "Код не совпадает с ожидаемым решением. Проверьте логику.",
            Score = isCorrect ? 100 : 0,
            Suggestions = isCorrect ? new() : new() { "Проверьте синтаксис", "Сравните с примером решения" }
        };
    }
    
    private Exercise GetFallbackExercise(string topicName, int difficulty)
    {
        return new Exercise
        {
            Title = $"Упражнение по теме: {topicName}",
            Description = "Создайте простую программу на C++, которая выводит приветствие.",
            StarterCode = GetDefaultStarterCode(),
            Solution = GetDefaultSolution(),
            ExpectedOutput = "Hello World!",
            Difficulty = difficulty
        };
    }
    
    private string GetDefaultStarterCode()
    {
        return @"#include <iostream>
using namespace std;

int main() {
    // Напишите ваш код здесь
    // Следуйте инструкциям в условии задачи
    
    return 0;
}";
    }
    
    private string GetDefaultSolution()
    {
        return @"#include <iostream>
using namespace std;

int main() {
    cout << ""Hello World!"" << endl;
    return 0;
}";
    }
    
    private string CleanStarterCode(string? starterCode)
    {
        if (string.IsNullOrEmpty(starterCode)) return GetDefaultStarterCode();
    
        if (starterCode.Contains("cout <<") && starterCode.Split('\n').Length > 10)
        {
            return GetDefaultStarterCode();
        }
    
        return starterCode;
    }
    
    private class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private class GeminiPart
    {
        public string? Text { get; set; }
    }

    private class ExerciseGenerationResult
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? StarterCode { get; set; }
        public string? Solution { get; set; }
        public string? ExpectedOutput { get; set; }
        public int Difficulty { get; set; }
    }
}