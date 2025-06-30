namespace LearningApp.Api.API.DTOs;

public class ExerciseResult
{
    public bool IsCorrect { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public int Score { get; set; }
}