namespace LearningApp.Api.Services;

public class AICodeCheckResult
{
    public bool IsCorrect { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public int Score { get; set; } // 0-100
}