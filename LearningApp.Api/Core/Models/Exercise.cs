namespace LearningApp.Core.Models;

public class Exercise
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StarterCode { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public Topic Topic { get; set; } = null!;
}