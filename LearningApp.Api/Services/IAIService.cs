using LearningApp.Api.Core.Models;

namespace LearningApp.Api.Services;

public interface IAIService
{
    Task<AICodeCheckResult> CheckCodeAsync(string userCode, string expectedSolution, string exerciseDescription);
    Task<Exercise> GenerateExerciseAsync(string topicName, int difficulty, string? focusArea = null);
    Task<string> GenerateHintAsync(string userCode, string exerciseDescription, string correctSolution);
}