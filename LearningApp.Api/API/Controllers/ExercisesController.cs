using LearningApp.Api.API.DTOs;
using LearningApp.Api.Core.Models;
using LearningApp.Api.Data;
using LearningApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.Api.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly LearningDbContext _context;
    private readonly IAIService _aiService;

    public ExercisesController(LearningDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Exercise>> GetExercise(int id)
    {
        var exercise = await _context.Exercises
            .Include(e => e.Topic)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (exercise == null) return NotFound();
        
        return Ok(exercise);
    }

    [HttpPost("{id}/hint")]
    public async Task<ActionResult<HintResponse>> GetHint(
        int id, [FromBody] HintRequest request)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        var hint = await _aiService.GenerateHintAsync(
            request.UserCode,
            exercise.Description,
            exercise.Solution
        );

        return Ok(new HintResponse { Hint = hint });
    }
    
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ExerciseResult>> SubmitSolution(
        int id, [FromBody] SubmitSolutionRequest request)
    {
        var exercise = await _context.Exercises
            .Include(e => e.Topic)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (exercise == null) return NotFound();

        var aiResult = await _aiService.CheckCodeAsync(
            request.Code, 
            exercise.Solution, 
            exercise.Description
        );

        return Ok(new ExerciseResult
        {
            IsCorrect = aiResult.IsCorrect,
            Message = aiResult.Feedback,
            Suggestions = aiResult.Suggestions,
            Score = aiResult.Score
        });
    }

    [HttpPost("generate")]
    public async Task<ActionResult<Exercise>> GenerateExercise([FromBody] GenerateExerciseRequest request)
    {
        var topic = await _context.Topics.FindAsync(request.TopicId);
        if (topic == null) return BadRequest("Topic not found");
        
        var generatedExercise = await _aiService.GenerateExerciseAsync(
            topic.Name,
            request.Difficulty,
            request.FocusArea
        );
        
        generatedExercise.TopicId = request.TopicId;
        _context.Exercises.Add(generatedExercise);
        await _context.SaveChangesAsync();
        
        var savedExercise = await _context.Exercises
            .Include(e => e.Topic)
            .FirstAsync(e => e.Id == generatedExercise.Id);

        return Ok(savedExercise);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExercise(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Упражнение удалено", id = id });
    }
}