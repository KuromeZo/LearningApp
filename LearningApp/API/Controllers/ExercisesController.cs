using LearningApp.API.DTOs;
using LearningApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly LearningDbContext _context;

    public ExercisesController(LearningDbContext context)
    {
        _context = context;
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ExerciseResult>> SubmitSolution(
        int id, [FromBody] SubmitSolutionRequest request)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        var isCorrect = request.Code.Trim() == exercise.Solution.Trim();

        return Ok(new ExerciseResult
        {
            IsCorrect = isCorrect,
            Message = isCorrect ? "Правильно!" : "Попробуйте ещё раз"
        });
    }
    
}