namespace LearningApp.Api.API.DTOs;

public class GenerateExerciseRequest
{
    public int TopicId { get; set; }
    public int Difficulty { get; set; } = 1;
    public string? FocusArea { get; set; }
}